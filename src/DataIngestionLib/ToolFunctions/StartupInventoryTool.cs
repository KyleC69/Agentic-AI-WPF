// Build Date: 2026/03/30
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         StartupInventoryTool.cs
// Author: Kyle L. Crowder
// Build Num: 233145



using System.ComponentModel;
using System.Management;
using System.Runtime.InteropServices;




namespace DataIngestionLib.ToolFunctions;





public sealed class StartupInventoryEntry
{
    public string Location { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Origin { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
}





public sealed class StartupInventoryTool
{
    private const int MaxResults = 40;
    private const int MaxScheduledTasks = 15;








    private static IEnumerable<StartupInventoryEntry> ReadScheduledTasks(int maxResults)
    {
        List<StartupInventoryEntry> tasks = [];
        Type? serviceType = Type.GetTypeFromProgID("Schedule.Service");
        if (serviceType == null)
        {
            return tasks;
        }

        dynamic service = Activator.CreateInstance(serviceType)!;
        service.Connect();
        var rootFolder = service.GetFolder("\\");
        var registeredTasks = rootFolder.GetTasks(0);

        var count = Math.Min((int)registeredTasks.Count, maxResults);
        for (var index = 1; index <= count; index++)
        {
            var task = registeredTasks[index];
            tasks.Add(new StartupInventoryEntry { Type = "ScheduledTask", Name = DiagnosticsText.Truncate((string)task.Name, 128), Location = DiagnosticsText.Truncate((string)task.Path, 128), Origin = DiagnosticsText.Truncate((string)task.Definition.RegistrationInfo.Description) });
        }

        return tasks;
    }








    private static IEnumerable<StartupInventoryEntry> ReadStartupCommands(int maxResults)
    {
        using ManagementObjectSearcher searcher = new("root\\cimv2", "SELECT Command, Location, Name, User FROM Win32_StartupCommand");
        using ManagementObjectCollection results = searcher.Get();

        return results.Cast<ManagementBaseObject>().Take(maxResults).Select(item => new StartupInventoryEntry { Type = "StartupCommand", Name = DiagnosticsText.Truncate(item["Name"]?.ToString(), 128), Location = DiagnosticsText.Truncate(item["Location"]?.ToString(), 128), Origin = DiagnosticsText.Truncate(item["Command"]?.ToString()) }).ToList();
    }








    [Description("Read a bounded inventory of startup commands and scheduled tasks for local diagnostics.")]
    public ToolResult<IReadOnlyList<StartupInventoryEntry>> ReadStartupItems([Description("Maximum number of combined startup items to return. Range: 1 to 40.")] int maxResults = 20)
    {
        if (!OperatingSystem.IsWindows())
        {
            return ToolResult<IReadOnlyList<StartupInventoryEntry>>.Fail("Startup inventory is only supported on Windows.");
        }

        if (maxResults < 1 || maxResults > MaxResults)
        {
            return ToolResult<IReadOnlyList<StartupInventoryEntry>>.Fail($"maxResults must be between 1 and {MaxResults}.");
        }

        try
        {
            List<StartupInventoryEntry> items = [];
            items.AddRange(ReadStartupCommands(maxResults));
            items.AddRange(ReadScheduledTasks(Math.Min(maxResults, MaxScheduledTasks)));

            return ToolResult<IReadOnlyList<StartupInventoryEntry>>.Ok(items.OrderBy(item => item.Type, StringComparer.OrdinalIgnoreCase).ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase).Take(maxResults).ToList().AsReadOnly());
        }
        catch (ManagementException ex)
        {
            return ToolResult<IReadOnlyList<StartupInventoryEntry>>.Fail($"Startup inventory query failed: {ex.Message}");
        }
        catch (COMException ex)
        {
            return ToolResult<IReadOnlyList<StartupInventoryEntry>>.Fail($"Scheduled task query failed: {ex.Message}");
        }
    }
}