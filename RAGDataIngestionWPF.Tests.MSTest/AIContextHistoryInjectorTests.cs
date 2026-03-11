// Build Date: 2026/03/11
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         AIContextHistoryInjectorTests.cs
// Author: Kyle L. Crowder
// Build Num: 105603



using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;
using DataIngestionLib.Options;
using DataIngestionLib.Services.ContextInjectors;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

using Moq;




namespace RAGDataIngestionWPF.Tests.MSTest;





/// <summary>
///     Unit tests for <see cref="AIContextHistoryInjector" /> verifying context windowing,
///     message filtering, pruning, and lifecycle management.
/// </summary>
[TestClass]
public class AIContextHistoryInjectorTests
{




    [TestMethod]
    public async Task BuildContextMessagesAsync_DeduplicatesRequestMessages()
    {
        PersistedChatMessage persisted = MakeMessage("user", "Hello");

        Mock<IChatHistoryProvider> providerMock = new();
        providerMock
                .Setup(p => p.GetMessagesAsync("conv-1", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync([persisted]);

        AIContextHistoryInjector injector = new(
                providerMock.Object,
                CreateOptions(new ChatHistoryOptions { MaxContextMessages = 10 }));

        // Same message content in current request — should be de-duplicated
        ChatHistory currentRequest = new();
        currentRequest.AddUserMessage("Hello");

        var result = await injector.BuildContextMessagesAsync(
                "conv-1",
                currentRequest,
                CancellationToken.None);

        Assert.AreEqual(0, result.Count(), "Duplicate request messages should be filtered from history.");
    }








    [TestMethod]
    public async Task BuildContextMessagesAsync_RespectsMaxContextMessages()
    {
        var messages = Enumerable
                .Range(1, 15)
                .Select(i => MakeMessage("user", $"Message {i}", DateTimeOffset.UtcNow.AddMinutes(i)))
                .ToList();

        var providerMock = new Mock<IChatHistoryProvider>();
        providerMock
                .Setup(p => p.GetMessagesAsync("conv-1", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(messages);

        AIContextHistoryInjector injector = new AIContextHistoryInjector(providerMock.Object, CreateOptions(new ChatHistoryOptions { MaxContextMessages = 5 }));

        var result = await injector.BuildContextMessagesAsync(
                "conv-1",
                new ChatHistory(),
                CancellationToken.None);

        Assert.AreEqual(5, result.Count(), "Window should be capped at MaxContextMessages.");
    }








    [TestMethod]
    public async Task BuildContextMessagesAsync_ReturnsEmpty_WhenNoHistoryExists()
    {
        var providerMock = new Mock<IChatHistoryProvider>();
        providerMock
                .Setup(p => p.GetMessagesAsync("conv-1", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

        AIContextHistoryInjector injector = new AIContextHistoryInjector(providerMock.Object, CreateOptions(new ChatHistoryOptions { MaxContextMessages = 10 }));

        var result = await injector.BuildContextMessagesAsync(
                "conv-1",
                new ChatHistory(),
                CancellationToken.None);

        Assert.AreEqual(0, result.Count());
    }








    [TestMethod]
    public async Task BuildContextMessagesAsync_SkipsMessagesWithEmptyContent()
    {
        List<PersistedChatMessage> messages =
        [
                MakeMessage("user", "Valid message"),
                MakeMessage("user", string.Empty),
                MakeMessage("user", "  ")
        ];

        var providerMock = new Mock<IChatHistoryProvider>();
        providerMock
                .Setup(p => p.GetMessagesAsync("conv-1", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(messages);

        AIContextHistoryInjector injector = new AIContextHistoryInjector(providerMock.Object, CreateOptions(new ChatHistoryOptions { MaxContextMessages = 10 }));

        var result = await injector.BuildContextMessagesAsync(
                "conv-1",
                new ChatHistory(),
                CancellationToken.None);

        Assert.AreEqual(1, result.Count(), "Messages with empty or whitespace content should be excluded.");
    }








    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public async Task BuildContextMessagesAsync_ThrowsArgumentException_WhenConversationIdIsNullOrWhiteSpace(string? conversationId)
    {
        var providerMock = new Mock<IChatHistoryProvider>();

        AIContextHistoryInjector injector = new AIContextHistoryInjector(providerMock.Object, CreateOptions(new ChatHistoryOptions()));

        await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
                await injector.BuildContextMessagesAsync(conversationId!, new ChatHistory()));
    }








    private static IOptionsMonitor<ChatHistoryOptions> CreateOptions(ChatHistoryOptions options)
    {
        var mock = new Mock<IOptionsMonitor<ChatHistoryOptions>>();
        mock.SetupGet(m => m.CurrentValue).Returns(options);
        return mock.Object;
    }








    private static PersistedChatMessage MakeMessage(string role, string content, DateTimeOffset? timestamp = null, Guid? id = null)
    {
        return new PersistedChatMessage
        {
                MessageId = id ?? Guid.NewGuid(),
                ConversationId = "conv-1",
                SessionId = "session-1",
                AgentId = "agent-1",
                UserId = "user-1",
                ApplicationId = "app-1",
                Role = role,
                Content = content,
                TimestampUtc = timestamp ?? DateTimeOffset.UtcNow
        };
    }








    [TestMethod]
    public async Task PruneConversationAsync_RemovesOldestMessagesWhenOverLimit()
    {
        var messages = Enumerable
                .Range(1, 10)
                .Select(i => MakeMessage("user", $"Msg {i}", DateTimeOffset.UtcNow.AddMinutes(i)))
                .ToList();

        List<Guid> deleted = [];

        var providerMock = new Mock<IChatHistoryProvider>();
        providerMock
                .Setup(p => p.GetMessagesAsync("conv-1", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(messages);

        providerMock
                .Setup(p => p.DeleteMessageAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Callback<Guid, CancellationToken>((id, _) => deleted.Add(id))
                .ReturnsAsync(true);

        AIContextHistoryInjector injector = new AIContextHistoryInjector(providerMock.Object, CreateOptions(new ChatHistoryOptions { MaxContextMessages = 6 }));

        var removed = await injector.PruneConversationAsync("conv-1", CancellationToken.None);

        Assert.AreEqual(4, removed, "4 oldest messages should be pruned when limit is 6 out of 10.");
        // The oldest 4 messages should be the ones deleted
        var expectedDeleted = messages.Take(4).Select(m => m.MessageId);
        CollectionAssert.AreEquivalent(expectedDeleted.ToList(), deleted);
    }








    [TestMethod]
    public async Task PruneConversationAsync_ReturnsZero_WhenUnderLimit()
    {
        var messages = Enumerable
                .Range(1, 3)
                .Select(i => MakeMessage("user", $"Msg {i}"))
                .ToList();

        var providerMock = new Mock<IChatHistoryProvider>();
        providerMock
                .Setup(p => p.GetMessagesAsync("conv-1", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(messages);

        AIContextHistoryInjector injector = new AIContextHistoryInjector(providerMock.Object, CreateOptions(new ChatHistoryOptions { MaxContextMessages = 10 }));

        var removed = await injector.PruneConversationAsync("conv-1", CancellationToken.None);

        Assert.AreEqual(0, removed, "No messages should be pruned when under the limit.");
    }








    [TestMethod]
    public async Task StoreMessagesAsync_DoesNotPersistEmptyMessages()
    {
        List<PersistedChatMessage> stored = [];

        var providerMock = new Mock<IChatHistoryProvider>();
        providerMock
                .Setup(p => p.CreateMessageAsync(It.IsAny<PersistedChatMessage>(), It.IsAny<CancellationToken>()))
                .Callback<PersistedChatMessage, CancellationToken>((msg, _) => stored.Add(msg))
                .ReturnsAsync((PersistedChatMessage msg, CancellationToken _) => msg);

        providerMock
                .Setup(p => p.GetMessagesAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

        AIContextHistoryInjector injector = new AIContextHistoryInjector(providerMock.Object, CreateOptions(new ChatHistoryOptions { MaxContextMessages = 100 }));

        ChatHistory request = new ChatHistory();
        request.Add(new AIChatMessage(ChatRole.User, string.Empty));

        await injector.StoreMessagesAsync(
                "conv-1", "session-1", "agent-1", "user-1", "app-1",
                request, new ChatHistory(), CancellationToken.None);

        Assert.AreEqual(0, stored.Count, "Messages with empty content should not be persisted.");
    }








    [TestMethod]
    public async Task StoreMessagesAsync_DoesNotPersistSystemMessages()
    {
        List<PersistedChatMessage> stored = [];

        var providerMock = new Mock<IChatHistoryProvider>();
        providerMock
                .Setup(p => p.CreateMessageAsync(It.IsAny<PersistedChatMessage>(), It.IsAny<CancellationToken>()))
                .Callback<PersistedChatMessage, CancellationToken>((msg, _) => stored.Add(msg))
                .ReturnsAsync((PersistedChatMessage msg, CancellationToken _) => msg);

        providerMock
                .Setup(p => p.GetMessagesAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

        AIContextHistoryInjector injector = new AIContextHistoryInjector(providerMock.Object, CreateOptions(new ChatHistoryOptions { MaxContextMessages = 100 }));

        ChatHistory request = new ChatHistory();
        request.AddSystemMessage("You are a helpful assistant.");
        request.AddUserMessage("Hello");

        await injector.StoreMessagesAsync(
                "conv-1", "session-1", "agent-1", "user-1", "app-1",
                request, new ChatHistory(), CancellationToken.None);

        Assert.IsFalse(stored.Any(m => m.Role == "system"), "System messages should not be persisted.");
        Assert.AreEqual(1, stored.Count, "Only the user message should be persisted.");
    }








    [TestMethod]
    public async Task StoreMessagesAsync_PersistsUserAndAssistantMessages()
    {
        List<PersistedChatMessage> stored = [];

        var providerMock = new Mock<IChatHistoryProvider>();
        providerMock
                .Setup(p => p.CreateMessageAsync(It.IsAny<PersistedChatMessage>(), It.IsAny<CancellationToken>()))
                .Callback<PersistedChatMessage, CancellationToken>((msg, _) => stored.Add(msg))
                .ReturnsAsync((PersistedChatMessage msg, CancellationToken _) => msg);

        providerMock
                .Setup(p => p.GetMessagesAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(stored.AsReadOnly());

        AIContextHistoryInjector injector = new AIContextHistoryInjector(providerMock.Object, CreateOptions(new ChatHistoryOptions { MaxContextMessages = 100 }));

        ChatHistory request = new ChatHistory();
        request.AddUserMessage("What is the weather?");

        ChatHistory response = new ChatHistory();
        response.AddAssistantMessage("It is sunny.");

        await injector.StoreMessagesAsync(
                "conv-1", "session-1", "agent-1", "user-1", "app-1",
                request, response, CancellationToken.None);

        Assert.AreEqual(2, stored.Count, "Both request and response messages should be persisted.");
        Assert.IsTrue(stored.Any(m => m.Role == "user"), "User message should be stored.");
        Assert.IsTrue(stored.Any(m => m.Role == "assistant"), "Assistant message should be stored.");
    }
}