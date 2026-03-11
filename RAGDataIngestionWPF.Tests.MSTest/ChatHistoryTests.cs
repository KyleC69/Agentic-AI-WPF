// 2026/03/10
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF.Tests.MSTest
//  File:         ChatHistoryTests.cs
//   Author: Kyle L. Crowder



using DataIngestionLib.Models;

using Microsoft.Extensions.AI;




namespace RAGDataIngestionWPF.Tests.MSTest;




/// <summary>
///     Unit tests for <see cref="ChatHistory" /> verifying role-focused add helpers,
///     guard clauses, message retrieval, and token estimation heuristics.
/// </summary>
[TestClass]
public class ChatHistoryTests
{
    [TestMethod]
    public void AddUserMessage_AddsMessageWithUserRole()
    {
        // Arrange
        ChatHistory history = new();

        // Act
        history.AddUserMessage("What is the weather?");

        // Assert
        Assert.AreEqual(1, history.Count);
        Assert.AreEqual<ChatRole>(ChatRole.User, history[0].Role);
        Assert.AreEqual("What is the weather?", history[0].Text);
    }




    [TestMethod]
    public void AddAssistantMessage_AddsMessageWithAssistantRole()
    {
        // Arrange
        ChatHistory history = new();

        // Act
        history.AddAssistantMessage("It is sunny.");

        // Assert
        Assert.AreEqual(1, history.Count);
        Assert.AreEqual<ChatRole>(ChatRole.Assistant, history[0].Role);
        Assert.AreEqual("It is sunny.", history[0].Text);
    }




    [TestMethod]
    public void AddSystemMessage_AddsMessageWithSystemRole()
    {
        // Arrange
        ChatHistory history = new();

        // Act
        history.AddSystemMessage("You are a helpful assistant.");

        // Assert
        Assert.AreEqual(1, history.Count);
        Assert.AreEqual<ChatRole>(ChatRole.System, history[0].Role);
    }




    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void AddUserMessage_WithNullOrWhitespaceContent_ThrowsArgumentException(string? content)
    {
        // Arrange
        ChatHistory history = new();

        // Act / Assert
        Assert.ThrowsExactly<ArgumentException>(() => history.AddUserMessage(content!));
    }




    [TestMethod]
    public void TryGetLastMessage_ReturnsLatestMessageMatchingRole()
    {
        // Arrange
        ChatHistory history = new();
        history.AddUserMessage("First user message");
        history.AddAssistantMessage("First assistant reply");
        history.AddUserMessage("Second user message");

        // Act
        bool found = history.TryGetLastMessage(ChatRole.User, out AIChatMessage? message);

        // Assert
        Assert.IsTrue(found);
        Assert.IsNotNull(message);
        Assert.AreEqual("Second user message", message.Text);
    }




    [TestMethod]
    public void TryGetLastMessage_ReturnsFalse_WhenRoleNotPresent()
    {
        // Arrange
        ChatHistory history = new();
        history.AddUserMessage("user only");

        // Act
        bool found = history.TryGetLastMessage(ChatRole.Assistant, out AIChatMessage? message);

        // Assert
        Assert.IsFalse(found);
        Assert.IsNull(message);
    }




    [TestMethod]
    public void GetLastMessageText_ReturnsCorrectText_ForMatchingRole()
    {
        // Arrange
        ChatHistory history = new();
        history.AddUserMessage("ask something");
        history.AddAssistantMessage("reply text");

        // Act
        string text = history.GetLastMessageText(ChatRole.Assistant);

        // Assert
        Assert.AreEqual("reply text", text);
    }




    [TestMethod]
    public void GetLastMessageText_ReturnsEmptyString_WhenRoleNotPresent()
    {
        // Arrange
        ChatHistory history = new();

        // Act
        string text = history.GetLastMessageText(ChatRole.User);

        // Assert
        Assert.AreEqual(string.Empty, text);
    }




    [TestMethod]
    public void EstimateTokenCount_ReturnsPositiveCount_ForNonEmptyHistory()
    {
        // Arrange
        ChatHistory history = new();
        history.AddUserMessage("This is a reasonably long user message for token estimation.");

        // Act
        int tokens = history.EstimateTokenCount();

        // Assert
        Assert.IsTrue(tokens > 0, "Token count should be positive for non-empty text.");
    }




    [TestMethod]
    public void EstimateTokenCount_ReturnsZero_ForEmptyHistory()
    {
        // Arrange
        ChatHistory history = new();

        // Act
        int tokens = history.EstimateTokenCount();

        // Assert
        Assert.AreEqual(0, tokens);
    }




    [TestMethod]
    public void EstimateContextTokenCount_RespectsMaxTokenBudget()
    {
        // Arrange — add several long messages that individually exceed a small budget.
        // The heuristic estimates 1 token per 4 characters (Math.Max(1, text.Length / 4)),
        // so 400-character messages each cost ~100 tokens.
        ChatHistory history = new();
        history.AddUserMessage(new string('a', 400));       // ~100 tokens
        history.AddAssistantMessage(new string('b', 400));  // ~100 tokens
        history.AddUserMessage(new string('c', 400));       // ~100 tokens

        // Act — budget of 150 tokens should admit fewer than all three messages
        int tokens = history.EstimateContextTokenCount(150);

        // Assert
        Assert.IsTrue(tokens <= 150, "Token count must not exceed the configured budget.");
        Assert.IsTrue(tokens > 0, "At least one message should fit within the budget.");
    }




    [TestMethod]
    public void EstimateContextTokenCount_WithNonPositiveMaxTokens_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        ChatHistory history = new();
        history.AddUserMessage("some text");

        // Act / Assert
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => history.EstimateContextTokenCount(0));
    }




    [TestMethod]
    public void AddRange_AddsAllProvidedMessages()
    {
        // Arrange
        ChatHistory history = new();
        List<AIChatMessage> messages =
        [
            new AIChatMessage(ChatRole.User, "first"),
            new AIChatMessage(ChatRole.Assistant, "second"),
            new AIChatMessage(ChatRole.User, "third"),
        ];

        // Act
        history.AddRange(messages);

        // Assert
        Assert.AreEqual(3, history.Count);
    }




    [TestMethod]
    public void LastMessage_ReturnsNull_WhenHistoryIsEmpty()
    {
        // Arrange
        ChatHistory history = new();

        // Act / Assert
        Assert.IsNull(history.LastMessage);
    }




    [TestMethod]
    public void LastMessage_ReturnsNewestMessage()
    {
        // Arrange
        ChatHistory history = new();
        history.AddUserMessage("older message");
        history.AddAssistantMessage("newest message");

        // Act / Assert
        Assert.AreEqual("newest message", history.LastMessage?.Text);
    }
}
