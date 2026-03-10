// 2026/03/08
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         AIContextHistoryInjector.cs
//   Author: Kyle L. Crowder



using System.Text.Json;

using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;
using DataIngestionLib.Models.Extensions;
using DataIngestionLib.Options;

using Microsoft.Agents.AI;
using Microsoft.Extensions.Options;




namespace DataIngestionLib.Services.ContextInjectors;




/// <summary>
///     SQL-backed implementation of <see cref="IAIContextHistoryInjector" /> that builds windowed
///     context from persisted history, stores new messages after each interaction, and provides
///     conversation lifecycle management (pruning, updating, and deleting messages).
/// </summary>
/// <remarks>
///     This class also implements <see cref="IChatHistoryMemoryProvider" /> so that it can be
///     injected into context providers that only require the narrower read/write surface.
/// </remarks>
public sealed class AIContextHistoryInjector : IAIContextHistoryInjector
{
    private readonly IChatHistoryProvider _chatHistoryProvider;
    private readonly IOptionsMonitor<ChatHistoryOptions> _optionsMonitor;
    private readonly IChatHistorySummarizer? _summarizer;








    public AIContextHistoryInjector(IChatHistoryProvider chatHistoryProvider, IOptionsMonitor<ChatHistoryOptions> optionsMonitor, IChatHistorySummarizer? summarizer = null)
    {
        ArgumentNullException.ThrowIfNull(chatHistoryProvider);
        ArgumentNullException.ThrowIfNull(optionsMonitor);

        _chatHistoryProvider = chatHistoryProvider;
        _optionsMonitor = optionsMonitor;
        _summarizer = summarizer;
    }








    public async ValueTask<IEnumerable<AIChatMessage>> BuildContextMessagesAsync(
            string conversationId,
            ChatHistory currentRequestMessages,
            CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            throw new ArgumentException("Conversation id is required.", nameof(conversationId));
        }

        ArgumentNullException.ThrowIfNull(currentRequestMessages);

        ChatHistoryOptions options = _optionsMonitor.CurrentValue;
        IReadOnlyList<PersistedChatMessage> persistedMessages = await _chatHistoryProvider
                .GetMessagesAsync(conversationId.Trim(), take: null, cancellationToken)
                .ConfigureAwait(false);

        HashSet<string> requestMessageKeys = BuildMessageKeySet(currentRequestMessages);

        ChatHistory historicalMessages =
        [
                .. persistedMessages
                        .OrderBy(message => message.TimestampUtc)
                        .ThenBy(message => message.MessageId)
                        .Where(message => !string.IsNullOrWhiteSpace(message.Content))
                        .Select(static message => new AIChatMessage(ParseRole(message.Role), message.Content.Trim()))
                        .Where(message => !requestMessageKeys.Contains(CreateMessageKey(message)))
        ];

