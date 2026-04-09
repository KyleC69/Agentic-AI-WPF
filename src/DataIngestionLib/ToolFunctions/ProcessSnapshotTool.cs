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





public sealed class ProcessSnapshot
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string StartTimeUtc { get; init; } = string.Empty;
    public int ThreadCount { get; init; }
    public long WorkingSetMb { get; init; }
}





public sealed class ProcessSnapshotTool
{
    private const int MaxResults = 20;








    private static ProcessSnapshot? CreateSnapshot(Process process)
    {
        using (process)
        {
            try
            {
                return new ProcessSnapshot
                {
                    Name = DiagnosticsText.Truncate(process.ProcessName, 64),
                    Id = process.Id,
                    WorkingSetMb = (long)Math.Round(process.WorkingSet64 / 1024d / 1024d, MidpointRounding.AwayFromZero),
                    ThreadCount = process.Threads.Count,
                    StartTimeUtc = SafeGetStartTime(process)
                };
            }
            catch (Exception ex) when (ex is InvalidOperationException or NotSupportedException or Win32Exception)
            {
                return null;
            }
        }
    }








    [Description("Read a bounded process snapshot ordered by working set memory.")]
    public ToolResult<IReadOnlyList<ProcessSnapshot>> ReadTopProcesses([Description("Maximum number of processes to return. Range: 1 to 20.")] int maxResults = 10)
    {
        if (maxResults is < 1 or > MaxResults)
        {
            return ToolResult<IReadOnlyList<ProcessSnapshot>>.Fail($"maxResults must be between 1 and {MaxResults}.");
        }

        try
        {
            ReadOnlyCollection<ProcessSnapshot> processes = Process.GetProcesses().Select(CreateSnapshot).Where(snapshot => snapshot != null).Cast<ProcessSnapshot>().OrderByDescending(snapshot => snapshot.WorkingSetMb).Take(maxResults).ToList().AsReadOnly();

            return ToolResult<IReadOnlyList<ProcessSnapshot>>.Ok(processes);
        }
        catch (InvalidOperationException ex)
        {
            return ToolResult<IReadOnlyList<ProcessSnapshot>>.Fail($"Process inspection failed: {ex.Message}");
        }
    }








    private static string SafeGetStartTime(Process process)
    {
        try
        {
            return process.StartTime
                    .ToUniversalTime()
                    .ToString("O", System.Globalization.CultureInfo.InvariantCulture);
        }
        catch
        {
            return string.Empty;
        }
    }
}