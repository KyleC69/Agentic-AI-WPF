// Build Date: 2026/03/31
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         SystemInfoTool.cs
// Author: Kyle L. Crowder
// Build Num: 232112



namespace DataIngestionLib.ToolFunctions;





/// <summary>
///     Provides functionality to retrieve system information, including operating system details,
///     machine name, processor count, and .NET runtime version.
/// </summary>
public sealed class SystemInfoTool
{
    public static ToolResult<SystemInfoSnapshot> GetInfo()
    {
        return ToolResult<SystemInfoSnapshot>.Ok(new() { Os = Environment.OSVersion.ToString(), MachineName = Environment.MachineName, ProcessorCount = Environment.ProcessorCount, DotNetVersion = Environment.Version.ToString() });
    }
}





public sealed class SystemInfoSnapshot
{
    public string DotNetVersion { get; init; } = "";
    public string MachineName { get; init; } = "";
    public string Os { get; init; } = "";
    public int ProcessorCount { get; init; }
}