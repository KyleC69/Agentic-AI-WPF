// Build Date: 2026/03/30
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         StorageHealthTool.cs
// Author: Kyle L. Crowder
// Build Num: 233145



using System.ComponentModel;
using System.IO;




namespace DataIngestionLib.ToolFunctions;





public sealed class StorageHealthSnapshot
{
    public string DeviceId { get; init; } = string.Empty;
    public string DriveType { get; init; } = string.Empty;
    public string FileSystem { get; init; } = string.Empty;
    public string FreeSpaceGb { get; init; } = string.Empty;
    public string SizeGb { get; init; } = string.Empty;
    public string VolumeName { get; init; } = string.Empty;
}





public sealed class StorageHealthTool
{
    private const int MaxResults = 20;








    private static string FormatGigabytes(long bytes)
    {
        return Math.Round(bytes / 1024d / 1024d / 1024d, 2).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
    }








    [Description("Read a bounded storage health snapshot for local logical disks.")]
    public ToolResult<IReadOnlyList<StorageHealthSnapshot>> ReadLogicalDisks([Description("Maximum number of logical disks to return. Range: 1 to 20.")] int maxResults = 10)
    {
        if (!OperatingSystem.IsWindows())
        {
            return ToolResult<IReadOnlyList<StorageHealthSnapshot>>.Fail("Storage inspection is only supported on Windows.");
        }

        if (maxResults < 1 || maxResults > MaxResults)
        {
            return ToolResult<IReadOnlyList<StorageHealthSnapshot>>.Fail($"maxResults must be between 1 and {MaxResults}.");
        }

        try
        {
            var disks = DriveInfo.GetDrives()
                    .Where(drive => drive.IsReady)
                    .OrderBy(drive => drive.Name, StringComparer.OrdinalIgnoreCase)
                    .Take(maxResults)
                    .Select(drive => new StorageHealthSnapshot
                    {
                            DeviceId = drive.Name,
                            DriveType = drive.DriveType.ToString(),
                            FileSystem = DiagnosticsText.Truncate(drive.DriveFormat, 32),
                            FreeSpaceGb = FormatGigabytes(drive.AvailableFreeSpace),
                            SizeGb = FormatGigabytes(drive.TotalSize),
                            VolumeName = DiagnosticsText.Truncate(drive.VolumeLabel, 64)
                    })
                    .ToList()
                    .AsReadOnly();

            return ToolResult<IReadOnlyList<StorageHealthSnapshot>>.Ok(disks);
        }
        catch (IOException ex)
        {
            return ToolResult<IReadOnlyList<StorageHealthSnapshot>>.Fail($"Storage inspection failed: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            return ToolResult<IReadOnlyList<StorageHealthSnapshot>>.Fail($"Storage inspection failed: {ex.Message}");
        }
    }
}