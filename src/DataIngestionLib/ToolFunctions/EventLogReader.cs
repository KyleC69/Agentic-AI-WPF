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



using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;




namespace DataIngestionLib.ToolFunctions;





public sealed class EventLogEntryDto
{
    public EventLogEntryType EntryType { get; init; }
    public int EventId { get; init; }
    public string Message { get; init; } = "";
    public string Source { get; init; } = "";
    public DateTime TimeGenerated { get; init; }
}





public sealed class EventLogReadResult
{
    public IReadOnlyList<EventLogEntryDto>? Entries { get; init; }
    public string? Error { get; init; }
    public bool Success { get; init; }








    public static EventLogReadResult Fail(string message)
    {
        return new() { Success = false, Error = message };
    }








    public static EventLogReadResult Ok(IReadOnlyList<EventLogEntryDto> entries)
    {
        return new() { Success = true, Entries = entries };
    }
}





public sealed class SandboxEventLogReader(int maxEvents = 100)
{
    private readonly int _maxEvents = Math.Max(1, maxEvents);







    [Description("An event log reader by log name, returns 100 of the newest events.")]
    public ToolResult<IReadOnlyList<EventLogEntryDto>> ReadLog(string logName)
    {
        if (string.IsNullOrWhiteSpace(logName))
        {
            return ToolResult<IReadOnlyList<EventLogEntryDto>>.Fail("Log name cannot be empty.");
        }

        if (!EventLog.Exists(logName))
        {
            return ToolResult<IReadOnlyList<EventLogEntryDto>>.Fail($"Event log '{logName}' does not exist.");
        }

        try
        {
            using EventLog log = new(logName);

            ReadOnlyCollection<EventLogEntryDto> entries = log.Entries.Cast<EventLogEntry>()
                    .Reverse() // newest first
                    .Take(_maxEvents)
                    .Select(e => new EventLogEntryDto
                    {
                        TimeGenerated = e.TimeGenerated,
                        Source = e.Source,
                        Message = e.Message,
                        EventId = e.InstanceId > int.MaxValue ? 0 : (int)e.InstanceId,
                        EntryType = e.EntryType
                    })
                    .ToList()
                    .AsReadOnly();

            return ToolResult<IReadOnlyList<EventLogEntryDto>>.Ok(entries);
        }
        catch (System.Security.SecurityException ex)
        {
            return ToolResult<IReadOnlyList<EventLogEntryDto>>.Fail($"Security exception while reading event log: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            return ToolResult<IReadOnlyList<EventLogEntryDto>>.Fail($"Access denied while reading event log: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            return ToolResult<IReadOnlyList<EventLogEntryDto>>.Fail($"Invalid event log operation: {ex.Message}");
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            return ToolResult<IReadOnlyList<EventLogEntryDto>>.Fail($"Windows error while reading event log: {ex.Message}");
        }
    }
}