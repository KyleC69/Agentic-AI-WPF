// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         AIChatMessageTests.cs
// Author: Kyle L. Crowder
// Build Num: 091001



using DataIngestionLib.Models;

using Microsoft.Extensions.AI;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class AIChatMessageTests
{
    [TestMethod]
    public void AuthorNameNormalizesWhitespaceToNull()
    {
        AIChatMessage message = new AIChatMessage(ChatRole.User, "hello")
        {
                AuthorName = "   "
        };

        Assert.IsNull(message.AuthorName);
    }








    [TestMethod]
    public void CloneCreatesDistinctMessageWithSameCoreValues()
    {
        DateTimeOffset createdAt = new DateTimeOffset(2026, 3, 15, 0, 0, 0, TimeSpan.Zero);
        AIChatMessage originalMessage = new AIChatMessage(ChatRole.Assistant, "answer")
        {
                AuthorName = "Assistant",
                CreatedAt = createdAt,
                MessageId = "message-1"
        };

        AIChatMessage clone = originalMessage.Clone();

        Assert.AreNotSame(originalMessage, clone);
        Assert.AreEqual(originalMessage, clone);
        Assert.AreEqual("answer", clone.Text);
    }








    [TestMethod]
    public void EqualityUsesRoleTextAndIdentifiers()
    {
        DateTimeOffset createdAt = new DateTimeOffset(2026, 3, 15, 1, 0, 0, TimeSpan.Zero);
        AIChatMessage first = new AIChatMessage(ChatRole.User, "same text")
        {
                AuthorName = "User",
                CreatedAt = createdAt,
                MessageId = "m-1"
        };
        AIChatMessage second = new AIChatMessage(ChatRole.User, "same text")
        {
                AuthorName = "User",
                CreatedAt = createdAt,
                MessageId = "m-1"
        };
        AIChatMessage different = new AIChatMessage(ChatRole.User, "different text")
        {
                AuthorName = "User",
                CreatedAt = createdAt,
                MessageId = "m-1"
        };

        Assert.AreEqual(first, second);
        Assert.IsTrue(first == second);
        Assert.AreNotEqual(first, different);
        Assert.IsTrue(first != different);
    }








    [TestMethod]
    public void IsUserIsTrueOnlyForUserMessages()
    {
        AIChatMessage userMessage = new AIChatMessage(ChatRole.User, "hello");
        AIChatMessage assistantMessage = new AIChatMessage(ChatRole.Assistant, "hi");

        Assert.IsTrue(userMessage.IsUser);
        Assert.IsFalse(assistantMessage.IsUser);
    }








    [TestMethod]
    public void TextConcatenatesAllTextContentSegments()
    {
        AIChatMessage message = new AIChatMessage(ChatRole.User, new List<AIContent> { new TextContent("Hello"), new TextContent(", world") });

        Assert.AreEqual("Hello, world", message.Text);
    }
}