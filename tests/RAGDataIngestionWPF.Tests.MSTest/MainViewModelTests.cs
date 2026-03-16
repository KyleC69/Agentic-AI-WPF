// Build Date: 2026/03/16
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         MainViewModelTests.cs
// Author: GitHub Copilot

using System.Collections.Specialized;

using DataIngestionLib.Contracts.Services;

using Microsoft.Extensions.AI;

using Moq;

using RAGDataIngestionWPF.ViewModels;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class MainViewModelTests
{
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
}
