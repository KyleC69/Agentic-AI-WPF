// Build Date: 2026/04/06
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF.Tests.MSTest
// File:         PersistedAndModelContractsTests.cs
// Author: Kyle L. Crowder
// Build Num: 213000



using System.Text.Json;

using AgentAILib.Models;
using AgentAILib.Services;




namespace AgenticAIWPF.Tests.MSTest;





[TestClass]
public class PersistedAndModelContractsTests
{

    [TestMethod]
    public void AIModelsConstantsExposeExpectedValues()
    {
        var gpt4 = AIModels.GPT4;
        var gptOss = AIModels.GPTOSS120;
        var llama1B = AIModels.LLAMA1_8B;
        var mxbai = AIModels.MXBAI;

        Assert.IsFalse(string.IsNullOrWhiteSpace(gpt4));
        Assert.IsFalse(string.IsNullOrWhiteSpace(gptOss));
        Assert.IsFalse(string.IsNullOrWhiteSpace(llama1B));
        Assert.IsFalse(string.IsNullOrWhiteSpace(mxbai));

        CollectionAssert.AllItemsAreUnique(new object[] { gpt4, gptOss, llama1B, mxbai });
    }







    [TestMethod]
    public void AIModels_ChatModels_ContainsOnlyNonEmptyUniqueDescriptors()
    {
        var models = AIModels.ChatModels;

        Assert.IsTrue(models.Count > 0);
        CollectionAssert.AllItemsAreNotNull(models.ToList());

        var ids = models.Select(m => m.ModelId).ToList();
        var names = models.Select(m => m.DisplayName).ToList();

        CollectionAssert.AllItemsAreUnique(ids);
        CollectionAssert.AllItemsAreUnique(names);
        Assert.IsTrue(ids.All(id => !string.IsNullOrWhiteSpace(id)));
        Assert.IsTrue(names.All(n => !string.IsNullOrWhiteSpace(n)));
    }







    [TestMethod]
    public void AIModels_Default_IsFirstChatModel()
    {
        Assert.AreEqual(AIModels.ChatModels[0], AIModels.Default);
    }







    [TestMethod]
    public void AIModelDescriptor_RecordEquality_IsValueBased()
    {
        var a = new AIModelDescriptor("Test Model", "test:1b");
        var b = new AIModelDescriptor("Test Model", "test:1b");
        var c = new AIModelDescriptor("Other Model", "test:1b");

        Assert.AreEqual(a, b);
        Assert.AreNotEqual(a, c);
    }








    [TestMethod]
    public void HistoryIdentitySupportsInitAndMutationProperties()
    {
        HistoryIdentity identity = new HistoryIdentity(HistoryIdentityService.GetConversationId()) { ApplicationId = "app", ConversationId = "conv" };

        identity.AgentId = "agent";
        identity.UserId = "user";

        Assert.AreEqual("app", identity.ApplicationId);
        Assert.AreEqual("conv", identity.ConversationId);
        Assert.AreEqual("agent", identity.AgentId);
        Assert.AreEqual("user", identity.UserId);
    }








    [TestMethod]
    public void PersistedChatMessageDefaultsAreExpected()
    {
        PersistedChatMessage message = new PersistedChatMessage();

        Assert.AreEqual(string.Empty, message.AgentId);
        Assert.AreEqual(string.Empty, message.ApplicationId);
        Assert.AreEqual(string.Empty, message.Content);
        Assert.AreEqual(string.Empty, message.ConversationId);
        Assert.AreEqual(string.Empty, message.Role);
        Assert.AreEqual(string.Empty, message.UserId);
        Assert.AreEqual(Guid.Empty, message.MessageId);
        Assert.IsNull(message.Metadata);
    }








    [TestMethod]
    public void PersistedChatMessageRecordEqualityIsValueBased()
    {
        using JsonDocument metadata = JsonDocument.Parse("{\"a\":1}");

        PersistedChatMessage left = new PersistedChatMessage
        {
                AgentId = "a",
                ApplicationId = "app",
                Content = "c",
                ConversationId = "conv",
                MessageId = Guid.NewGuid(),
                Metadata = metadata,
                Role = "user",
                TimestampUtc = DateTime.Now,
                UserId = "u"
        };

        PersistedChatMessage right = left with { };

        Assert.AreEqual(left, right);
    }








    [TestMethod]
    public void TokenBudgetStoresAssignedValues()
    {
        TokenBudget budget = new TokenBudget
        {
                BudgetTotal = 100,
                MaximumContext = 90,
                MetaBudget = 5,
                RAGBudget = 10,
                SessionBudget = 20,
                SystemBudget = 30,
                ToolBudget = 40
        };

        Assert.AreEqual(100, budget.BudgetTotal);
        Assert.AreEqual(90, budget.MaximumContext);
        Assert.AreEqual(5, budget.MetaBudget);
        Assert.AreEqual(10, budget.RAGBudget);
        Assert.AreEqual(20, budget.SessionBudget);
        Assert.AreEqual(30, budget.SystemBudget);
        Assert.AreEqual(40, budget.ToolBudget);
    }
}