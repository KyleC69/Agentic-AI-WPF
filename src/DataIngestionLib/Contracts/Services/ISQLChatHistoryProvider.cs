// Build Date: 2026/03/29
// Solution: File
// Project:   DataIngestionLib
// File:         ISQLChatHistoryProvider.cs
// Author: Kyle L. Crowder
// Build Num: 051918



namespace DataIngestionLib.Contracts.Services;





/// <summary>
///     Extends <see cref="IChatHistoryProvider" /> with SQL Server-specific capabilities.
/// </summary>
public interface ISQLChatHistoryProvider : IChatHistoryProvider
{
    /// <summary>
    ///     Returns the most recent conversation ID for the specified agent, user, and application.
    /// </summary>
    /// <param name="agentId">The agent ID to match.</param>
    /// <param name="userId">The user ID to match.</param>
    /// <param name="applicationId">The application ID to match.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    ValueTask<string?> GetLatestConversationIdAsync(string agentId, string userId, string applicationId, CancellationToken cancellationToken = default);
}