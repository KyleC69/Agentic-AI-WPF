using System.Text.Json;

using DataIngestionLib.Models;
using DataIngestionLib.Services;




namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class PersistedAndModelContractsTests
{
    [TestMethod]
    public void PersistedChatMessageDefaultsAreExpected()
    {
        PersistedChatMessage message = new();

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

        PersistedChatMessage left = new()
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
    public void AIModelsConstantsExposeExpectedValues()
    {
        string gpt4 = AIModels.GPT4;
        string gptOss = AIModels.GPTOSS;
        string llama1B = AIModels.LLAMA1_B;
        string mxbai = AIModels.MXBAI;

        Assert.IsFalse(string.IsNullOrWhiteSpace(gpt4));
        Assert.IsFalse(string.IsNullOrWhiteSpace(gptOss));
        Assert.IsFalse(string.IsNullOrWhiteSpace(llama1B));
        Assert.IsFalse(string.IsNullOrWhiteSpace(mxbai));

        CollectionAssert.AllItemsAreUnique(new object[] { gpt4, gptOss, llama1B, mxbai });
    }

    [TestMethod]
    public void TokenBudgetStoresAssignedValues()
    {
        TokenBudget budget = new()
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

    [TestMethod]
    public void HistoryIdentitySupportsInitAndMutationProperties()
    {
        HistoryIdentity identity = new(HistoryIdentityService.GetConversationId())
        {
            ApplicationId = "app",
            ConversationId = "conv"
        };

        identity.AgentId = "agent";
        identity.UserId = "user";

        Assert.AreEqual("app", identity.ApplicationId);
        Assert.AreEqual("conv", identity.ConversationId);
        Assert.AreEqual("agent", identity.AgentId);
        Assert.AreEqual("user", identity.UserId);
    }


}
