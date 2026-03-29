// Build Date: 2026/03/29
// Solution: File
// Project:   DataIngestionLib
// File:         WindowsEventChannelReaderTool.cs
// Author: Kyle L. Crowder
// Build Num: 051944



using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;




namespace DataIngestionLib.ToolFunctions;





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
    private const int DefaultMaxEvents = 20;
    private const int MaxAllowedEvents = 50;
    private const int MaxMessageLength = 1200;

    private static readonly HashSet<string> AllowedChannels =
    [
            "Application",
            "System",
            "Setup",
            "Microsoft-Windows-Diagnostics-Performance/Operational",
            "Microsoft-Windows-WMI-Activity/Operational",
            "Microsoft-Windows-WindowsUpdateClient/Operational"
    ];








    [Description("Read recent entries from an allowlisted Windows event channel for local diagnostics.")]
    public ToolResult<IReadOnlyList<WindowsEventChannelEntryDto>> ReadChannel([Description("Allowed event channel name, for example 'System' or 'Microsoft-Windows-Diagnostics-Performance/Operational'.")] string channelName, [Description("Maximum number of recent events to return. Range: 1 to 50.")] int maxEvents = DefaultMaxEvents)
    {
        if (string.IsNullOrWhiteSpace(channelName))
        {
            return ToolResult<IReadOnlyList<WindowsEventChannelEntryDto>>.Fail("Channel name cannot be empty.");
        }

        if (!OperatingSystem.IsWindows())
        {
            return ToolResult<IReadOnlyList<WindowsEventChannelEntryDto>>.Fail("Windows event channels are only supported on Windows.");
        }

        if (maxEvents < 1 || maxEvents > MaxAllowedEvents)
        {
            return ToolResult<IReadOnlyList<WindowsEventChannelEntryDto>>.Fail($"maxEvents must be between 1 and {MaxAllowedEvents}.");
        }

        var normalizedChannelName = AllowedChannels.FirstOrDefault(channel => string.Equals(channel, channelName.Trim(), StringComparison.OrdinalIgnoreCase));
        if (normalizedChannelName == null)
        {
            return ToolResult<IReadOnlyList<WindowsEventChannelEntryDto>>.Fail("Channel is not allowlisted for diagnostics.");
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
        if (value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength] + "...";
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