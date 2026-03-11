// Build Date: 2026/03/11
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IAIContextHistoryInjector.cs
// Author: Kyle L. Crowder
// Build Num: 105648



using DataIngestionLib.Models;




namespace DataIngestionLib.Contracts.Services;





/// <summary>
///     Defines the full management surface for AI chat context history, including building context
///     messages, storing messages, pruning conversations, updating message content, and deleting
///     messages.
/// </summary>
/// <remarks>
///     This interface extends <see cref="IChatHistoryMemoryProvider" /> with lifecycle management
///     operations. Services that only need to read and write history for context injection should
///     depend on <see cref="IChatHistoryMemoryProvider" /> instead.
/// </remarks>
public interface IAIContextHistoryInjector : IChatHistoryMemoryProvider
{


    /// <summary>
    ///     Permanently deletes a single chat message from the history.
    /// </summary>
    /// <param name="messageId">The unique identifier of the message to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    ///     <see langword="true" /> when the message was found and deleted; otherwise
    ///     <see langword="false" />.
    /// </returns>
    ValueTask<bool> DeleteMessageAsync(Guid messageId, CancellationToken cancellationToken = default);








    /// <summary>
    ///     Removes the oldest messages from a conversation so that the total number of stored
    ///     messages stays within the configured <c>MaxContextMessages</c> limit.
    /// </summary>
    /// <param name="conversationId">The conversation to prune.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The number of messages that were deleted.</returns>
    ValueTask<int> PruneConversationAsync(string conversationId, CancellationToken cancellationToken = default);








    /// <summary>
    ///     Updates the content of a specific chat message in the history.
    /// </summary>
    /// <param name="messageId">The unique identifier of the message to update.</param>
    /// <param name="content">The new text content to store.</param>
    /// <param name="timestampUtc">The UTC timestamp indicating when the update occurred.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    ///     The updated <see cref="PersistedChatMessage" /> when the message was found, or
    ///     <see langword="null" /> when no message with the given <paramref name="messageId" /> exists.
    /// </returns>
    ValueTask<PersistedChatMessage?> UpdateMessageContentAsync(
            Guid messageId,
            string content,
            DateTimeOffset timestampUtc,
            CancellationToken cancellationToken = default);
}