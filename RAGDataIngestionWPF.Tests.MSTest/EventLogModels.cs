// Build Date: 2026/03/11
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         Class1.cs
// Author: Kyle L. Crowder
// Build Num: 105601



using System.Collections.ObjectModel;
using System.Diagnostics;





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
    public IReadOnlyList<EventLogEntryDto> Entries { get; init; }
    public string Error { get; init; }
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








    public EventLogReadResult ReadLog(string logName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(logName))
            {
                return EventLogReadResult.Fail("Log name cannot be empty.");
            }

            if (!EventLog.Exists(logName))
            {
                return EventLogReadResult.Fail($"Event log '{logName}' does not exist.");
            }

            using EventLog log = new(logName);

            ReadOnlyCollection<EventLogEntryDto> entries = log.Entries
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

            return EventLogReadResult.Ok(entries);
        }
        catch (Exception ex)
        {
            return EventLogReadResult.Fail(ex.Message);
        }
    }
}