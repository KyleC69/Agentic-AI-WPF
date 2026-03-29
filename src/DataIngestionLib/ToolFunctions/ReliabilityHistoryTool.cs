// Build Date: 2026/03/29
// Solution: File
// Project:   DataIngestionLib
// File:         ReliabilityHistoryTool.cs
// Author: Kyle L. Crowder
// Build Num: 051942



using System.ComponentModel;
using System.Management;




namespace DataIngestionLib.ToolFunctions;





public sealed class ReliabilityRecordSnapshot
{
    public string ProductName { get; init; } = string.Empty;
    public string RecordNumber { get; init; } = string.Empty;
    public string SourceName { get; init; } = string.Empty;
    public string TimeGenerated { get; init; } = string.Empty;
    public string User { get; init; } = string.Empty;
}





public sealed class ReliabilityHistoryTool
{
    private const int MaxResults = 25;








    [Description("Read bounded reliability history records for recent system instability and crash diagnosis.")]
    public ToolResult<IReadOnlyList<ReliabilityRecordSnapshot>> ReadRecent([Description("Maximum number of records to return. Range: 1 to 25.")] int maxResults = 10)
    {
        if (!OperatingSystem.IsWindows())
        {
            return ToolResult<IReadOnlyList<ReliabilityRecordSnapshot>>.Fail("Reliability history is only supported on Windows.");
        }

        if (maxResults < 1 || maxResults > MaxResults)
        {
            return ToolResult<IReadOnlyList<ReliabilityRecordSnapshot>>.Fail($"maxResults must be between 1 and {MaxResults}.");
        }

        try
        {
            using ManagementObjectSearcher searcher = new("root\\cimv2", "SELECT ProductName, RecordNumber, SourceName, TimeGenerated, User FROM Win32_ReliabilityRecords");
            using ManagementObjectCollection results = searcher.Get();

            var records = results.Cast<ManagementBaseObject>()
                    .OrderByDescending(record => record["TimeGenerated"]?.ToString(), StringComparer.OrdinalIgnoreCase)
                    .Take(maxResults)
                    .Select(record => new ReliabilityRecordSnapshot
                    {
                            ProductName = DiagnosticsText.Truncate(record["ProductName"]?.ToString()),
                            RecordNumber = DiagnosticsText.Truncate(record["RecordNumber"]?.ToString(), 32),
                            SourceName = DiagnosticsText.Truncate(record["SourceName"]?.ToString(), 128),
                            TimeGenerated = DiagnosticsText.Truncate(record["TimeGenerated"]?.ToString(), 64),
                            User = DiagnosticsText.Truncate(record["User"]?.ToString(), 64)
                    })
                    .ToList()
                    .AsReadOnly();

            return ToolResult<IReadOnlyList<ReliabilityRecordSnapshot>>.Ok(records);
        }
        catch (ManagementException ex)
        {
            return ToolResult<IReadOnlyList<ReliabilityRecordSnapshot>>.Fail($"Reliability history query failed: {ex.Message}");
        }
    }
}