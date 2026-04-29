// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//
//
//
//



using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;

using AgentAILib.ToolFunctions.Utils;




namespace AgentAILib.ToolFunctions.OSTools;





public sealed class EventSummary
{
    public int EventId { get; init; }
    public string Level { get; init; } = string.Empty;
    public string LogName { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public DateTime? TimeCreated { get; init; }
}





public sealed class EventLogSweepTool
{
    [Description("Reads warnings, errors and critical events from all event logs within the last X hours, returning up to MaxPerLog events per log.")]
    public ToolResult<List<EventSummary>> ReadRecentWarningsAndErrorsAcrossAllLogs([Description("Number of hours to look back.")] int hours, [Description("Maximum number of events to return per log.")] int maxPerLog)
    {
        if (hours <= 0)
        {
            return ToolResult<List<EventSummary>>.Fail("Hours must be greater than zero.");
        }

        if (maxPerLog <= 0)
        {
            return ToolResult<List<EventSummary>>.Fail("MaxPerLog must be greater than zero.");
        }

        try
        {
            EventLogSession session = new();
            IEnumerable<string> logNames = session.GetLogNames();
            List<EventSummary> results = new();

            var msWindow = hours * 3600000L;

            foreach (var logName in logNames)
            {
                try
                {
                    
                    EventLogQuery query = new(logName, PathType.LogName, $"*[System[(Level=1 or Level=2 or Level=3) and TimeCreated[timediff(@SystemTime) <= {msWindow}]]]");

                    using EventLogReader reader = new(query);

                    var count = 0;

                    while (count < maxPerLog)
                    {
                        using EventRecord? record = reader.ReadEvent();
                        if (record is null)
                        {
                            break;
                        }

                        var level = record.Level switch
                        {   1 => "Critical",
                            2 => "Error",
                            3 => "Warning",
                            _ => "Unknown"
                        };

                        var message = string.Empty;
                        try
                        {
                            message = DiagnosticsText.CleanModelText(record.FormatDescription() ?? string.Empty);
                        }
                        catch
                        {
                            message = "Unable to read event message.";
                        }

                        results.Add(new EventSummary
                        {
                            LogName = logName,
                            EventId = record.Id,
                            Level = level,
                            TimeCreated = record.TimeCreated,
                            Message = message
                        });

                        count++;
                    }
                }
                catch (Exception ex)
                {
                    // Per-log failure is isolated; we continue scanning others.
                    results.Add(new EventSummary
                    {
                        LogName = logName,
                        EventId = -1,
                        Level = "Error",
                        TimeCreated = DateTime.UtcNow,
                        Message = $"Failed to read log '{logName}': {ex.Message}"
                    });
                }
            }

            return ToolResult<List<EventSummary>>.Ok(results);
        }
        catch (Exception ex)
        {
            return ToolResult<List<EventSummary>>.Fail($"Failed to enumerate event logs: {ex.Message}");
        }
    }
}