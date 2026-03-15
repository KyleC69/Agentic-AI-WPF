// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         ChatHistoryTests.cs
// Author: Kyle L. Crowder
// Build Num: 091003



using DataIngestionLib.Models;

using Microsoft.Extensions.AI;




namespace RAGDataIngestionWPF.Tests.MSTest;





/// <summary>
///     Unit tests for <see cref="AIChatHistory" /> verifying role-focused add helpers,
///     guard clauses, message retrieval, and token estimation heuristics.
/// </summary>
[TestClass]
public class ChatHistoryTests
{




    [TestMethod]
    public void AddAssistantMessageAddsMessageWithAssistantRole()
    {
        // Arrange
        AIChatHistory history = [];

        // Act
        history.AddAssistantMessage("It is sunny.");

        // Assert
        Assert.AreEqual(1, history.Count);
        Assert.AreEqual<ChatRole>(ChatRole.Assistant, history[0].Role);
        Assert.AreEqual("It is sunny.", history[0].Text);
    }








    [TestMethod]
    public void AddRangeAddsAllProvidedMessages()
    {
        // Arrange
        AIChatHistory history = [];
        List<AIChatMessage> messages =
        [
                new AIChatMessage(ChatRole.User, "first"),
                new AIChatMessage(ChatRole.Assistant, "second"),
                new AIChatMessage(ChatRole.User, "third")
        ];

        // Act
        history.AddRange(messages);

        // Assert
        Assert.AreEqual(3, history.Count);
    }








    [TestMethod]
    public void AddSystemMessageAddsMessageWithSystemRole()
    {
        // Arrange
        AIChatHistory history = [];

        // Act
        history.AddSystemMessage("You are a helpful assistant.");

        // Assert
        Assert.AreEqual(1, history.Count);
        Assert.AreEqual<ChatRole>(ChatRole.System, history[0].Role);
    }








    [TestMethod]
    public void AddUserMessageAddsMessageWithUserRole()
    {
        // Arrange
        AIChatHistory history = [];

        // Act
        history.AddUserMessage("What is the weather?");

        // Assert
        Assert.AreEqual(1, history.Count);
        Assert.AreEqual<ChatRole>(ChatRole.User, history[0].Role);
        Assert.AreEqual("What is the weather?", history[0].Text);
    }








    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void AddUserMessageWithNullOrWhitespaceContentThrowsArgumentException(string content)
    {
        // Arrange
        AIChatHistory history = [];

        // Act / Assert
        ArgumentException unused = Assert.ThrowsExactly<ArgumentException>(() => history.AddUserMessage(content!));
    }








    [TestMethod]
    public void EstimateContextTokenCountRespectsMaxTokenBudget()
    {
        // Arrange — add several long messages that individually exceed a small budget.
        // The heuristic estimates 1 token per 4 characters (Math.Max(1, text.Length / 4)),
        // so 400-character messages each cost ~100 tokens.
        AIChatHistory history = [];
        history.AddUserMessage(new('a', 400)); // ~100 tokens
        history.AddAssistantMessage(new('b', 400)); // ~100 tokens
        history.AddUserMessage(new('c', 400)); // ~100 tokens

        // Act — budget of 150 tokens should admit fewer than all three messages
        var tokens = history.EstimateContextTokenCount(150);

        // Assert
        Assert.IsLessThanOrEqualTo(150, tokens, "Token count must not exceed the configured budget.");
        Assert.IsGreaterThan(0, tokens, "At least one message should fit within the budget.");
    }








    [TestMethod]
    public void EstimateContextTokenCountWithNonPositiveMaxTokensThrowsArgumentOutOfRangeException()
    {
        // Arrange
        AIChatHistory history = [];
        history.AddUserMessage("some text");

        // Act / Assert
        ArgumentOutOfRangeException unused = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => history.EstimateContextTokenCount(0));
    }








    [TestMethod]
    public void EstimateTokenCountReturnsPositiveCountForNonEmptyHistory()
    {
        // Arrange
        AIChatHistory history = [];
        history.AddUserMessage("This is a reasonably long user message for token estimation.");

        // Act
        var tokens = history.EstimateTokenCount();

        // Assert
        Assert.IsGreaterThan(0, tokens, "Token count should be positive for non-empty text.");
    }








    [TestMethod]
    public void EstimateTokenCountReturnsZeroForEmptyHistory()
    {
        // Arrange
        AIChatHistory history = [];

        // Act
        var tokens = history.EstimateTokenCount();

        // Assert
        Assert.AreEqual(0, tokens);
    }








    [TestMethod]
    public void GetLastMessageTextReturnsCorrectTextForMatchingRole()
    {
        // Arrange
        AIChatHistory history = [];
        history.AddUserMessage("ask something");
        history.AddAssistantMessage("reply text");

        // Act
        var text = history.GetLastMessageText(ChatRole.Assistant);

        // Assert
        Assert.AreEqual("reply text", text);
    }








    [TestMethod]
    public void GetLastMessageTextReturnsEmptyStringWhenRoleNotPresent()
    {
        // Arrange
        AIChatHistory history = [];

        // Act
        var text = history.GetLastMessageText(ChatRole.User);

        // Assert
        Assert.AreEqual(string.Empty, text);
    }








    [TestMethod]
    public void LastMessageReturnsNewestMessage()
    {
        // Arrange
        AIChatHistory history = [];
        history.AddUserMessage("older message");
        history.AddAssistantMessage("newest message");

        // Act / Assert
        Assert.AreEqual("newest message", history.LastMessage?.Text);
    }








    [TestMethod]
    public void LastMessageReturnsNullWhenHistoryIsEmpty()
    {
        // Arrange
        AIChatHistory history = [];

        // Act / Assert
        Assert.IsNull(history.LastMessage);
    }








    [TestMethod]
    public void TryGetLastMessageReturnsFalseWhenRoleNotPresent()
    {
        // Arrange
        AIChatHistory history = [];
        history.AddUserMessage("user only");

        // Act
        var found = history.TryGetLastMessage(ChatRole.Assistant, out AIChatMessage message);

        // Assert
        Assert.IsFalse(found);
        Assert.IsNull(message);
    }








    [TestMethod]
    public void TryGetLastMessageReturnsLatestMessageMatchingRole()
    {
        // Arrange
        AIChatHistory history = [];
        history.AddUserMessage("First user message");
        history.AddAssistantMessage("First assistant reply");
        history.AddUserMessage("Second user message");

        // Act
        var found = history.TryGetLastMessage(ChatRole.User, out AIChatMessage message);

        // Assert
        Assert.IsTrue(found);
        Assert.IsNotNull(message);
        Assert.AreEqual("Second user message", message.Text);
    }
}