        return await ApplyWindowAsync(conversationId, historicalMessages, options, cancellationToken).ConfigureAwait(false);
    }








    public async ValueTask StoreMessagesAsync(
            string conversationId,
            string sessionId,
            string agentId,
            string userId,
            string applicationId,
            ChatHistory requestMessages,
            ChatHistory responseMessages,
            CancellationToken cancellationToken = default)
    {
        ValidateIdentifiers(conversationId, sessionId, agentId, userId, applicationId);
        ArgumentNullException.ThrowIfNull(requestMessages);
        ArgumentNullException.ThrowIfNull(responseMessages);

        ChatHistory filteredRequestMessages = requestMessages
                .Where(ShouldPersistRequestMessage)
                .ToArray();

        ChatHistory filteredResponseMessages = responseMessages
                .Where(ShouldPersistResponseMessage)
                .ToArray();

        ChatHistory messagesToStore = [.. filteredRequestMessages, .. filteredResponseMessages];
        if (messagesToStore.Count == 0)
        {
            return;
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;
        for (int index = 0; index < messagesToStore.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            AIChatMessage message = messagesToStore[index];

            PersistedChatMessage persistedMessage = new()
            {
                MessageId = Guid.NewGuid(),
                ConversationId = conversationId.Trim(),
                SessionId = sessionId.Trim(),
                AgentId = agentId.Trim(),
                UserId = userId.Trim(),
                ApplicationId = applicationId.Trim(),
                Role = message.Role.Value,
                Content = message.Text ?? string.Empty,
                TimestampUtc = now.AddTicks(index),
                Metadata = CreateMetadata(message)
            };

            await _chatHistoryProvider.CreateMessageAsync(persistedMessage, cancellationToken).ConfigureAwait(false);
        }

        await PruneConversationAsync(conversationId, cancellationToken).ConfigureAwait(false);
    }







    /// <summary>
    /// Removes excess messages from the conversation history, ensuring the total number of messages does not exceed the
    /// configured limit.
    /// </summary>
    /// <remarks>If the maximum context messages limit is set to zero or less, no messages will be removed.
    /// The method will only delete the oldest messages exceeding the configured limit.</remarks>
    /// <param name="conversationId">The unique identifier of the conversation from which messages will be pruned. This parameter cannot be null or
    /// whitespace.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation if needed.</param>
    /// <returns>The number of messages that were removed from the conversation history.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="conversationId"/> is null or whitespace.</exception>
    public async ValueTask<int> PruneConversationAsync(string conversationId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            throw new ArgumentException("Conversation id is required.", nameof(conversationId));
        }

        ChatHistoryOptions options = _optionsMonitor.CurrentValue;
        if (options.MaxContextMessages <= 0)
        {
            return 0;
        }

        IReadOnlyList<PersistedChatMessage> persistedMessages = await _chatHistoryProvider
                .GetMessagesAsync(conversationId.Trim(), take: null, cancellationToken)
                .ConfigureAwait(false);

        int overflow = persistedMessages.Count - options.MaxContextMessages;
        if (overflow <= 0)
        {
            return 0;
        }

        var messagesToDelete = persistedMessages
                .OrderBy(message => message.TimestampUtc)
                .ThenBy(message => message.MessageId)
                .Take(overflow)
                .ToList();

        int removedCount = 0;
        foreach (PersistedChatMessage message in messagesToDelete)
        {
            cancellationToken.ThrowIfCancellationRequested();
            bool removed = await _chatHistoryProvider.DeleteMessageAsync(message.MessageId, cancellationToken).ConfigureAwait(false);
            if (removed)
            {
                removedCount++;
            }
        }

        return removedCount;
    }







    /// <summary>
    /// Updates the content of a specific chat message in the chat history.
    /// </summary>
    /// <param name="messageId">The unique identifier of the message to be updated.</param>
    /// <param name="content">The new content to replace the existing message content.</param>
    /// <param name="timestampUtc">The timestamp indicating when the update occurred, in UTC.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the updated 
    /// <see cref="PersistedChatMessage"/> if the update was successful, or <c>null</c> if the message was not found.
    /// </returns>
    public ValueTask<PersistedChatMessage?> UpdateMessageContentAsync(Guid messageId, string content, DateTimeOffset timestampUtc, CancellationToken cancellationToken = default)
    {
        return _chatHistoryProvider.UpdateMessageAsync(messageId, content, timestampUtc, cancellationToken);
    }








    public ValueTask<bool> DeleteMessageAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        return _chatHistoryProvider.DeleteMessageAsync(messageId, cancellationToken);
    }







    /// <summary>
    /// Applies a windowing mechanism to the provided historical chat messages, ensuring that the resulting set
    /// adheres to the constraints defined in the <see cref="ChatHistoryOptions"/>.
    /// </summary>
    /// <param name="conversationId">
    /// The unique identifier of the conversation for which the windowing mechanism is applied.
    /// </param>
    /// <param name="historicalMessages">
    /// The collection of historical chat messages to be processed.
    /// </param>
    /// <param name="options">
    /// The configuration options that define the constraints for the windowing mechanism, such as the maximum
    /// number of messages and tokens.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// A <see cref="ChatHistory"/> containing the pruned and processed set of messages that fit within the
    /// specified constraints.
    /// </returns>
    /// <remarks>
    /// If summarization is enabled in the <paramref name="options"/> and the number of pruned messages exceeds
    /// zero, a summary may be generated and included in the resulting window. The method ensures that the
    /// resulting set of messages adheres to the constraints even after summarization.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="historicalMessages"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown if the operation is canceled via the <paramref name="cancellationToken"/>.
    /// </exception>
    private async ValueTask<ChatHistory> ApplyWindowAsync(
            string conversationId,
            ChatHistory historicalMessages,
            ChatHistoryOptions options,
            CancellationToken cancellationToken)
    {
        if (historicalMessages.Count == 0)
        {
            return [];
        }

        int maxMessages = options.MaxContextMessages <= 0 ? int.MaxValue : options.MaxContextMessages;
        int maxTokens = options.MaxContextTokens is null or <= 0 ? int.MaxValue : options.MaxContextTokens.Value;

        ChatHistory window = [.. historicalMessages];
        ChatHistory prunedMessages = [];

        while (window.Count > maxMessages || EstimateTokens(window) > maxTokens)
        {
            prunedMessages.Add(window[0]);
            window.RemoveAt(0);
        }

        if (window.Count == 0)
        {
            return [];
        }

        if (options.EnableSummarization && _summarizer is not null && prunedMessages.Count > 0)
        {
            AIChatMessage? summary = await _summarizer.SummarizeAsync(conversationId, prunedMessages, cancellationToken).ConfigureAwait(false);
            if (summary is not null && !string.IsNullOrWhiteSpace(summary.Text))
            {
                window.Insert(0, summary);
                while (window.Count > maxMessages || EstimateTokens(window) > maxTokens)
                {
                    if (window.Count <= 1)
                    {
                        window.Clear();
                        break;
                    }

                    window.RemoveAt(1);
                }
            }
        }

        return window;
    }








    private static HashSet<string> BuildMessageKeySet(IEnumerable<AIChatMessage> messages)
    {
        return messages
                .Where(static message => !string.IsNullOrWhiteSpace(message.Text))
                .Select(CreateMessageKey)
                .ToHashSet(StringComparer.Ordinal);
    }








    private static string CreateMessageKey(AIChatMessage message)
    {
        string role = message.Role.Value.Trim().ToLowerInvariant();
        string text = message.Text?.Trim() ?? string.Empty;
        return $"{role}\u001F{text}";
    }








    private static JsonDocument CreateMetadata(AIChatMessage message)
    {
        string sourceType = message.GetAgentRequestMessageSourceType().ToString();

        return JsonSerializer.SerializeToDocument(new Dictionary<string, string?>
        {
            ["sourceType"] = sourceType
        });
    }








    private static void EnsureNotEmpty(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }
    }








    private static int EstimateTokens(IEnumerable<AIChatMessage> messages)
    {
        return messages.Sum(static message =>
        {
            string text = message.Text ?? string.Empty;
            return string.IsNullOrWhiteSpace(text) ? 0 : Math.Max(1, text.Length / 4);
        });
    }








    private static AIChatRole ParseRole(string role)
    {
        return role.Trim().ToLowerInvariant() switch
        {
            "assistant" => AIChatRole.Assistant,
            "rag_context" => AIChatRole.RAGContext,
            "context" => AIChatRole.AIContext,
            "system" => AIChatRole.System,
            "tool" => AIChatRole.Tool,
            _ => AIChatRole.User
        };
    }








    private static bool ShouldPersistRequestMessage(AIChatMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.Text))
        {
            return false;
        }

        AgentRequestMessageSourceType sourceType = message.GetAgentRequestMessageSourceType();
        return sourceType == AgentRequestMessageSourceType.External && message.Role != AIChatRole.System;
    }








    private static bool ShouldPersistResponseMessage(AIChatMessage message)
    {
        return !string.IsNullOrWhiteSpace(message.Text) && message.Role != AIChatRole.System;
    }








    private static void ValidateIdentifiers(string conversationId, string sessionId, string agentId, string userId, string applicationId)
    {
        EnsureNotEmpty(conversationId, nameof(conversationId));
        EnsureNotEmpty(sessionId, nameof(sessionId));
        EnsureNotEmpty(agentId, nameof(agentId));
        EnsureNotEmpty(userId, nameof(userId));
        EnsureNotEmpty(applicationId, nameof(applicationId));
    }
}