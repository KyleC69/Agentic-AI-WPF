// Copyright (c) Your Organization. All rights reserved.



using Microsoft.Agents.Builder;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using KernelChatHistory = Microsoft.SemanticKernel.ChatCompletion.ChatHistory;



// ITurnContext
// ChatMessageContent



// ChatHistory, AuthorRole

namespace DataIngestionLib.Providers.ChatHistory;

/// <summary>
/// Contract for a SQL-backed chat history provider that offers granular multi-tenant
/// isolation via a composite (ApplicationId / UserId / AgentId / ConversationId) key.
///
/// Lifecycle in a turn handler:
/// <list type="number">
///   <item>Build a <see cref="ChatHistoryKey"/> from the incoming <see cref="ITurnContext"/>
///         using <see cref="BuildKey"/>.</item>
///   <item>Call <see cref="LoadHistoryAsync(ChatHistoryKey,CancellationToken)"/> to retrieve
///         the conversation's existing messages.</item>
///   <item>Append the user message and any assistant response via
///         <see cref="AppendMessageAsync(ChatHistoryKey,ChatMessageContent,ActivityMetadata?,CancellationToken)"/>.</item>
/// </list>
/// </summary>
public interface IChatHistoryProvider
{
    // ── Key building ──────────────────────────────────────────────────────────

    /// <summary>
    /// Constructs a <see cref="ChatHistoryKey"/> by combining the enterprise-supplied
    /// <paramref name="applicationId"/> and <paramref name="agentId"/> with conversation
    /// identity fields extracted from <paramref name="turnContext"/>.
    ///
    /// Propagated from the activity:
    /// <list type="bullet">
    ///   <item><c>UserId</c>        ← <c>Activity.From.Id</c></item>
    ///   <item><c>ConversationId</c> ← <c>Activity.Conversation.Id</c></item>
    ///   <item><c>ChannelId</c>     ← <c>Activity.ChannelId</c></item>
    ///   <item><c>TenantId</c>      ← <c>Activity.Conversation.TenantId</c></item>
    /// </list>
    /// </summary>
    ChatHistoryKey BuildKey(ITurnContext turnContext, Guid applicationId, string agentId);

    // ── Load ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Loads the ordered chat history for the given composite key.
    /// Returns an empty <see cref="ChatHistory"/> when no prior messages exist.
    /// </summary>
    Task<KernelChatHistory> LoadHistoryAsync(
        ChatHistoryKey key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Convenience overload that builds the key internally from <paramref name="turnContext"/>.
    /// </summary>
    Task<KernelChatHistory> LoadHistoryAsync(
        ITurnContext turnContext,
        Guid applicationId,
        string agentId,
        CancellationToken cancellationToken = default);

    // ── Append ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Appends a single <see cref="ChatMessageContent"/> to the persisted history.
    /// Optionally attaches <paramref name="activityMetadata"/> for full provenance tracking.
    ///
    /// Use this method for incremental persistence (append-only) rather than
    /// overwriting the full history, which is more efficient and audit-friendly.
    /// </summary>
    Task AppendMessageAsync(
        ChatHistoryKey key,
        ChatMessageContent message,
        ActivityMetadata? activityMetadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Convenience overload that builds the key and extracts activity metadata automatically
    /// from <paramref name="turnContext"/>.
    /// </summary>
    Task AppendMessageAsync(
        ITurnContext turnContext,
        Guid applicationId,
        string agentId,
        ChatMessageContent message,
        CancellationToken cancellationToken = default);

    // ── Save (full replace) ───────────────────────────────────────────────────

    /// <summary>
    /// Replaces the entire stored history for <paramref name="key"/> with
    /// <paramref name="history"/>.  Performs a DELETE + INSERT batch.
    ///
    /// Prefer <see cref="AppendMessageAsync"/> for normal turn processing.
    /// Use this method for bulk imports or after in-memory compression/reduction.
    /// </summary>
    Task SaveHistoryAsync(
        ChatHistoryKey key,
        KernelChatHistory history,
        CancellationToken cancellationToken = default);

    // ── Delete ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Permanently deletes all messages for the given composite key.
    /// Supports right-to-erasure workflows.
    /// </summary>
    Task DeleteHistoryAsync(
        ChatHistoryKey key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all conversations owned by <paramref name="userId"/> across all agents
    /// within <paramref name="applicationId"/>.
    /// Supports right-to-erasure workflows operating at the user level.
    /// </summary>
    Task DeleteUserHistoryAsync(
        Guid applicationId,
        string userId,
        CancellationToken cancellationToken = default);

    // ── Query ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Lists the distinct conversation IDs for a specific user/agent combination.
    /// </summary>
    Task<IReadOnlyList<string>> ListConversationIdsAsync(
        Guid applicationId,
        string userId,
        string agentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns fully-qualified <see cref="ChatHistoryKey"/> records for every conversation
    /// a user has had across all agents within the application, ordered by most recent first.
    /// </summary>
    Task<IReadOnlyList<ChatHistoryKey>> ListUserConversationsAsync(
        Guid applicationId,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns fully-qualified <see cref="ChatHistoryKey"/> records for every conversation
    /// handled by a specific agent, ordered by most recent first.
    /// Useful for agent-level analytics and auditing.
    /// </summary>
    Task<IReadOnlyList<ChatHistoryKey>> ListAgentConversationsAsync(
        Guid applicationId,
        string agentId,
        CancellationToken cancellationToken = default);
}
