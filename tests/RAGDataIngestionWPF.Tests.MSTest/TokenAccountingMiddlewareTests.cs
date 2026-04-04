using DataIngestionLib.Agents;
using DataIngestionLib.Models;

using Microsoft.Extensions.AI;

using Moq;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class TokenAccountingMiddlewareTests
{
    [TestMethod]
    public async Task OnResponseAsync_UsesContextWindowTotalsForSnapshotAndPreservesUsageTotalsInAdditionalCounts()
    {
        var hasSnapshot = false;
        TokenUsageSnapshot capturedSnapshot = null!;
        TokenAccountingMiddleware middleware = new(new Mock<IChatClient>().Object, snapshot =>
        {
            capturedSnapshot = snapshot;
            hasSnapshot = true;
        });

        IReadOnlyList<ChatMessage> requestMessages =
        [
            new ChatMessage(ChatRole.User, "Summarize recent diagnostics output."),
            new ChatMessage(AIChatRole.RAGContext, "Retrieved chunk from docs and logs."),
            new ChatMessage(ChatRole.Tool, "Tool call result payload.")
        ];

        TokenAccountingMiddleware.MiddlewareRequestContext requestContext = await middleware.OnRequestAsync(requestMessages, CancellationToken.None);

        ChatResponse response = new()
        {
            Usage = new UsageDetails
            {
                InputTokenCount = 300,
                OutputTokenCount = 120,
                TotalTokenCount = 420,
                AdditionalCounts = new AdditionalPropertiesDictionary<long>()
            }
        };

        await middleware.OnResponseAsync(requestContext, response, CancellationToken.None);

        Assert.IsTrue(hasSnapshot);
        Assert.IsTrue(capturedSnapshot.AdditionalCounts.TryGetValue("context_total_tokens", out long contextTotal));
        Assert.IsTrue(capturedSnapshot.AdditionalCounts.TryGetValue("usage_total_tokens", out long usageTotal));
        Assert.AreEqual((int)contextTotal, capturedSnapshot.TotalTokens);
        Assert.AreEqual(420, usageTotal);
        Assert.AreNotEqual((int)usageTotal, capturedSnapshot.TotalTokens);
    }

    [TestMethod]
    public void CreateContextSnapshot_CountsSerializedMessagePayloadNotOnlyMessageText()
    {
        ChatMessage metadataHeavyMessage = new(ChatRole.User, string.Empty)
        {
            AdditionalProperties = new AdditionalPropertiesDictionary
            {
                ["payload"] = new string('x', 160)
            }
        };

        TokenUsageSnapshot snapshot = TokenAccountingMiddleware.CreateContextSnapshot([metadataHeavyMessage], "test.context");

        Assert.IsTrue(snapshot.TotalTokens > 0);
        Assert.IsTrue(snapshot.AdditionalCounts.TryGetValue("context_total_tokens", out long contextTotal));
        Assert.AreEqual(snapshot.TotalTokens, (int)contextTotal);
    }
}
