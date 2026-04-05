// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:      ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}

using System.Runtime.CompilerServices;
using System.Text.Json;

using DataIngestionLib.Models;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace DataIngestionLib.Agents;

public sealed class TokenAccountingMiddleware : DelegatingChatClient
{
    private const int CHARS_PER_TOKEN = 4;

    private readonly Action<TokenUsageSnapshot>? _tokenSnapshotSink;
    private readonly ILogger<TokenAccountingMiddleware> _logger;

    private static readonly object CategoryEventsGate = new();
    private static readonly JsonSerializerOptions JsonOptions = new();
    private static CategoryCounts LastPublishedCounts;

    public TokenAccountingMiddleware(
        IChatClient innerClient,
        ILogger<TokenAccountingMiddleware> logger,
        Action<TokenUsageSnapshot>? tokenSnapshotSink = null)
        : base(innerClient)
    {
        ArgumentNullException.ThrowIfNull(innerClient);
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
        _tokenSnapshotSink = tokenSnapshotSink;
    }

    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        MiddlewareRequestContext requestContext = await OnRequestAsync(messages, cancellationToken).ConfigureAwait(false);

        ChatResponse response = await base.GetResponseAsync(messages, options, cancellationToken).ConfigureAwait(false);

        await OnResponseAsync(requestContext, response, cancellationToken).ConfigureAwait(false);

