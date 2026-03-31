// Build Date: 2026/03/30
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         TokenAccountingMiddleware.cs
// Author: Kyle L. Crowder
// Build Num: 233120



using System.Runtime.CompilerServices;

using DataIngestionLib.Models;

using Microsoft.Extensions.AI;




namespace DataIngestionLib.Agents;





internal sealed class TokenAccountingMiddleware : DelegatingChatClient
{
    private readonly Action<TokenUsageSnapshot>? _tokenSnapshotSink;
    private static readonly AsyncLocal<TurnState?> CurrentTurnState = new();








    public TokenAccountingMiddleware(IChatClient innerClient, Action<TokenUsageSnapshot>? tokenSnapshotSink = null) : base(innerClient)
    {
        ArgumentNullException.ThrowIfNull(innerClient);

        _tokenSnapshotSink = tokenSnapshotSink;
    }








    public override async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        MiddlewareRequestContext requestContext = await OnRequestAsync(messages, cancellationToken).ConfigureAwait(false);
        ChatResponse response = await base.GetResponseAsync(messages, options, cancellationToken).ConfigureAwait(false);

        await OnResponseAsync(requestContext, response, cancellationToken).ConfigureAwait(false);

        return response;
    }








    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        MiddlewareRequestContext requestContext = await OnRequestAsync(messages, cancellationToken).ConfigureAwait(false);
        List<ChatResponseUpdate> updates = [];

        await foreach (ChatResponseUpdate update in base.GetStreamingResponseAsync(messages, options, cancellationToken).ConfigureAwait(false))
        {
            updates.Add(update);
            yield return update;
        }

        ChatResponse response = updates.Count == 0 ? new ChatResponse() : updates.ToChatResponse();
        await OnResponseAsync(requestContext, response, cancellationToken).ConfigureAwait(false);
    }








    private static int ClampToInt(long value)
    {
        return value <= 0 ? 0 : value >= int.MaxValue ? int.MaxValue : (int)value;
    }








    private static ContextBuckets ClassifyContextBuckets(IReadOnlyList<ChatMessage> messages)
    {
        var sessionTokens = 0;
        var ragTokens = 0;
        var toolTokens = 0;
        var systemTokens = 0;

        foreach (ChatMessage message in messages)
        {
            var tokenCount = EstimateTokens([message.Text]);
            var role = message.Role.Value;
            if (string.Equals(role, AIChatRole.System.Value, StringComparison.OrdinalIgnoreCase))
            {
                systemTokens += tokenCount;
            }
            else if (string.Equals(role, AIChatRole.Tool.Value, StringComparison.OrdinalIgnoreCase))
            {
                toolTokens += tokenCount;
            }
            else if (string.Equals(role, AIChatRole.RAGContext.Value, StringComparison.OrdinalIgnoreCase) || string.Equals(role, AIChatRole.AIContext.Value, StringComparison.OrdinalIgnoreCase) || string.Equals(role, "rag", StringComparison.OrdinalIgnoreCase))
            {
                ragTokens += tokenCount;
            }
            else
            {
                sessionTokens += tokenCount;
            }
        }

        return new ContextBuckets(sessionTokens + ragTokens + toolTokens + systemTokens, sessionTokens, ragTokens, toolTokens, systemTokens);
    }








    internal static TokenUsageSnapshot CreateContextSnapshot(IEnumerable<ChatMessage> messages, string source)
    {
        ArgumentNullException.ThrowIfNull(messages);

        var capturedMessages = messages as IReadOnlyList<ChatMessage> ?? [.. messages];
        ContextBuckets buckets = ClassifyContextBuckets(capturedMessages);
        Dictionary<string, long> additionalCounts = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase)
        {
                ["turn_model_call_count"] = 0,
                ["turn_nested_model_call_count"] = 0,
                ["turn_estimated_input_tokens"] = 0,
                ["context_total_tokens"] = buckets.Total,
                ["context_session_tokens"] = buckets.Session,
                ["context_rag_tokens"] = buckets.Rag,
                ["context_tool_tokens"] = buckets.Tool,
                ["context_system_tokens"] = buckets.System,
                ["usage_rag_tokens"] = 0,
                ["usage_tool_tokens"] = 0,
                ["usage_system_tokens"] = 0
        };

        return new TokenUsageSnapshot(TotalTokens: buckets.Total, SessionTokens: buckets.Session, RagTokens: buckets.Rag, ToolTokens: buckets.Tool, SystemTokens: buckets.System, InputTokens: 0, OutputTokens: 0, CachedInputTokens: 0, ReasoningTokens: 0, Source: source, UpdatedAtUtc: DateTimeOffset.UtcNow, AdditionalCounts: additionalCounts);
    }








    private static int EstimateTokens(IEnumerable<string?> values)
    {
        var total = 0;
        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            total += Math.Max(1, value.Length / 4);
        }

        return total;
    }








    private static long GetAdditionalCount(UsageDetails usageDetails, params string[] keys)
    {
        if (usageDetails.AdditionalCounts is null || usageDetails.AdditionalCounts.Count == 0)
        {
            return 0;
        }

        HashSet<string> keySet = new HashSet<string>(keys, StringComparer.OrdinalIgnoreCase);
        long total = 0;
        foreach (var (key, value) in usageDetails.AdditionalCounts)
            if (keySet.Contains(key))
            {
                total += value;
            }

        return total;
    }








    private static UsageDetails NormalizeUsage(IReadOnlyList<ChatMessage> requestMessages, ChatResponse response)
    {
        UsageDetails usage = response.Usage ?? new UsageDetails();

        var usageInputTokens = usage.InputTokenCount ?? 0;
        var usageOutputTokens = usage.OutputTokenCount ?? 0;
        var usageTotalTokens = usage.TotalTokenCount ?? 0;

        var inputTokens = usageInputTokens > 0 ? usageInputTokens : EstimateTokens(requestMessages.Select(message => message.Text));

        var outputTokens = usageOutputTokens > 0 ? usageOutputTokens : EstimateTokens((response.Messages ?? []).Select(message => message.Text));

        var totalTokens = usageTotalTokens > 0 ? usageTotalTokens : inputTokens + outputTokens;

        return new UsageDetails
        {
                InputTokenCount = inputTokens,
                OutputTokenCount = outputTokens,
                TotalTokenCount = totalTokens,
                CachedInputTokenCount = usage.CachedInputTokenCount ?? 0,
                ReasoningTokenCount = usage.ReasoningTokenCount ?? 0,
                AdditionalCounts = usage.AdditionalCounts is { Count: > 0 } ? new AdditionalPropertiesDictionary<long>(usage.AdditionalCounts) : new AdditionalPropertiesDictionary<long>()
        };
    }








    internal Task<MiddlewareRequestContext> OnRequestAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var capturedMessages = messages as IReadOnlyList<ChatMessage> ?? [.. messages];
        var estimatedInputTokens = EstimateTokens(capturedMessages.Select(message => message.Text));

        MiddlewareRequestContext requestContext = new MiddlewareRequestContext(capturedMessages, estimatedInputTokens);

        TurnState state = CurrentTurnState.Value ?? new TurnState();
        CurrentTurnState.Value = state;

        state.Depth++;
        state.ModelCallCount++;
        state.EstimatedInputTokens += estimatedInputTokens;

        if (state.Depth == 1)
        {
            state.ContextBuckets = ClassifyContextBuckets(capturedMessages);
        }

        return Task.FromResult(requestContext);
    }








    internal Task OnResponseAsync(MiddlewareRequestContext requestContext, ChatResponse response, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TurnState? state = CurrentTurnState.Value;
        if (state is null)
        {
            return Task.CompletedTask;
        }

        UsageDetails normalizedUsage = NormalizeUsage(requestContext.Messages, response);

        state.AggregatedUsage.InputTokenCount = (state.AggregatedUsage.InputTokenCount ?? 0) + (normalizedUsage.InputTokenCount ?? 0);
        state.AggregatedUsage.OutputTokenCount = (state.AggregatedUsage.OutputTokenCount ?? 0) + (normalizedUsage.OutputTokenCount ?? 0);
        state.AggregatedUsage.TotalTokenCount = (state.AggregatedUsage.TotalTokenCount ?? 0) + (normalizedUsage.TotalTokenCount ?? 0);
        state.AggregatedUsage.CachedInputTokenCount = (state.AggregatedUsage.CachedInputTokenCount ?? 0) + (normalizedUsage.CachedInputTokenCount ?? 0);
        state.AggregatedUsage.ReasoningTokenCount = (state.AggregatedUsage.ReasoningTokenCount ?? 0) + (normalizedUsage.ReasoningTokenCount ?? 0);

        if (normalizedUsage.AdditionalCounts is not null)
        {
            state.AggregatedUsage.AdditionalCounts ??= new AdditionalPropertiesDictionary<long>();
            foreach (var (key, value) in normalizedUsage.AdditionalCounts)
                if (state.AggregatedUsage.AdditionalCounts.TryGetValue(key, out var existingValue))
                {
                    state.AggregatedUsage.AdditionalCounts[key] = existingValue + value;
                }
                else
                {
                    state.AggregatedUsage.AdditionalCounts[key] = value;
                }
        }

        state.Depth--;
        if (state.Depth > 0)
        {
            return Task.CompletedTask;
        }

        ContextBuckets buckets = state.ContextBuckets;
        UsageDetails aggregatedUsage = state.AggregatedUsage;

        Dictionary<string, long> additionalCounts = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase)
        {
                ["turn_model_call_count"] = state.ModelCallCount,
                ["turn_nested_model_call_count"] = Math.Max(0, state.ModelCallCount - 1),
                ["turn_estimated_input_tokens"] = state.EstimatedInputTokens,
                ["context_total_tokens"] = buckets.Total,
                ["context_session_tokens"] = buckets.Session,
                ["context_rag_tokens"] = buckets.Rag,
                ["context_tool_tokens"] = buckets.Tool,
                ["context_system_tokens"] = buckets.System,
                ["usage_rag_tokens"] = GetAdditionalCount(aggregatedUsage, "rag", "rag_tokens", "rag_token_count", "rag_context", "retrieval", "retrieval_tokens", "context", "context_tokens"),
                ["usage_tool_tokens"] = GetAdditionalCount(aggregatedUsage, "tool", "tool_tokens", "tool_token_count", "function", "function_tokens"),
                ["usage_system_tokens"] = GetAdditionalCount(aggregatedUsage, "system", "system_tokens", "system_token_count", "instruction", "instruction_tokens")
        };

        foreach (var (key, value) in aggregatedUsage.AdditionalCounts ?? new AdditionalPropertiesDictionary<long>()) additionalCounts[key] = value;

        TokenUsageSnapshot snapshot = new TokenUsageSnapshot(TotalTokens: ClampToInt(aggregatedUsage.TotalTokenCount ?? 0), SessionTokens: buckets.Session, RagTokens: buckets.Rag, ToolTokens: buckets.Tool, SystemTokens: buckets.System, InputTokens: ClampToInt(aggregatedUsage.InputTokenCount ?? 0), OutputTokens: ClampToInt(aggregatedUsage.OutputTokenCount ?? 0), CachedInputTokens: ClampToInt(aggregatedUsage.CachedInputTokenCount ?? 0), ReasoningTokens: ClampToInt(aggregatedUsage.ReasoningTokenCount ?? 0), Source: "middleware.turn.consolidated", UpdatedAtUtc: DateTimeOffset.UtcNow, AdditionalCounts: additionalCounts);

        _tokenSnapshotSink?.Invoke(snapshot);
        CurrentTurnState.Value = null;

        return Task.CompletedTask;
    }








    internal sealed record MiddlewareRequestContext(IReadOnlyList<ChatMessage> Messages, int EstimatedInputTokens);





    private sealed class TurnState
    {
        public UsageDetails AggregatedUsage { get; } = new UsageDetails { AdditionalCounts = new AdditionalPropertiesDictionary<long>() };
        public ContextBuckets ContextBuckets { get; set; }
        public int Depth { get; set; }
        public int EstimatedInputTokens { get; set; }
        public int ModelCallCount { get; set; }
    }





    private readonly record struct ContextBuckets(int Total, int Session, int Rag, int Tool, int System);
}