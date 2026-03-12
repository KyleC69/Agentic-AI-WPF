// Build Date: 2026/03/12
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         ILoggingLevelService.cs
// Author: Kyle L. Crowder



using Microsoft.Extensions.Logging;




namespace RAGDataIngestionWPF.Contracts.Services;




/// <summary>
/// Provides runtime control over the application's minimum log level.
/// The selected level is persisted across sessions and applied immediately
/// to all active loggers without requiring a host restart.
/// </summary>
public interface ILoggingLevelService
{
    /// <summary>Gets the currently active minimum log level.</summary>
    LogLevel GetMinimumLevel();

    /// <summary>
    /// Applies <paramref name="level"/> immediately to all active loggers
    /// and persists the selection so it is restored on the next startup.
    /// </summary>
    void SetMinimumLevel(LogLevel level);
}
