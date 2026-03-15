// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         ChatHistoryBehaviorTests.cs
// Author: Kyle L. Crowder
// Build Num: 091002



using DataIngestionLib.Models;

using Microsoft.Extensions.AI;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class ChatHistoryBehaviorTests
{

    [TestMethod]
    public void AddAssistantMessagesRejectsMessageWithUnexpectedRole()
    {
        AIChatHistory history = [];
        AIChatMessage invalidMessage = new AIChatMessage(ChatRole.User, "wrong role");

        Assert.ThrowsExactly<ArgumentException>(() => history.AddAssistantMessages(new[] { invalidMessage }));
    }








    [TestMethod]
    public void AddSystemMessagesRejectsNullMessageElement()
    {
        AIChatHistory history = [];
        AIChatMessage[] invalidMessages = [null!];

        Assert.ThrowsExactly<ArgumentNullException>(() => history.AddSystemMessages(invalidMessages));
    }








    [TestMethod]
    public void AddUserMessagesAddsAllMessagesWhenRolesMatch()
    {
        AIChatHistory history = [];

        history.AddUserMessages(
        [
                new AIChatMessage(ChatRole.User, "first"),
                new AIChatMessage(ChatRole.User, "second")
        ]);

        Assert.AreEqual(2, history.Count);
        Assert.AreEqual("second", history.LastMessage?.Text);
    }








    [TestMethod]
    public void ConstructorWithMessagesCopiesValuesIntoIndependentInstances()
    {
        AIChatMessage originalMessage = new AIChatMessage(ChatRole.User, "original");
        AIChatHistory history = new AIChatHistory(new[] { originalMessage });

        Assert.AreEqual(1, history.Count);
        Assert.AreEqual("original", history[0].Text);
        Assert.AreNotSame(originalMessage, history[0]);
    }








    [TestMethod]
    public void ConstructorWithMessageTuplesPreservesOrderAndValues()
    {
        AIChatHistory history = new AIChatHistory(new[] { (ChatRole.System, "system"), (ChatRole.User, "question"), (ChatRole.Assistant, "answer") });

        Assert.AreEqual(3, history.Count);
        Assert.AreEqual("system", history[0].Text);
        Assert.AreEqual<ChatRole>(ChatRole.User, history[1].Role);
        Assert.AreEqual("answer", history.LastMessage?.Text);
    }








    [TestMethod]
    public void ConstructorWithSystemMessageCreatesSingleSystemEntry()
    {
        AIChatHistory history = new AIChatHistory("You are helpful.");

        Assert.AreEqual(1, history.Count);
        Assert.AreEqual<ChatRole>(ChatRole.System, history[0].Role);
        Assert.AreEqual("You are helpful.", history[0].Text);
    }








    [TestMethod]
    public void EstimateTokenCountSkipsWhitespaceOnlyMessages()
    {
        AIChatHistory history = new AIChatHistory(new[] { new AIChatMessage(ChatRole.User, "    "), new AIChatMessage(ChatRole.Assistant, "abcd") });

        Assert.AreEqual(1, history.EstimateTokenCount());
    }








    [TestMethod]
    public void RemoveRangeRemovesRequestedSlice()
    {
        AIChatHistory history = [];
        history.AddUserMessage("first");
        history.AddAssistantMessage("second");
        history.AddSystemMessage("third");

        history.RemoveRange(1, 2);

        Assert.AreEqual(1, history.Count);
        Assert.AreEqual("first", history[0].Text);
    }
}