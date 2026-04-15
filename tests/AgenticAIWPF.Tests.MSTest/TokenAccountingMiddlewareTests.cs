// Build Date: 2026/04/09
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF.Tests.MSTest
// File:         TokenAccountingMiddlewareTests.cs
// Author: GitHub Copilot



using AgentAILib.Agents;
using AgentAILib.Models;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using Moq;




namespace AgenticAIWPF.Tests.MSTest;





[TestClass]
public class TokenAccountingMiddlewareTests
{
    [TestMethod]
    public void CreateContextSnapshot_WhenAiContextSourceTagged_ClassifiesMessageAsRag()
    {
        List<ChatMessage> messages =
        [
            new ChatMessage(ChatRole.User, "user question"),
            AgentAILib.Models.Extensions.ChatMessageExtensions.WithAgentRequestMessageSource(
                new ChatMessage(ChatRole.User, "retrieved context"),
                AgentRequestMessageSourceType.AIContextProvider,
                "ctx:1"),
            new ChatMessage(ChatRole.System, "system policy"),
            new ChatMessage(ChatRole.Tool, "tool result")
        ];

        TokenUsageSnapshot snapshot = TokenAccountingMiddleware.CreateContextSnapshot(messages, "test.context");

        Assert.IsTrue(snapshot.RagTokens > 0);
        Assert.IsTrue(snapshot.SessionTokens > 0);
        Assert.IsTrue(snapshot.SystemTokens > 0);
        Assert.IsTrue(snapshot.ToolTokens > 0);
        Assert.AreEqual(snapshot.TotalTokens, snapshot.SessionTokens + snapshot.RagTokens + snapshot.SystemTokens + snapshot.ToolTokens);
        Assert.AreEqual(snapshot.RagTokens, (int)snapshot.AdditionalCounts["context_rag_tokens"]);
    }





    [TestMethod]
    public async Task OnResponseAsync_WhenUsageProvided_UsesUsageTotalsAndCategoryCounts()
    {
        List<TokenUsageSnapshot> capturedSnapshots = [];
        TokenAccountingMiddleware middleware = CreateMiddleware(capturedSnapshots);

        List<ChatMessage> requestMessages = [new ChatMessage(ChatRole.User, "hello")];
        TokenAccountingMiddleware.MiddlewareRequestContext requestContext = await middleware.OnRequestAsync(requestMessages, CancellationToken.None);

        AdditionalPropertiesDictionary<long> usageAdditionalCounts = [];
        usageAdditionalCounts["usage_rag_tokens"] = 6;
        usageAdditionalCounts["tool_tokens"] = 4;
        usageAdditionalCounts["usage_system_tokens"] = 3;

        UsageDetails usage = new()
        {
            InputTokenCount = 30,
            OutputTokenCount = 12,
            TotalTokenCount = 42,
            CachedInputTokenCount = 5,
            ReasoningTokenCount = 2,
            AdditionalCounts = usageAdditionalCounts
        };

        ChatResponse response = new([new ChatMessage(ChatRole.Assistant, "answer")])
        {
            Usage = usage
        };

        await middleware.OnResponseAsync(requestContext, response, CancellationToken.None);

        TokenUsageSnapshot snapshot = capturedSnapshots.Single();

        Assert.AreEqual("middleware.response", snapshot.Source);
        Assert.AreEqual(42, snapshot.TotalTokens);
        Assert.AreEqual(30, snapshot.InputTokens);
        Assert.AreEqual(12, snapshot.OutputTokens);
        Assert.AreEqual(5, snapshot.CachedInputTokens);
        Assert.AreEqual(2, snapshot.ReasoningTokens);
        Assert.AreEqual(42L, snapshot.AdditionalCounts["usage_total_tokens"]);
        Assert.AreEqual(6L, snapshot.AdditionalCounts["usage_rag_tokens"]);
        Assert.AreEqual(4L, snapshot.AdditionalCounts["usage_tool_tokens"]);
        Assert.AreEqual(3L, snapshot.AdditionalCounts["usage_system_tokens"]);
    }





