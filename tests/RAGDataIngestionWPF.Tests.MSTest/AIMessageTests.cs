// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         AIMessageTests.cs
// Author: Kyle L. Crowder
// Build Num: 212953



using DataIngestionLib.Models;

using Microsoft.Extensions.AI;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class AIMessageTests
{

    [TestMethod]
    public void AuthorNameNonWhitespaceIsRetained()
    {
        AIMessage message = new();

        message.AuthorName = "alice";

        Assert.AreEqual("alice", message.AuthorName);
    }








    [TestMethod]
    public void AuthorNameWhitespaceIsNormalizedToNull()
    {
        AIMessage message = new();

        message.AuthorName = "   ";

        Assert.IsNull(message.AuthorName);
    }








    [TestMethod]
    public void CloneCopiesMessageProperties()
    {
        AdditionalPropertiesDictionary properties = [];
        properties["k"] = "v";

        AIMessage message = new(ChatRole.Tool, "payload")
        {
                AuthorName = "agent",
                MessageId = "mid",
                AdditionalProperties = properties,
                CreatedAt = DateTimeOffset.UtcNow,
                RawRepresentation = new object()
        };

        ChatMessage clone = message.Clone();

        Assert.AreEqual(message.AuthorName, clone.AuthorName);
        Assert.AreEqual(message.MessageId, clone.MessageId);
        Assert.AreEqual(message.Role, clone.Role);
        Assert.AreSame(message.AdditionalProperties, clone.AdditionalProperties);
        Assert.AreSame(message.RawRepresentation, clone.RawRepresentation);
    }








    [TestMethod]
    public void ConstructorWithNullTextCreatesEmptyContents()
    {
        string content = null;
        AIMessage message = new(ChatRole.Assistant, content);

        Assert.AreEqual(string.Empty, message.Text);
        Assert.AreEqual(0, message.Contents.Count);
    }








    [TestMethod]
    public void ConstructorWithTextCreatesSingleTextContent()
    {
        AIMessage message = new(ChatRole.Assistant, "hello");

        Assert.AreEqual(ChatRole.Assistant, message.Role);
        Assert.AreEqual("hello", message.Text);
        Assert.AreEqual(1, message.Contents.Count);
    }








    [TestMethod]
    public void DefaultConstructorInitializesUserRoleAndEmptyText()
    {
        AIMessage message = new();

        Assert.AreEqual(ChatRole.User, message.Role);
        Assert.AreEqual(string.Empty, message.Text);
        Assert.AreEqual(0, message.Contents.Count);
    }








    [TestMethod]
    public void TextConcatenatesAllTextContentItems()
    {
        AIMessage message = new(ChatRole.User, new List<AIContent> { new TextContent("one"), new TextContent("two") });

        Assert.AreEqual("onetwo", message.Text);
        Assert.AreEqual("onetwo", message.ToString());
    }
}