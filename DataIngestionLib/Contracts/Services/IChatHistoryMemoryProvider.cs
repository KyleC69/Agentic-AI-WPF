// 2026/03/08
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         IChatHistoryMemoryProvider.cs
//   Author: Kyle L. Crowder



using DataIngestionLib.Models;




namespace DataIngestionLib.Contracts.Services;




/// <summary>
///     Defines the contract for providing and persisting chat history context during AI agent interactions.
/// </summary>
/// <remarks>
///     This interface is intentionally narrowed to the two operations required for context injection
///     (retrieving history for context and storing new messages), rather than exposing the full
///     management surface of <see cref="IAIContextHistoryInjector" />. Consumers that only need to
///     read and write history should depend on this interface so they remain decoupled from pruning,
///     update, and delete operations.
/// </remarks>
public interface IChatHistoryMemoryProvider
{
    /// <summary>
    ///     Builds a windowed set of historical chat messages suitable for injecting into the current
    ///     request context.
    /// </summary>
    /// <param name="conversationId">The conversation whose history should be retrieved.</param>
    /// <param name="currentRequestMessages">
    ///     The messages already present in the current request; used to deduplicate history entries.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    ///     An ordered collection of <see cref="AIChatMessage" /> values representing the relevant
    ///     historical context, or an empty collection when no history is available.
    /// </returns>
    ValueTask<IEnumerable<AIChatMessage>> BuildContextMessagesAsync(
            string conversationId,
            ChatHistory currentRequestMessages,
            CancellationToken cancellationToken = default);


    /// <summary>
    ///     Persists the request and response messages from a completed interaction into the
    ///     conversation history.
    /// </summary>
    /// <param name="conversationId">The conversation to which the messages belong.</param>
    /// <param name="sessionId">The session in which the interaction took place.</param>
    /// <param name="agentId">The identifier of the agent that produced the response.</param>
    /// <param name="userId">The identifier of the user who initiated the request.</param>
    /// <param name="applicationId">The identifier of the hosting application.</param>
    /// <param name="requestMessages">The messages sent by the user or context providers.</param>
    /// <param name="responseMessages">The messages produced by the agent.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    ValueTask StoreMessagesAsync(
            string conversationId,
            string sessionId,
            string agentId,
            string userId,
            string applicationId,
            ChatHistory requestMessages,
            ChatHistory responseMessages,
            CancellationToken cancellationToken = default);
}
