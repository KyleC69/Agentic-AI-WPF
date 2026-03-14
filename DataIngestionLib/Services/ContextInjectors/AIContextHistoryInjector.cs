// Build Date: 2026/03/13
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         AIContextHistoryInjector.cs
// Author: Kyle L. Crowder
// Build Num: 175055



using System.Text.Json;
using System.Windows.Automation.Peers;

using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;
using DataIngestionLib.Models.Extensions;

using Microsoft.Agents.AI;

using SystemConfigurationManager = System.Configuration.ConfigurationManager;




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
    private readonly IChatHistorySummarizer? _summarizer;








    public AIContextHistoryInjector(IChatHistoryProvider chatHistoryProvider, IChatHistorySummarizer? summarizer = null)
    {
        ArgumentNullException.ThrowIfNull(chatHistoryProvider);

        _chatHistoryProvider = chatHistoryProvider;
        _summarizer = summarizer;
    }








    /// <summary>
    ///     Builds the historical context messages for a conversation, excluding duplicates already
    ///     present in the current request and applying configured message/token window limits.
    /// </summary>
    /// <param name="conversationId">The conversation identifier whose persisted history should be loaded.</param>
    /// <param name="currentRequestMessages">The current request messages used to deduplicate persisted history.</param>
    /// <param name="cancellationToken">A token to observe while loading and shaping context history.</param>
    /// <returns>An ordered sequence of context messages to inject into the current agent request.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="conversationId" /> is null, empty, or whitespace.</exception>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="currentRequestMessages" /> is
    ///     <see langword="null" />.
    /// </exception>
    public async ValueTask<IEnumerable<AIChatMessage>> BuildContextMessagesAsync(
            string conversationId,
            AIChatHistory currentRequestMessages,
            CancellationToken cancellationToken = default)
    {
        //When building context messages to inject we need to assign the AICChatRole.Context role to messages being added.
        //This makes removal of the added context on the round trip in the Store Override


        if (string.IsNullOrWhiteSpace(conversationId))
        {
            throw new ArgumentException("Conversation id is required.", nameof(conversationId));
        }

        ArgumentNullException.ThrowIfNull(currentRequestMessages);

        var maxContextMessages = GetMaxContextMessages();
        var maxContextTokens = GetMaxContextTokens() ?? int.MaxValue;
        var persistedMessages = await _chatHistoryProvider
                .GetMessagesAsync(conversationId.Trim(), take: null, cancellationToken)
                .ConfigureAwait(false);

        var requestMessageKeys = BuildMessageKeySet(currentRequestMessages);

        AIChatHistory historicalMessages =
        [
                .. persistedMessages
                        .OrderBy(message => message.TimestampUtc)
                        .ThenBy(message => message.MessageId)
                        .Where(message => !string.IsNullOrWhiteSpace(message.Content))
                        .Select(static message => new AIChatMessage(ParseRole(message.Role), message.Content.Trim()))
                        .Where(message => !requestMessageKeys.Contains(CreateMessageKey(message)))
        ];

        return await ApplyWindowAsync(historicalMessages, maxContextMessages, maxContextTokens, cancellationToken).ConfigureAwait(false);
    }








    /// <summary>
    ///     Stores the specified request and response chat messages for a conversation, associating them with the provided
    ///     identifiers.
    /// </summary>
    /// <remarks>
    ///     Only messages that meet the persistence criteria are stored. If no messages are eligible for
    ///     storage, the method completes without performing any operation. The method also prunes the conversation history
    ///     as needed after storing messages.
    /// </remarks>
    /// <param name="conversationId">
    ///     The unique identifier for the conversation to which the messages belong. Cannot be null or
    ///     empty.
    /// </param>
    /// <param name="sessionId">The unique identifier for the session within the conversation. Cannot be null or empty.</param>
    /// <param name="agentId">The unique identifier of the agent involved in the conversation. Cannot be null or empty.</param>
    /// <param name="userId">The unique identifier of the user participating in the conversation. Cannot be null or empty.</param>
    /// <param name="applicationId">
    ///     The unique identifier of the application context for the conversation. Cannot be null or
    ///     empty.
    /// </param>
    /// <param name="requestMessages">The collection of chat messages sent as requests in the conversation. Cannot be null.</param>
    /// <param name="responseMessages">The collection of chat messages sent as responses in the conversation. Cannot be null.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous store operation.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when any required identifier (<paramref name="conversationId" />, <paramref name="sessionId" />,
    ///     <paramref name="agentId" />, <paramref name="userId" />, or <paramref name="applicationId" />) is null,
    ///     empty, or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="requestMessages" /> or <paramref name="responseMessages" /> is
    ///     <see langword="null" />.
    /// </exception>
    public async ValueTask StoreMessagesAsync(
            string conversationId,
            string sessionId,
            string agentId,
            string userId,
            string applicationId,
            AIChatHistory requestMessages,
            AIChatHistory responseMessages,
            CancellationToken cancellationToken = default)
    {
        ValidateIdentifiers(conversationId, sessionId, agentId, userId, applicationId);
        ArgumentNullException.ThrowIfNull(requestMessages);
        ArgumentNullException.ThrowIfNull(responseMessages);
        AIChatHistory filteredRequestMessages = FilterMessages(requestMessages, ShouldPersistRequestMessage);
        AIChatHistory filteredResponseMessages = FilterMessages(responseMessages, ShouldPersistResponseMessage);

        AIChatHistory messagesToStore = new();
        messagesToStore.AddRange(filteredRequestMessages);
        messagesToStore.AddRange(filteredResponseMessages);
        
        if (messagesToStore.Count == 0)
        {
            return;
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;
        for (var index = 0; index < messagesToStore.Count; index++)
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

            PersistedChatMessage unused1 = await _chatHistoryProvider.CreateMessageAsync(persistedMessage, cancellationToken).ConfigureAwait(false);
        }

        var unused = await PruneConversationAsync(conversationId, cancellationToken).ConfigureAwait(false);
    }








    /// <summary>
    ///     Removes excess messages from the conversation history, ensuring the total number of messages does not exceed the
    ///     configured limit.
    /// </summary>
    /// <remarks>
    ///     If the maximum context messages limit is set to zero or less, no messages will be removed.
    ///     The method will only delete the oldest messages exceeding the configured limit.
    /// </remarks>
    /// <param name="conversationId">
    ///     The unique identifier of the conversation from which messages will be pruned. This parameter cannot be null or
    ///     whitespace.
    /// </param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation if needed.</param>
    /// <returns>The number of messages that were removed from the conversation history.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="conversationId" /> is null or whitespace.</exception>
    public async ValueTask<int> PruneConversationAsync(string conversationId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            throw new ArgumentException("Conversation id is required.", nameof(conversationId));
        }

        var maxContextMessages = GetMaxContextMessages();
        if (maxContextMessages <= 0)
        {
            return 0;
        }

        var persistedMessages = await _chatHistoryProvider
                .GetMessagesAsync(conversationId.Trim(), take: null, cancellationToken)
                .ConfigureAwait(false);

        var overflow = persistedMessages.Count - maxContextMessages;
        if (overflow <= 0)
        {
            return 0;
        }

        List<PersistedChatMessage> messagesToDelete =
        [
                .. persistedMessages
                        .OrderBy(message => message.TimestampUtc)
                        .ThenBy(message => message.MessageId)
                        .Take(overflow)
        ];

        var removedCount = 0;
        foreach (PersistedChatMessage message in messagesToDelete)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var removed = await _chatHistoryProvider.DeleteMessageAsync(message.MessageId, cancellationToken).ConfigureAwait(false);
            if (removed)
            {
                removedCount++;
            }
        }

        return removedCount;
    }








    /// <summary>
    ///     Updates the content of a specific chat message in the chat history.
    /// </summary>
    /// <param name="messageId">The unique identifier of the message to be updated.</param>
    /// <param name="content">The new content to replace the existing message content.</param>
    /// <param name="timestampUtc">The timestamp indicating when the update occurred, in UTC.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the updated
    ///     <see cref="PersistedChatMessage" /> if the update was successful, or <c>null</c> if the message was not found.
    /// </returns>
    public ValueTask<PersistedChatMessage?> UpdateMessageContentAsync(Guid messageId, string content, DateTimeOffset timestampUtc, CancellationToken cancellationToken = default)
    {
        return _chatHistoryProvider.UpdateMessageAsync(messageId, content, timestampUtc, cancellationToken);
    }








    /// <summary>
    ///     Deletes a single message from persisted chat history.
    /// </summary>
    /// <param name="messageId">The unique identifier of the message to delete.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the delete operation.</param>
    /// <returns>
    ///     <see langword="true" /> when the message existed and was deleted; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public ValueTask<bool> DeleteMessageAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        return _chatHistoryProvider.DeleteMessageAsync(messageId, cancellationToken);
    }








    /// <summary>
    ///     Applies a windowing mechanism to the provided historical chat messages using configured message and token
    ///     limits.
    /// </summary>
    /// <param name="historicalMessages">
    ///     The collection of historical chat messages to be processed.
    /// </param>
    /// <param name="maxMessages">
    ///     The maximum number of messages retained in context.
    /// </param>
    /// <param name="maxTokens">
    ///     The maximum estimated token count retained in context.
    /// </param>
    /// <param name="cancellationToken">
    ///     A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///     A <see cref="AIChatHistory" /> containing the pruned and processed set of messages that fit within the
    ///     specified constraints.
    /// </returns>
    /// <remarks>
    ///     The method retains only the newest messages that fit within both limits.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="historicalMessages" /> is <c>null</c>.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown if the operation is canceled via the <paramref name="cancellationToken" />.
    /// </exception>
    private async ValueTask<AIChatHistory> ApplyWindowAsync(
            AIChatHistory historicalMessages,
            int maxMessages,
            int maxTokens,
            CancellationToken cancellationToken)
    {
        if (historicalMessages.Count == 0)
        {
            return [];
        }

        maxMessages = maxMessages <= 0 ? int.MaxValue : maxMessages;
        maxTokens = maxTokens <= 0 ? int.MaxValue : maxTokens;

        AIChatHistory window = new();
        window.AddRange(historicalMessages);
        AIChatHistory prunedMessages = [];

        while (window.Count > maxMessages || EstimateTokens(window) > maxTokens)
        {
            ((ICollection<AIChatMessage>)prunedMessages).Add(window[0]);
            window.RemoveAt(0);
        }

        return window.Count == 0 ? [] : window;
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
        var role = message.Role.Value.Trim().ToLowerInvariant();
        var text = message.Text?.Trim() ?? string.Empty;
        return $"{role}\u001F{text}";
    }








    private static JsonDocument CreateMetadata(AIChatMessage message)
    {
        var sourceType = message.GetAgentRequestMessageSourceType().ToString();

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
            var text = message.Text ?? string.Empty;
            return string.IsNullOrWhiteSpace(text) ? 0 : Math.Max(1, text.Length / 4);
        });
    }








    private static AIChatHistory FilterMessages(AIChatHistory messages, Func<AIChatMessage, bool> shouldPersist)
    {
        ArgumentNullException.ThrowIfNull(messages);
        ArgumentNullException.ThrowIfNull(shouldPersist);

        AIChatHistory filteredMessages = [];
        foreach (AIChatMessage message in messages)
        {
            if (shouldPersist(message))
            {
                ((ICollection<AIChatMessage>)filteredMessages).Add(message);
            }
        }

        return filteredMessages;
    }








    private static int GetMaxContextMessages()
    {
        var raw = SystemConfigurationManager.AppSettings["MaxContextMessages"] ?? string.Empty;
        return int.TryParse(raw, out var parsed) && parsed > 0 ? parsed : 40;
    }








    private static int? GetMaxContextTokens()
    {
        var raw = SystemConfigurationManager.AppSettings["MaxContextTokens"] ?? string.Empty;
        return string.IsNullOrWhiteSpace(raw) ? null : int.TryParse(raw, out var parsed) && parsed > 0 ? parsed : 120000;
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