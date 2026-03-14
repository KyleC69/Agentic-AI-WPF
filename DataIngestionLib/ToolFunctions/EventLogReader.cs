// Build Date: 2026/03/13
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         EventLogReader.cs
// Author: Kyle L. Crowder
// Build Num: 175059



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





public sealed class SandboxEventLogReader
{
    private readonly int _maxEvents;








    public SandboxEventLogReader(int maxEvents = 100)
    {
        _maxEvents = Math.Max(1, maxEvents);
    }








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

            var entries = log.Entries
                    .Cast<EventLogEntry>()
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