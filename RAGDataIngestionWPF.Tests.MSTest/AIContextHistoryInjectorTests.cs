// Build Date: 2026/03/13
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         AIContextHistoryInjectorTests.cs
// Author: Kyle L. Crowder
// Build Num: 175103



using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;
using DataIngestionLib.Services.ContextInjectors;

using Microsoft.Extensions.AI;

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

        SetAppSetting("MaxContextMessages", "10");
        AIContextHistoryInjector injector = new(providerMock.Object);

        // Same message content in current request — should be de-duplicated
        AIChatHistory currentRequest = [];
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
        List<PersistedChatMessage> messages = Enumerable
                .Range(1, 15)
                .Select(i => MakeMessage("user", $"Message {i}", DateTimeOffset.UtcNow.AddMinutes(i)))
                .ToList();

        Mock<IChatHistoryProvider> providerMock = new();
        providerMock
                .Setup(p => p.GetMessagesAsync("conv-1", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(messages);

        SetAppSetting("MaxContextMessages", "5");
        AIContextHistoryInjector injector = new AIContextHistoryInjector(providerMock.Object);

        var result = await injector.BuildContextMessagesAsync(
                "conv-1",
                [],
                CancellationToken.None);

        Assert.AreEqual(5, result.Count(), "Window should be capped at MaxContextMessages.");
    }








    [TestMethod]
    public async Task BuildContextMessagesAsync_ReturnsEmpty_WhenNoHistoryExists()
    {
        Mock<IChatHistoryProvider> providerMock = new();
        providerMock
                .Setup(p => p.GetMessagesAsync("conv-1", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

        SetAppSetting("MaxContextMessages", "10");
        AIContextHistoryInjector injector = new AIContextHistoryInjector(providerMock.Object);

        var result = await injector.BuildContextMessagesAsync(
                "conv-1",
                [],
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

        Mock<IChatHistoryProvider> providerMock = new();
        providerMock
                .Setup(p => p.GetMessagesAsync("conv-1", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(messages);

        SetAppSetting("MaxContextMessages", "10");
        AIContextHistoryInjector injector = new AIContextHistoryInjector(providerMock.Object);

        var result = await injector.BuildContextMessagesAsync(
                "conv-1",
                [],
                CancellationToken.None);

        Assert.AreEqual(1, result.Count(), "Messages with empty or whitespace content should be excluded.");
    }








    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public async Task BuildContextMessagesAsync_ThrowsArgumentException_WhenConversationIdIsNullOrWhiteSpace(string conversationId)
    {
        Mock<IChatHistoryProvider> providerMock = new();

        SetAppSetting("MaxContextMessages", "40");
        AIContextHistoryInjector injector = new AIContextHistoryInjector(providerMock.Object);

        await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
                await injector.BuildContextMessagesAsync(conversationId!, []));
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
    public async Task PruneConversationAsync_ReturnsZero_WhenUnderLimit()
    {
        List<PersistedChatMessage> messages = Enumerable
                .Range(1, 3)
                .Select(i => MakeMessage("user", $"Msg {i}"))
                .ToList();

        Mock<IChatHistoryProvider> providerMock = new();
        providerMock
                .Setup(p => p.GetMessagesAsync("conv-1", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(messages);

        SetAppSetting("MaxContextMessages", "10");
        AIContextHistoryInjector injector = new AIContextHistoryInjector(providerMock.Object);

        var removed = await injector.PruneConversationAsync("conv-1", CancellationToken.None);

        Assert.AreEqual(0, removed, "No messages should be pruned when under the limit.");
    }








    private static void SetAppSetting(string key, string value)
    {
        System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
        if (config.AppSettings.Settings[key] is null)
        {
            config.AppSettings.Settings.Add(key, value);
        }
        else
        {
            config.AppSettings.Settings[key].Value = value;
        }

        config.Save(System.Configuration.ConfigurationSaveMode.Modified);
        System.Configuration.ConfigurationManager.RefreshSection("appSettings");
    }








    [TestMethod]
    public async Task StoreMessagesAsync_DoesNotPersistEmptyMessages()
    {
        List<PersistedChatMessage> stored = [];

        Mock<IChatHistoryProvider> providerMock = new();
        providerMock
                .Setup(p => p.CreateMessageAsync(It.IsAny<PersistedChatMessage>(), It.IsAny<CancellationToken>()))
                .Callback<PersistedChatMessage, CancellationToken>((msg, _) => stored.Add(msg))
                .ReturnsAsync((PersistedChatMessage msg, CancellationToken _) => msg);

        providerMock
                .Setup(p => p.GetMessagesAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

        SetAppSetting("MaxContextMessages", "100");
        AIContextHistoryInjector injector = new AIContextHistoryInjector(providerMock.Object);

        AIChatHistory request = [new AIChatMessage(ChatRole.User, string.Empty)];

        await injector.StoreMessagesAsync(
                "conv-1", "session-1", "agent-1", "user-1", "app-1",
                request, [], CancellationToken.None);

        Assert.AreEqual(0, stored.Count, "Messages with empty content should not be persisted.");
    }








    [TestMethod]
    public async Task StoreMessagesAsync_DoesNotPersistSystemMessages()
    {
        List<PersistedChatMessage> stored = [];

        Mock<IChatHistoryProvider> providerMock = new();
        providerMock
                .Setup(p => p.CreateMessageAsync(It.IsAny<PersistedChatMessage>(), It.IsAny<CancellationToken>()))
                .Callback<PersistedChatMessage, CancellationToken>((msg, _) => stored.Add(msg))
                .ReturnsAsync((PersistedChatMessage msg, CancellationToken _) => msg);

        providerMock
                .Setup(p => p.GetMessagesAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

        SetAppSetting("MaxContextMessages", "100");
        AIContextHistoryInjector injector = new AIContextHistoryInjector(providerMock.Object);

        AIChatHistory request = [];
        request.AddSystemMessage("You are a helpful assistant.");
        request.AddUserMessage("Hello");

        await injector.StoreMessagesAsync(
                "conv-1", "session-1", "agent-1", "user-1", "app-1",
                request, [], CancellationToken.None);

        Assert.IsFalse(stored.Any(m => m.Role == "system"), "System messages should not be persisted.");
        Assert.AreEqual(1, stored.Count, "Only the user message should be persisted.");
    }








    [TestMethod]
    public async Task StoreMessagesAsync_PersistsUserAndAssistantMessages()
    {
        List<PersistedChatMessage> stored = [];

        Mock<IChatHistoryProvider> providerMock = new();
        providerMock
                .Setup(p => p.CreateMessageAsync(It.IsAny<PersistedChatMessage>(), It.IsAny<CancellationToken>()))
                .Callback<PersistedChatMessage, CancellationToken>((msg, _) => stored.Add(msg))
                .ReturnsAsync((PersistedChatMessage msg, CancellationToken _) => msg);

        providerMock
                .Setup(p => p.GetMessagesAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(stored.AsReadOnly());

        SetAppSetting("MaxContextMessages", "100");
        AIContextHistoryInjector injector = new AIContextHistoryInjector(providerMock.Object);

        AIChatHistory request = [];
        request.AddUserMessage("What is the weather?");

        AIChatHistory response = [];
        response.AddAssistantMessage("It is sunny.");

        await injector.StoreMessagesAsync(
                "conv-1", "session-1", "agent-1", "user-1", "app-1",
                request, response, CancellationToken.None);

        Assert.AreEqual(2, stored.Count, "Both request and response messages should be persisted.");
        Assert.IsTrue(stored.Any(m => m.Role == "user"), "User message should be stored.");
        Assert.IsTrue(stored.Any(m => m.Role == "assistant"), "Assistant message should be stored.");
    }
}