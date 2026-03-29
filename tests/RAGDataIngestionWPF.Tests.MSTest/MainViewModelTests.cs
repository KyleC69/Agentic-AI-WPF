// Build Date: 2026/03/16
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         MainViewModelTests.cs
// Author: GitHub Copilot

using System.Collections.Specialized;

using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;

using Microsoft.Extensions.AI;

using Moq;

using RAGDataIngestionWPF.ViewModels;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class MainViewModelTests
{
    [TestMethod]
    public void ConstructorWithNullServiceThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => _ = new MainViewModel(null!));
    }

    [TestMethod]
    public async Task WhenSendMessageCompletesThenMessagesCollectionRaisesAddNotifications()
    {
        var chatConversationServiceMock = new Mock<IChatConversationService>();
        chatConversationServiceMock.SetupGet(service => service.ContextTokenCount).Returns(8);
        chatConversationServiceMock
                .Setup(service => service.SendRequestToModelAsync("hello", It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<ChatMessage>(new ChatMessage(ChatRole.Assistant, "hi there")));

        MainViewModel viewModel = new(chatConversationServiceMock.Object)
        {
                MessageInput = "hello"
        };
        var addNotificationCount = 0;
        viewModel.Messages.CollectionChanged += (_, args) =>
        {
            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                addNotificationCount += args.NewItems?.Count ?? 0;
            }
        };

        await viewModel.SendMessageCommand.ExecuteAsync(null);

        Assert.AreEqual(2, addNotificationCount);
    }

    [TestMethod]
    public async Task WhenSendMessageCompletesThenAssistantMessageIsProjectedForUiBinding()
    {
        var chatConversationServiceMock = new Mock<IChatConversationService>();
        chatConversationServiceMock.SetupGet(service => service.ContextTokenCount).Returns(8);
        chatConversationServiceMock
                .Setup(service => service.SendRequestToModelAsync("hello", It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<ChatMessage>(new ChatMessage(ChatRole.Assistant, "hi there")));

        MainViewModel viewModel = new(chatConversationServiceMock.Object)
        {
                MessageInput = "hello"
        };

        await viewModel.SendMessageCommand.ExecuteAsync(null);

        Assert.AreEqual("hi there", viewModel.Messages[1].Text);
        Assert.IsFalse(viewModel.Messages[1].IsUser);
        Assert.AreEqual(ChatRole.Assistant.ToString(), viewModel.Messages[1].Role);
    }

    [TestMethod]
    public async Task WhenSendMessageIsCancelledThenCancellationMessageIsAdded()
    {
        var chatConversationServiceMock = new Mock<IChatConversationService>();
        chatConversationServiceMock.SetupGet(service => service.ContextTokenCount).Returns(14);
        chatConversationServiceMock.SetupGet(service => service.SessionTokenCount).Returns(6);
        chatConversationServiceMock.SetupGet(service => service.RagTokenCount).Returns(3);
        chatConversationServiceMock.SetupGet(service => service.ToolTokenCount).Returns(2);
        chatConversationServiceMock.SetupGet(service => service.SystemTokenCount).Returns(1);
        chatConversationServiceMock
                .Setup(service => service.SendRequestToModelAsync("hello", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

        MainViewModel viewModel = new(chatConversationServiceMock.Object)
        {
                MessageInput = "hello"
        };

        await viewModel.SendMessageCommand.ExecuteAsync(null);

        Assert.AreEqual(2, viewModel.Messages.Count);
        Assert.AreEqual("Response cancelled.", viewModel.Messages[1].Text);
        Assert.AreEqual(14, viewModel.TotalTokenCount);
        Assert.AreEqual(6, viewModel.SessionTokenCount);
        Assert.AreEqual(3, viewModel.RagTokenCount);
        Assert.AreEqual(2, viewModel.ToolTokenCount);
        Assert.AreEqual(1, viewModel.SystemTokenCount);
    }

    [TestMethod]
    public void BusyStateChangedEventUpdatesBusyAndCommandExecutability()
    {
        var chatConversationServiceMock = new Mock<IChatConversationService>();
        MainViewModel viewModel = new(chatConversationServiceMock.Object)
        {
                MessageInput = "send"
        };

        chatConversationServiceMock.Raise(service => service.BusyStateChanged += null, chatConversationServiceMock.Object, true);

        Assert.IsTrue(viewModel.IsBusy);
        Assert.IsFalse(viewModel.SendMessageCommand.CanExecute(null));
        Assert.IsTrue(viewModel.CancelMessageCommand.CanExecute(null));

        chatConversationServiceMock.Raise(service => service.BusyStateChanged += null, chatConversationServiceMock.Object, false);

        Assert.IsFalse(viewModel.IsBusy);
        Assert.IsTrue(viewModel.SendMessageCommand.CanExecute(null));
        Assert.IsFalse(viewModel.CancelMessageCommand.CanExecute(null));
    }

    [TestMethod]
    public void TokenUsageUpdatedEventUpdatesTokenPropertiesInRealtime()
    {
        var chatConversationServiceMock = new Mock<IChatConversationService>();
        MainViewModel viewModel = new(chatConversationServiceMock.Object);

        var usageSnapshot = new TokenUsageSnapshot(
                TotalTokens: 32,
                SessionTokens: 13,
                RagTokens: 8,
                ToolTokens: 7,
                SystemTokens: 4,
                InputTokens: 10,
                OutputTokens: 12,
                CachedInputTokens: 3,
                ReasoningTokens: 2,
                Source: "test",
                UpdatedAtUtc: DateTimeOffset.UtcNow,
                AdditionalCounts: new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase));

        chatConversationServiceMock.Raise(service => service.TokenUsageUpdated += null, chatConversationServiceMock.Object, usageSnapshot);

        Assert.AreEqual(32, viewModel.TotalTokenCount);
        Assert.AreEqual(13, viewModel.SessionTokenCount);
        Assert.AreEqual(8, viewModel.RagTokenCount);
        Assert.AreEqual(7, viewModel.ToolTokenCount);
        Assert.AreEqual(4, viewModel.SystemTokenCount);
        Assert.AreEqual(10, viewModel.InputTokenCount);
        Assert.AreEqual(12, viewModel.OutputTokenCount);
        Assert.AreEqual(3, viewModel.CachedInputTokenCount);
        Assert.AreEqual(2, viewModel.ReasoningTokenCount);
    }

    [TestMethod]
    public void SendMessageCommandCannotExecuteForWhitespaceInput()
    {
        var chatConversationServiceMock = new Mock<IChatConversationService>();
        MainViewModel viewModel = new(chatConversationServiceMock.Object)
        {
                MessageInput = "   "
        };

        Assert.IsFalse(viewModel.SendMessageCommand.CanExecute(null));
    }

    [TestMethod]
    public void OnNavigatedToLoadsPersistedConversationIntoUi()
    {
        var chatConversationServiceMock = new Mock<IChatConversationService>();
        IReadOnlyList<ChatMessage> persistedMessages =
        [
                new ChatMessage(ChatRole.User, "previous question"),
                new ChatMessage(ChatRole.Assistant, "previous answer")
        ];

        chatConversationServiceMock
                .Setup(service => service.LoadConversationHistoryAsync(It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<IReadOnlyList<ChatMessage>>(persistedMessages));
        chatConversationServiceMock.SetupGet(service => service.ContextTokenCount).Returns(22);
        chatConversationServiceMock.SetupGet(service => service.SessionTokenCount).Returns(11);
        chatConversationServiceMock.SetupGet(service => service.RagTokenCount).Returns(5);
        chatConversationServiceMock.SetupGet(service => service.ToolTokenCount).Returns(4);
        chatConversationServiceMock.SetupGet(service => service.SystemTokenCount).Returns(2);

        MainViewModel viewModel = new(chatConversationServiceMock.Object);

        viewModel.OnNavigatedTo(null!);

        Assert.AreEqual(2, viewModel.Messages.Count);
        Assert.AreEqual("previous question", viewModel.Messages[0].Text);
        Assert.AreEqual("previous answer", viewModel.Messages[1].Text);
        Assert.AreEqual(22, viewModel.TotalTokenCount);
        Assert.AreEqual(11, viewModel.SessionTokenCount);
        Assert.AreEqual(5, viewModel.RagTokenCount);
        Assert.AreEqual(4, viewModel.ToolTokenCount);
        Assert.AreEqual(2, viewModel.SystemTokenCount);
    }
}
