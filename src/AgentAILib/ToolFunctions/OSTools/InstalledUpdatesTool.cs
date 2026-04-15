// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         InstalledUpdatesTool.cs
// Author: Kyle L. Crowder
// Build Num: 194512



using System.ComponentModel;
using System.Management;

using AgentAILib.ToolFunctions.Utils;




namespace AgentAILib.ToolFunctions.OSTools;





public sealed class InstalledUpdateSnapshot
{
    public string Description { get; init; } = string.Empty;
    public string HotFixId { get; init; } = string.Empty;
    public string InstalledBy { get; init; } = string.Empty;
    public string InstalledOn { get; init; } = string.Empty;
}





public sealed class InstalledUpdatesTool
{
    private const int MaxResults = 25;








    [Description("Read a bounded snapshot of installed Windows hotfixes and updates.")]
    public ToolResult<IReadOnlyList<InstalledUpdateSnapshot>> ReadInstalledUpdates([Description("Maximum number of updates to return. Range: 1 to 25.")] int maxResults = 10)
    {
        if (!OperatingSystem.IsWindows())
        {
            return ToolResult<IReadOnlyList<InstalledUpdateSnapshot>>.Fail("Installed update inspection is only supported on Windows.");
        }

        if (maxResults < 1 || maxResults > MaxResults)
        {
            return ToolResult<IReadOnlyList<InstalledUpdateSnapshot>>.Fail($"maxResults must be between 1 and {MaxResults}.");
        }

        try
        {
            using ManagementObjectSearcher searcher = new("root\\cimv2", "SELECT Description, HotFixID, InstalledBy, InstalledOn FROM Win32_QuickFixEngineering");
            using ManagementObjectCollection results = searcher.Get();

            var updates = results.Cast<ManagementBaseObject>().OrderByDescending(record => record["InstalledOn"]?.ToString(), StringComparer.OrdinalIgnoreCase).Take(maxResults).Select(record => new InstalledUpdateSnapshot { Description = DiagnosticsText.Truncate(record["Description"]?.ToString(), 128), HotFixId = DiagnosticsText.Truncate(record["HotFixID"]?.ToString(), 64), InstalledBy = DiagnosticsText.Truncate(record["InstalledBy"]?.ToString(), 64), InstalledOn = DiagnosticsText.Truncate(record["InstalledOn"]?.ToString(), 64) }).ToList().AsReadOnly();

            return ToolResult<IReadOnlyList<InstalledUpdateSnapshot>>.Ok(updates);
        }
        catch (ManagementException ex)
        {
            return ToolResult<IReadOnlyList<InstalledUpdateSnapshot>>.Fail($"Installed update query failed: {ex.Message}");
        }
    }
}