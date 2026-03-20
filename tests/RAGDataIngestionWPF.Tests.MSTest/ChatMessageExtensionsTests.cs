using DataIngestionLib.Models.Extensions;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class ChatMessageExtensionsTests
{
    [TestMethod]
    public void GetAgentRequestMessageSourceDefaultsWhenNoAttributionExists()
    {
        ChatMessage message = new(ChatRole.User, "hello");

        string sourceId = DataIngestionLib.Models.Extensions.ChatMessageExtensions.GetAgentRequestMessageSourceId(message);
        AgentRequestMessageSourceType sourceType = DataIngestionLib.Models.Extensions.ChatMessageExtensions.GetAgentRequestMessageSourceType(message);

        Assert.IsNull(sourceId);
        Assert.AreEqual(AgentRequestMessageSourceType.External, sourceType);
    }

    [TestMethod]
    public void WithAgentRequestMessageSourceCreatesTaggedClone()
    {
        ChatMessage original = new(ChatRole.User, "hello");

        ChatMessage tagged = DataIngestionLib.Models.Extensions.ChatMessageExtensions.WithAgentRequestMessageSource(original, AgentRequestMessageSourceType.ChatHistory, "memory:1");

        Assert.AreNotSame(original, tagged);
        Assert.AreEqual("memory:1", DataIngestionLib.Models.Extensions.ChatMessageExtensions.GetAgentRequestMessageSourceId(tagged));
        Assert.AreEqual(AgentRequestMessageSourceType.ChatHistory, DataIngestionLib.Models.Extensions.ChatMessageExtensions.GetAgentRequestMessageSourceType(tagged));
        Assert.AreEqual(AgentRequestMessageSourceType.External, DataIngestionLib.Models.Extensions.ChatMessageExtensions.GetAgentRequestMessageSourceType(original));
    }

    [TestMethod]
    public void WithAgentRequestMessageSourceReturnsSameMessageWhenAttributionUnchanged()
    {
        ChatMessage source = DataIngestionLib.Models.Extensions.ChatMessageExtensions.WithAgentRequestMessageSource(new ChatMessage(ChatRole.Assistant, "ok"), AgentRequestMessageSourceType.External, "x");

        ChatMessage result = DataIngestionLib.Models.Extensions.ChatMessageExtensions.WithAgentRequestMessageSource(source, AgentRequestMessageSourceType.External, "x");

        Assert.AreSame(source, result);
    }

    [TestMethod]
    public void WithAgentRequestMessageSourcePreservesExistingAdditionalProperties()
    {
        ChatMessage source = new(ChatRole.User, "hi")
        {
            AdditionalProperties = []
        };
        source.AdditionalProperties["existing"] = 42;

        ChatMessage result = DataIngestionLib.Models.Extensions.ChatMessageExtensions.WithAgentRequestMessageSource(source, AgentRequestMessageSourceType.External, "src");

        Assert.AreEqual(42, result.AdditionalProperties["existing"]);
        Assert.AreEqual("src", DataIngestionLib.Models.Extensions.ChatMessageExtensions.GetAgentRequestMessageSourceId(result));
    }
}
