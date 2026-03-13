// Build Date: 2026/03/12
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         LoggingLevelService.cs
// Author: Kyle L. Crowder



using System.Windows;

using Microsoft.Extensions.Logging;

using RAGDataIngestionWPF.Contracts.Services;
using RAGDataIngestionWPF.Models;




namespace RAGDataIngestionWPF.Services;




/// <summary>
/// Manages the application's runtime minimum log level.
/// Changes are applied immediately via the shared <see cref="LoggingLevelSwitch"/>
/// that was captured in the host's logging filter lambda at startup, and the
/// selection is persisted to <see cref="Application.Current"/>.<see cref="Application.Properties"/>
/// so that it is restored by <see cref="IPersistAndRestoreService"/> on the next run.
/// </summary>
public sealed class LoggingLevelService : ILoggingLevelService
{
    private const string PropertiesKey = "MinimumLogLevel";

    private readonly LoggingLevelSwitch _levelSwitch;




    /// <param name="levelSwitch">
    /// The singleton switch that was captured by the logging filter lambda during host construction.
    /// Writing to this switch immediately changes the effective minimum level for every active logger.
    /// </param>
    public LoggingLevelService(LoggingLevelSwitch levelSwitch)
    {
        _levelSwitch = levelSwitch;

        // Restore the persisted level, if any, as early as possible so that
        // all services that log during startup already use the preferred level.
        if (Application.Current?.Properties.Contains(PropertiesKey) == true)
        {
            string? stored = Application.Current.Properties[PropertiesKey]?.ToString();
            if (Enum.TryParse(stored, out LogLevel persisted))
            {
                _levelSwitch.MinimumLevel = persisted;
            }
        }
    }




    /// <inheritdoc />
    public LogLevel GetMinimumLevel() => _levelSwitch.MinimumLevel;




    /// <inheritdoc />
    public void SetMinimumLevel(LogLevel level)
    {
        _levelSwitch.MinimumLevel = level;

        if (Application.Current != null)
        {
            Application.Current.Properties[PropertiesKey] = level.ToString();
        }
    }
}