        return response;
    }

    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _ = await OnRequestAsync(messages, cancellationToken).ConfigureAwait(false);

        await foreach (ChatResponseUpdate update in base.GetStreamingResponseAsync(messages, options, cancellationToken).ConfigureAwait(false))
        {
            yield return update;
        }
    }

    internal Task<MiddlewareRequestContext> OnRequestAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<ChatMessage> capturedMessages = messages as IReadOnlyList<ChatMessage> ?? [.. messages];
        int estimatedInputTokens = EstimateMessagesTokens(capturedMessages);

        MiddlewareRequestContext requestContext = new(capturedMessages, estimatedInputTokens);
        return Task.FromResult(requestContext);
    }

    internal Task OnResponseAsync(MiddlewareRequestContext requestContext, ChatResponse response, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        UsageDetails usage = NormalizeUsage(requestContext.Messages, response);

        ContextBuckets contextBuckets = ClassifyContextBuckets(requestContext.Messages);
        Dictionary<string, long> additionalCounts = new(StringComparer.OrdinalIgnoreCase)
        {
            ["turn_model_call_count"] = 1,
            ["turn_nested_model_call_count"] = 0,
            ["turn_estimated_input_tokens"] = requestContext.EstimatedInputTokens,
            ["context_total_tokens"] = contextBuckets.Total,
            ["context_session_tokens"] = contextBuckets.Session,
            ["context_rag_tokens"] = contextBuckets.Rag,
            ["context_tool_tokens"] = contextBuckets.Tool,
            ["context_system_tokens"] = contextBuckets.System,
            ["usage_total_tokens"] = usage.TotalTokenCount ?? 0,
            ["usage_input_tokens"] = usage.InputTokenCount ?? 0,
            ["usage_output_tokens"] = usage.OutputTokenCount ?? 0,
            ["usage_cached_input_tokens"] = usage.CachedInputTokenCount ?? 0,
            ["usage_reasoning_tokens"] = usage.ReasoningTokenCount ?? 0,
            ["usage_rag_tokens"] = GetAdditionalCount(usage, "usage_rag_tokens", "rag_tokens"),
            ["usage_tool_tokens"] = GetAdditionalCount(usage, "usage_tool_tokens", "tool_tokens"),
            ["usage_system_tokens"] = GetAdditionalCount(usage, "usage_system_tokens", "system_tokens")
        };

        TokenUsageSnapshot snapshot = new(
            TotalTokens: contextBuckets.Total,
            SessionTokens: contextBuckets.Session,
            RagTokens: contextBuckets.Rag,
            ToolTokens: contextBuckets.Tool,
            SystemTokens: contextBuckets.System,
            InputTokens: ClampToInt(usage.InputTokenCount ?? 0),
            OutputTokens: ClampToInt(usage.OutputTokenCount ?? 0),
            CachedInputTokens: ClampToInt(usage.CachedInputTokenCount ?? 0),
            ReasoningTokens: ClampToInt(usage.ReasoningTokenCount ?? 0),
            Source: "middleware.response",
            UpdatedAtUtc: DateTimeOffset.UtcNow,
            AdditionalCounts: additionalCounts);

        _tokenSnapshotSink?.Invoke(snapshot);
        PublishCategoryEvents(snapshot);

        return Task.CompletedTask;
    }

    internal static TokenUsageSnapshot CreateContextSnapshot(IEnumerable<ChatMessage> messages, string source)
    {
        ArgumentNullException.ThrowIfNull(messages);

        IReadOnlyList<ChatMessage> capturedMessages = messages as IReadOnlyList<ChatMessage> ?? [.. messages];
        ContextBuckets buckets = ClassifyContextBuckets(capturedMessages);

        Dictionary<string, long> additionalCounts = new(StringComparer.OrdinalIgnoreCase)
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

        TokenUsageSnapshot snapshot = new(
            TotalTokens: buckets.Total,
            SessionTokens: buckets.Session,
            RagTokens: buckets.Rag,
            ToolTokens: buckets.Tool,
            SystemTokens: buckets.System,
            InputTokens: 0,
            OutputTokens: 0,
            CachedInputTokens: 0,
            ReasoningTokens: 0,
            Source: source,
            UpdatedAtUtc: DateTimeOffset.UtcNow,
            AdditionalCounts: additionalCounts);

        PublishCategoryEvents(snapshot);

        return snapshot;
    }

    public static event EventHandler<TokenCategoryChangedEventArgs>? CachedInputTokensChanged;
    public static event EventHandler<TokenCategoryChangedEventArgs>? InputTokensChanged;
    public static event EventHandler<TokenCategoryChangedEventArgs>? OutputTokensChanged;
    public static event EventHandler<TokenCategoryChangedEventArgs>? RagTokensChanged;
    public static event EventHandler<TokenCategoryChangedEventArgs>? ReasoningTokensChanged;
    public static event EventHandler<TokenCategoryChangedEventArgs>? SessionTokensChanged;
    public static event EventHandler<TokenCategoryChangedEventArgs>? SystemTokensChanged;
    public static event EventHandler<TokenCategoryChangedEventArgs>? ToolTokensChanged;
    public static event EventHandler<TokenCategoryChangedEventArgs>? TotalTokensChanged;

    private static ContextBuckets ClassifyContextBuckets(IReadOnlyList<ChatMessage> messages)
    {
        int sessionTokens = 0;
        int ragTokens = 0;
        int toolTokens = 0;
        int systemTokens = 0;

        foreach (ChatMessage message in messages)
        {
            int tokenCount = EstimateMessageTokens(message);
            string role = message.Role.Value;

            if (string.Equals(role, AIChatRole.System.Value, StringComparison.OrdinalIgnoreCase))
            {
                systemTokens += tokenCount;
            }
            else if (string.Equals(role, AIChatRole.Tool.Value, StringComparison.OrdinalIgnoreCase))
            {
                toolTokens += tokenCount;
            }
            else if (string.Equals(role, AIChatRole.RAGContext.Value, StringComparison.OrdinalIgnoreCase)
                     || string.Equals(role, AIChatRole.AIContext.Value, StringComparison.OrdinalIgnoreCase)
                     || string.Equals(role, "rag", StringComparison.OrdinalIgnoreCase))
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

    private static int EstimateMessageTokens(ChatMessage message)
    {
        string serialized = JsonSerializer.Serialize(message, JsonOptions);
        return Math.Max(1, (int)Math.Ceiling(serialized.Length / (double)CHARS_PER_TOKEN));
    }

    private static int EstimateMessagesTokens(IEnumerable<ChatMessage> messages)
    {
        int total = 0;
        foreach (ChatMessage message in messages)
        {
            total += EstimateMessageTokens(message);
        }

        return total;
    }

    private static int EstimateTokens(IEnumerable<string?> values)
    {
        int total = 0;
        foreach (string? value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            total += Math.Max(1, value.Length / CHARS_PER_TOKEN);
        }

        return total;
    }

    private static int ClampToInt(long value)
    {
        return value <= 0 ? 0 : value >= int.MaxValue ? int.MaxValue : (int)value;
    }

    private static UsageDetails NormalizeUsage(IReadOnlyList<ChatMessage> requestMessages, ChatResponse response)
    {
        UsageDetails usage = response.Usage ?? new UsageDetails();

        int usageInputTokens = ClampToInt(usage.InputTokenCount ?? 0);
        int usageOutputTokens = ClampToInt(usage.OutputTokenCount ?? 0);
        int usageTotalTokens = ClampToInt(usage.TotalTokenCount ?? 0);

        int inputTokens = usageInputTokens > 0
            ? usageInputTokens
            : EstimateTokens(requestMessages.Select(message => message.Text));

        int outputTokens = usageOutputTokens > 0
            ? usageOutputTokens
            : EstimateTokens((response.Messages ?? []).Select(message => message.Text));

        int totalTokens = usageTotalTokens > 0 ? usageTotalTokens : inputTokens + outputTokens;

        return new UsageDetails
        {
            InputTokenCount = inputTokens,
            OutputTokenCount = outputTokens,
            TotalTokenCount = totalTokens,
            CachedInputTokenCount = usage.CachedInputTokenCount ?? 0,
            ReasoningTokenCount = usage.ReasoningTokenCount ?? 0,
            AdditionalCounts = usage.AdditionalCounts is { Count: > 0 }
                ? new AdditionalPropertiesDictionary<long>(usage.AdditionalCounts)
                : new AdditionalPropertiesDictionary<long>()
        };
    }

    private static long GetAdditionalCount(UsageDetails usageDetails, params string[] keys)
    {
        if (usageDetails.AdditionalCounts is null || usageDetails.AdditionalCounts.Count == 0)
        {
            return 0;
        }

        HashSet<string> keySet = new(keys, StringComparer.OrdinalIgnoreCase);
        long total = 0;

        foreach ((string key, long value) in usageDetails.AdditionalCounts)
        {
            if (keySet.Contains(key))
            {
                total += value;
            }
        }

        return total;
    }

    private static void PublishCategoryEvents(TokenUsageSnapshot snapshot)
    {
        DateTimeOffset updatedAt = snapshot.UpdatedAtUtc;
        string source = snapshot.Source;

        lock (CategoryEventsGate)
        {
            RaiseIfChanged(CachedInputTokensChanged, "cached_input", LastPublishedCounts.CachedInput, snapshot.CachedInputTokens, source, updatedAt);
            RaiseIfChanged(InputTokensChanged, "input", LastPublishedCounts.Input, snapshot.InputTokens, source, updatedAt);
            RaiseIfChanged(OutputTokensChanged, "output", LastPublishedCounts.Output, snapshot.OutputTokens, source, updatedAt);
            RaiseIfChanged(RagTokensChanged, "rag", LastPublishedCounts.Rag, snapshot.RagTokens, source, updatedAt);
            RaiseIfChanged(ReasoningTokensChanged, "reasoning", LastPublishedCounts.Reasoning, snapshot.ReasoningTokens, source, updatedAt);
            RaiseIfChanged(SessionTokensChanged, "session", LastPublishedCounts.Session, snapshot.SessionTokens, source, updatedAt);
            RaiseIfChanged(SystemTokensChanged, "system", LastPublishedCounts.System, snapshot.SystemTokens, source, updatedAt);
            RaiseIfChanged(ToolTokensChanged, "tool", LastPublishedCounts.Tool, snapshot.ToolTokens, source, updatedAt);
            RaiseIfChanged(TotalTokensChanged, "total", LastPublishedCounts.Total, snapshot.TotalTokens, source, updatedAt);

            LastPublishedCounts = new CategoryCounts(
                snapshot.CachedInputTokens,
                snapshot.InputTokens,
                snapshot.OutputTokens,
                snapshot.RagTokens,
                snapshot.ReasoningTokens,
                snapshot.SessionTokens,
                snapshot.SystemTokens,
                snapshot.ToolTokens,
                snapshot.TotalTokens);
        }
    }

    private static void RaiseIfChanged(
        EventHandler<TokenCategoryChangedEventArgs>? categoryHandler,
        string category,
        int previousValue,
        int currentValue,
        string source,
        DateTimeOffset updatedAtUtc)
    {
        if (previousValue == currentValue)
        {
            return;
        }

        TokenCategoryChangedEventArgs args = new(category, previousValue, currentValue, source, updatedAtUtc);
        categoryHandler?.Invoke(null, args);
    }

    internal sealed record MiddlewareRequestContext(IReadOnlyList<ChatMessage> Messages, int EstimatedInputTokens);

    public sealed class TokenCategoryChangedEventArgs : EventArgs
    {
        public TokenCategoryChangedEventArgs(string category, int previousValue, int currentValue, string source, DateTimeOffset updatedAtUtc)
        {
            Category = category;
            PreviousValue = previousValue;
            CurrentValue = currentValue;
            Source = source;
            UpdatedAtUtc = updatedAtUtc;
        }

        public string Category { get; }
        public int PreviousValue { get; }
        public int CurrentValue { get; }
        public string Source { get; }
        public DateTimeOffset UpdatedAtUtc { get; }
    }

    private readonly record struct ContextBuckets(int Total, int Session, int Rag, int Tool, int System);

    private readonly record struct CategoryCounts(
        int CachedInput,
        int Input,
        int Output,
        int Rag,
        int Reasoning,
        int Session,
        int System,
        int Tool,
        int Total);
}
