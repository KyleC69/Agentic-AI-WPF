// Build Date: 2026/03/19
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ISQLChatHistoryProvider.cs
// Author: Kyle L. Crowder
// Build Num: 044230



namespace DataIngestionLib.Contracts.Services;





/// <summary>
///     Captures the most recent conversation and session identifiers that were active at the end
///     of the previous application run, allowing the next run to resume from where it left off.
/// </summary>
/// <param name="ConversationId">
///     The conversation identifier from the last persisted session.
/// </param>
/// <param name="SessionId">
///     The session identifier from the last persisted session.
/// </param>
public sealed record ChatHistorySessionSnapshot(string ConversationId, string SessionId);





/// <summary>
///     Extends <see cref="IChatHistoryProvider" /> with SQL Server-specific capabilities.
/// </summary>
public interface ISQLChatHistoryProvider : IChatHistoryProvider
{
    /// <summary>
    ///     Returns the most recent session snapshot (conversation ID and session ID) recorded in the
    ///     database, or <see langword="null" /> when no history exists yet.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    ValueTask<ChatHistorySessionSnapshot?> GetLatestSessionSnapshotAsync(CancellationToken cancellationToken = default);
}