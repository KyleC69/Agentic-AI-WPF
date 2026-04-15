// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         EventErrorsTool.cs
// Author: Kyle L. Crowder
// Build Num: 194511



using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;

using AgentAILib.ToolFunctions.Utils;




namespace AgentAILib.ToolFunctions.OSTools;





public sealed class EventErrorEntry
{
    public int EventId { get; init; }
    public string Level { get; init; } = string.Empty;
    public string LogName { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string ProviderName { get; init; } = string.Empty;
    public DateTime? TimeCreated { get; init; }
}





internal interface IEventLogQuerySession
{
    EventLogReader CreateReader(string logName, string query);


    IReadOnlyList<string> GetLogNames();
}





internal sealed class EventLogQuerySession : IEventLogQuerySession
{
    public EventLogReader CreateReader(string logName, string query)
    {
        EventLogQuery eventLogQuery = new(logName, PathType.LogName, query) { ReverseDirection = true };

        return new EventLogReader(eventLogQuery);
    }








    public IReadOnlyList<string> GetLogNames()
    {
        using EventLogSession session = new();

        return session.GetLogNames().ToList().AsReadOnly();
    }
}





public sealed class EventErrorsTool
{

    private readonly IEventLogQuerySession _querySession;
    internal const int DEFAULT_LOOKBACK_HOURS = 6;
    internal const int MAX_EVENTS_TO_RETURN = 25;
    internal const int MAX_LOGS_TO_SCAN = 25;
    private const int MAX_MESSAGE_LENGTH = 1200;








    public EventErrorsTool() : this(new EventLogQuerySession())
    {
    }








    internal EventErrorsTool(IEventLogQuerySession querySession)
    {
        _querySession = querySession;
    }








    internal static string BuildCriticalAndErrorQuery(int lookbackHours)
    {
        var milliseconds = (long)TimeSpan.FromHours(lookbackHours).TotalMilliseconds;

        return $"*[System[(Level=1 or Level=2) and TimeCreated[timediff(@SystemTime) <= {milliseconds}]]]";
    }








    internal static IReadOnlyList<string> GetCandidateLogs(IReadOnlyList<string> availableLogs)
    {
        return availableLogs.Where(logName => !string.IsNullOrWhiteSpace(logName)).OrderBy(logName => GetLogPriority(logName)).ThenBy(logName => logName, StringComparer.OrdinalIgnoreCase).Take(MAX_LOGS_TO_SCAN).ToList().AsReadOnly();
    }








    private static int GetLogPriority(string logName)
    {
        if (string.Equals(logName, "System", StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        if (string.Equals(logName, "Application", StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        if (string.Equals(logName, "Setup", StringComparison.OrdinalIgnoreCase))
        {
            return 2;
        }

        if (logName.EndsWith("/Admin", StringComparison.OrdinalIgnoreCase))
        {
            return 3;
        }

        if (logName.EndsWith("/Operational", StringComparison.OrdinalIgnoreCase))
        {
            return 4;
        }

        return 5;
    }








    private static string NormalizeLevel(string? levelDisplayName, byte? level)
    {
        if (!string.IsNullOrWhiteSpace(levelDisplayName))
        {
            return levelDisplayName;
        }

        return level switch
        {
                1 => "Critical",
                2 => "Error",
                3 => "Warning",
                4 => "Information",
                5 => "Verbose",
                _ => "Unknown"
        };
    }








    [Description("Read recent critical and error events from local Windows event logs. Scans at most 25 logs and returns at most 25 newest matching events from the last 6 hours by default.")]
    public ToolResult<IReadOnlyList<EventErrorEntry>> ReadRecentCriticalAndErrorEvents([Description("Number of hours to look back for critical and error events. Range: 1 to 24.")] int lookbackHours = DEFAULT_LOOKBACK_HOURS)
    {
        if (!OperatingSystem.IsWindows())
        {
            return ToolResult<IReadOnlyList<EventErrorEntry>>.Fail("Windows event log queries are only supported on Windows.");
        }

        if (lookbackHours < 1 || lookbackHours > 24)
        {
            return ToolResult<IReadOnlyList<EventErrorEntry>>.Fail("lookbackHours must be between 1 and 24.");
        }

        try
        {
            var query = BuildCriticalAndErrorQuery(lookbackHours);
            var candidateLogs = GetCandidateLogs(_querySession.GetLogNames());
            List<EventErrorEntry> matchingEvents = [];

            foreach (var logName in candidateLogs)
            {
                try
                {
                    using EventLogReader reader = _querySession.CreateReader(logName, query);

                    for (EventRecord? record = reader.ReadEvent(); record != null; record = reader.ReadEvent())
                    {
                        using (record)
                        {
                            matchingEvents.Add(new EventErrorEntry
                            {
                                    EventId = record.Id,
                                    Level = NormalizeLevel(record.LevelDisplayName, record.Level),
                                    LogName = logName,
                                    Message = DiagnosticsText.Truncate(TryFormatMessage(record), MAX_MESSAGE_LENGTH),
                                    ProviderName = DiagnosticsText.Truncate(record.ProviderName ?? string.Empty, 128),
                                    TimeCreated = record.TimeCreated
                            });
                        }
                    }
                }
                catch (EventLogNotFoundException)
                {
                }
                catch (EventLogException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }

            var newestEvents = matchingEvents.OrderByDescending(entry => entry.TimeCreated ?? DateTime.MinValue).ThenBy(entry => entry.LogName, StringComparer.OrdinalIgnoreCase).Take(MAX_EVENTS_TO_RETURN).ToList().AsReadOnly();

            return ToolResult<IReadOnlyList<EventErrorEntry>>.Ok(newestEvents);
        }
        catch (EventLogException ex)
        {
            return ToolResult<IReadOnlyList<EventErrorEntry>>.Fail($"Windows event log query failed: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            return ToolResult<IReadOnlyList<EventErrorEntry>>.Fail($"Access denied while querying Windows event logs: {ex.Message}");
        }
    }








    private static string TryFormatMessage(EventRecord record)
    {
        try
        {
            return record.FormatDescription() ?? string.Empty;
        }
        catch (EventLogException)
        {
            return string.Empty;
        }
    }
}