    [TestMethod]
    public async Task OnStreamingCompletedAsync_WhenUsageMissing_UsesEstimatedTokens()
    {
        List<TokenUsageSnapshot> capturedSnapshots = [];
        TokenAccountingMiddleware middleware = CreateMiddleware(capturedSnapshots);

        List<ChatMessage> requestMessages =
        [
            new ChatMessage(ChatRole.User, "hello"),
            AgentAILib.Models.Extensions.ChatMessageExtensions.WithAgentRequestMessageSource(
                new ChatMessage(ChatRole.User, "retrieved context"),
                AgentRequestMessageSourceType.AIContextProvider,
                "ctx:2")
        ];

        TokenAccountingMiddleware.MiddlewareRequestContext requestContext = await middleware.OnRequestAsync(requestMessages, CancellationToken.None);

        await middleware.OnStreamingCompletedAsync(requestContext, estimatedStreamingOutputTokens: 9, usage: null, CancellationToken.None);

        TokenUsageSnapshot snapshot = capturedSnapshots.Single();

        Assert.AreEqual("middleware.streaming_response", snapshot.Source);
        Assert.AreEqual(requestContext.EstimatedInputTokens, snapshot.InputTokens);
        Assert.AreEqual(9, snapshot.OutputTokens);
        Assert.AreEqual(requestContext.EstimatedInputTokens + 9, snapshot.TotalTokens);
        Assert.AreEqual(snapshot.TotalTokens, (int)snapshot.AdditionalCounts["usage_total_tokens"]);
        Assert.IsTrue(snapshot.RagTokens > 0);
    }





    [TestMethod]
    public async Task OnStreamingCompletedAsync_WhenUsageProvided_PrefersUsageCounts()
    {
        List<TokenUsageSnapshot> capturedSnapshots = [];
        TokenAccountingMiddleware middleware = CreateMiddleware(capturedSnapshots);

        List<ChatMessage> requestMessages = [new ChatMessage(ChatRole.User, "request")];
        TokenAccountingMiddleware.MiddlewareRequestContext requestContext = await middleware.OnRequestAsync(requestMessages, CancellationToken.None);

        AdditionalPropertiesDictionary<long> usageAdditionalCounts = [];
        usageAdditionalCounts["rag_tokens"] = 11;
        usageAdditionalCounts["usage_tool_tokens"] = 7;
        usageAdditionalCounts["system_tokens"] = 5;

        UsageDetails usage = new()
        {
            InputTokenCount = 80,
            OutputTokenCount = 20,
            TotalTokenCount = 100,
            CachedInputTokenCount = 9,
            ReasoningTokenCount = 4,
            AdditionalCounts = usageAdditionalCounts
        };

        await middleware.OnStreamingCompletedAsync(requestContext, estimatedStreamingOutputTokens: 2, usage, CancellationToken.None);

        TokenUsageSnapshot snapshot = capturedSnapshots.Single();

        Assert.AreEqual(100, snapshot.TotalTokens);
        Assert.AreEqual(80, snapshot.InputTokens);
        Assert.AreEqual(20, snapshot.OutputTokens);
        Assert.AreEqual(9, snapshot.CachedInputTokens);
        Assert.AreEqual(4, snapshot.ReasoningTokens);
        Assert.AreEqual(11L, snapshot.AdditionalCounts["usage_rag_tokens"]);
        Assert.AreEqual(7L, snapshot.AdditionalCounts["usage_tool_tokens"]);
        Assert.AreEqual(5L, snapshot.AdditionalCounts["usage_system_tokens"]);
    }





    private static TokenAccountingMiddleware CreateMiddleware(List<TokenUsageSnapshot> capturedSnapshots)
    {
        IChatClient innerClient = new Mock<IChatClient>(MockBehavior.Strict).Object;
        ILogger<TokenAccountingMiddleware> logger = new Mock<ILogger<TokenAccountingMiddleware>>().Object;

        return new TokenAccountingMiddleware(innerClient, logger, snapshot => capturedSnapshots.Add(snapshot));
    }
}
