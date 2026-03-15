using DataIngestionLib.Models;

using Microsoft.Extensions.AI;



namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class AIChatMessageTests
{
    [TestMethod]
    public void AuthorNameNormalizesWhitespaceToNull()
    {
        AIChatMessage message = new(ChatRole.User, "hello")
        {
            AuthorName = "   "
        };

        Assert.IsNull(message.AuthorName);
    }

    [TestMethod]
    public void CloneCreatesDistinctMessageWithSameCoreValues()
    {
        DateTimeOffset createdAt = new(2026, 3, 15, 0, 0, 0, TimeSpan.Zero);
        AIChatMessage originalMessage = new(ChatRole.Assistant, "answer")
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
        DateTimeOffset createdAt = new(2026, 3, 15, 1, 0, 0, TimeSpan.Zero);
        AIChatMessage first = new(ChatRole.User, "same text")
        {
            AuthorName = "User",
            CreatedAt = createdAt,
            MessageId = "m-1"
        };
        AIChatMessage second = new(ChatRole.User, "same text")
        {
            AuthorName = "User",
            CreatedAt = createdAt,
            MessageId = "m-1"
        };
        AIChatMessage different = new(ChatRole.User, "different text")
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
        AIChatMessage userMessage = new(ChatRole.User, "hello");
        AIChatMessage assistantMessage = new(ChatRole.Assistant, "hi");

        Assert.IsTrue(userMessage.IsUser);
        Assert.IsFalse(assistantMessage.IsUser);
    }

    [TestMethod]
    public void TextConcatenatesAllTextContentSegments()
    {
        AIChatMessage message = new(ChatRole.User, new List<AIContent>
        {
            new TextContent("Hello"),
            new TextContent(", world"),
        });

        Assert.AreEqual("Hello, world", message.Text);
    }
}
