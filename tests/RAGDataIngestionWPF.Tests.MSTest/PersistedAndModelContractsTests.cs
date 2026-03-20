using System.ComponentModel.DataAnnotations;
using System.Text.Json;

using DataIngestionLib.Models;
using DataIngestionLib.RAGModels;
using DataIngestionLib.Services.Contracts;

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
        DateTimeOffset now = DateTimeOffset.UtcNow;
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
            TimestampUtc = now,
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
        string llama3B = AIModels.LLAMA323_B;
        string mxbai = AIModels.MXBAI;

        Assert.IsFalse(string.IsNullOrWhiteSpace(gpt4));
        Assert.IsFalse(string.IsNullOrWhiteSpace(gptOss));
        Assert.IsFalse(string.IsNullOrWhiteSpace(llama1B));
        Assert.IsFalse(string.IsNullOrWhiteSpace(llama3B));
        Assert.IsFalse(string.IsNullOrWhiteSpace(mxbai));

        CollectionAssert.AllItemsAreUnique(new object[] { gpt4, gptOss, llama1B, llama3B, mxbai });
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
        HistoryIdentity identity = new()
        {
            ApplicationId = "app",
            ConversationId = "conv"
        };

        identity.AgentId = "agent";
        identity.MessageId = "mid";
        identity.UserId = "user";

        Assert.AreEqual("app", identity.ApplicationId);
        Assert.AreEqual("conv", identity.ConversationId);
        Assert.AreEqual("agent", identity.AgentId);
        Assert.AreEqual("mid", identity.MessageId);
        Assert.AreEqual("user", identity.UserId);
    }

    [TestMethod]
    public void DocumentHonorsMaxLengthValidation()
    {
        Document document = new()
        {
            Title = new string('t', 513),
            ContentRaw = "content"
        };

        ValidationContext context = new(document);
        List<ValidationResult> results = [];

        bool valid = Validator.TryValidateObject(document, context, results, true);

        Assert.IsFalse(valid);
        Assert.IsTrue(results.Any(result => result.MemberNames.Contains(nameof(Document.Title))));
    }

    [TestMethod]
    public void MetadataHonorsMaxLengthValidation()
    {
        Metadata metadata = new()
        {
            Tags = new string('x', 1025)
        };

        ValidationContext context = new(metadata);
        List<ValidationResult> results = [];

        bool valid = Validator.TryValidateObject(metadata, context, results, true);

        Assert.IsFalse(valid);
        Assert.IsTrue(results.Any(result => result.MemberNames.Contains(nameof(Metadata.Tags))));
    }

    [TestMethod]
    public void RemoteRagHonorsMaxLengthValidation()
    {
        RemoteRag rag = new()
        {
            Title = "title",
            Description = "description",
            OgUrl = "https://example.com",
            Summary = new string('s', 4001)
        };

        ValidationContext context = new(rag);
        List<ValidationResult> results = [];

        bool valid = Validator.TryValidateObject(rag, context, results, true);

        Assert.IsFalse(valid);
        Assert.IsTrue(results.Any(result => result.MemberNames.Contains(nameof(RemoteRag.Summary))));
    }
}
