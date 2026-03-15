using DataIngestionLib.Models;
using DataIngestionLib.Models.Extensions;

using Microsoft.Agents.AI;



namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class ChatMessageExtensionsTests
{
    [TestMethod]
    public void GetAgentRequestMessageSourceDefaultsWhenMessageIsUntagged()
    {
        AIChatMessage message = new(AIChatRole.User, "hello");

        Assert.IsNull(message.GetAgentRequestMessageSourceId());
        Assert.AreEqual(AgentRequestMessageSourceType.External, message.GetAgentRequestMessageSourceType());
    }

    [TestMethod]
    public void WithAgentRequestMessageSourceReturnsSameInstanceWhenTagAlreadyMatches()
    {
        AIChatMessage message = new(AIChatRole.User, "hello");
        AIChatMessage taggedMessage = message.WithAgentRequestMessageSource(AgentRequestMessageSourceType.ChatHistory, "source-1");

        AIChatMessage returnedMessage = taggedMessage.WithAgentRequestMessageSource(AgentRequestMessageSourceType.ChatHistory, "source-1");

        Assert.AreSame(taggedMessage, returnedMessage);
    }

    [TestMethod]
    public void WithAgentRequestMessageSourceSetsRequestedSourceMetadata()
    {
        AIChatMessage message = new(AIChatRole.Assistant, "response");

        AIChatMessage taggedMessage = message.WithAgentRequestMessageSource(AgentRequestMessageSourceType.ChatHistory, "rag-42");

        Assert.AreNotSame(message, taggedMessage);
        Assert.AreEqual(AgentRequestMessageSourceType.ChatHistory, taggedMessage.GetAgentRequestMessageSourceType());
        Assert.AreEqual("rag-42", taggedMessage.GetAgentRequestMessageSourceId());
    }

    [TestMethod]
    public void WithAgentRequestMessageSourceDoesNotMutateOriginalAdditionalProperties()
    {
        AIChatMessage originalMessage = new(AIChatRole.User, "question")
        {
            AdditionalProperties = []
        };
        originalMessage.AdditionalProperties["existing"] = "value";

        AIChatMessage taggedMessage = originalMessage.WithAgentRequestMessageSource(AgentRequestMessageSourceType.ChatHistory, "chat-history");

        Assert.AreEqual("value", originalMessage.AdditionalProperties["existing"]);
        Assert.IsFalse(originalMessage.AdditionalProperties.ContainsKey(AgentRequestMessageSourceAttribution.AdditionalPropertiesKey));
        Assert.AreEqual("value", taggedMessage.AdditionalProperties!["existing"]);
        Assert.AreEqual("chat-history", taggedMessage.GetAgentRequestMessageSourceId());
    }
}
