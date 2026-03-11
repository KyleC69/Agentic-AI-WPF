// Build Date: 2026/03/11
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         SystemInfoTool.cs
// Author: Kyle L. Crowder
// Build Num: 105650



namespace DataIngestionLib.ToolFunctions;





public sealed class SystemInfoTool
{
    public ToolResult<SystemInfoSnapshot> GetInfo()
    {
        return ToolResult<SystemInfoSnapshot>.Ok(new()
        {
                OS = Environment.OSVersion.ToString(),
                MachineName = Environment.MachineName,
                ProcessorCount = Environment.ProcessorCount,
                DotNetVersion = Environment.Version.ToString()
        });
    }
}





public sealed class SystemInfoSnapshot
{
    public string DotNetVersion { get; set; } = "";
    public string MachineName { get; set; } = "";
    public string OS { get; set; } = "";
    public int ProcessorCount { get; set; }
}