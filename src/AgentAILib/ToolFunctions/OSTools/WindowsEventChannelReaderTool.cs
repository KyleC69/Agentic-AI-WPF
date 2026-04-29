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





public sealed class WindowsEventChannelEntryDto
{
    public int EventId { get; init; }
    public string Level { get; init; } = string.Empty;
    public string LogName { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string ProviderName { get; init; } = string.Empty;
    public DateTime? TimeCreated { get; init; }
}





public sealed class WindowsEventChannelReaderTool
{
    private const int DefaultMaxEvents = 50;
    private const int MaxAllowedEvents = 100;
    private const int MaxMessageLength = 1200;








    [Description("Reads recent entries from a single event log on the Windows Host machine for local diagnostics.")]
    public ToolResult<IReadOnlyList<WindowsEventChannelEntryDto>> ReadEventLogChannel([Description("Enter an event log name, for example 'System' or 'Microsoft-Windows-Diagnostics-Performance/Operational'.")] string channelName, [Description("Maximum number of recent events to return. Range: 1 to 50.")] int maxEvents = DefaultMaxEvents)
    {
        if (string.IsNullOrWhiteSpace(channelName))
        {
            return ToolResult<IReadOnlyList<WindowsEventChannelEntryDto>>.Fail("Channel name cannot be empty.");
        }

        if (!OperatingSystem.IsWindows())
        {
            return ToolResult<IReadOnlyList<WindowsEventChannelEntryDto>>.Fail("Application only supports Windows clients.");
        }

        if (maxEvents is < 1 or > MaxAllowedEvents)
        {
            return ToolResult<IReadOnlyList<WindowsEventChannelEntryDto>>.Fail($"maxEvents must be between 1 and {MaxAllowedEvents}.");
        }

        var normalizedChannelName = channelName.Trim();
        if (string.IsNullOrWhiteSpace(normalizedChannelName))
        {
            return ToolResult<IReadOnlyList<WindowsEventChannelEntryDto>>.Fail("Channel name cannot be empty.");
        }

        try
        {
            using EventLogSession session = new();
            var availableChannel = session.GetLogNames().FirstOrDefault(log => string.Equals(log, normalizedChannelName, StringComparison.OrdinalIgnoreCase));
            if (availableChannel == null)
            {
                return ToolResult<IReadOnlyList<WindowsEventChannelEntryDto>>.Fail($"Event channel '{normalizedChannelName}' is not available on this machine.");
            }

            EventLogQuery query = new(availableChannel, PathType.LogName) { ReverseDirection = true };

            using EventLogReader reader = new(query);
            List<WindowsEventChannelEntryDto> entries = [];

            for (EventRecord? record = reader.ReadEvent(); record != null && entries.Count < maxEvents; record = reader.ReadEvent())
            {
                using (record)
                {
                    entries.Add(new WindowsEventChannelEntryDto
                    {
                        EventId = record.Id,
                        Level = record.LevelDisplayName ?? record.Level?.ToString() ?? "Unknown",
                        LogName = availableChannel,
                        Message = Truncate(TryFormatMessage(record)),
                        ProviderName = Truncate(record.ProviderName ?? string.Empty, 128),
                        TimeCreated = record.TimeCreated
                    });
                }
            }

            return ToolResult<IReadOnlyList<WindowsEventChannelEntryDto>>.Ok(entries.AsReadOnly());
        }
        catch (EventLogNotFoundException ex)
        {
            return ToolResult<IReadOnlyList<WindowsEventChannelEntryDto>>.Fail($"Event channel '{normalizedChannelName}' was not found: {ex.Message}");
        }
        catch (EventLogException ex)
        {
            return ToolResult<IReadOnlyList<WindowsEventChannelEntryDto>>.Fail($"Windows event channel read failed: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            return ToolResult<IReadOnlyList<WindowsEventChannelEntryDto>>.Fail($"Access denied while reading event channel: {ex.Message}");
        }
    }








    private static string Truncate(string value, int maxLength = MaxMessageLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength] + "...";
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