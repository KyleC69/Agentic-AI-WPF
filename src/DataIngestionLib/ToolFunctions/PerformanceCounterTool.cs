// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         PerformanceCounterTool.cs
// Author: Kyle L. Crowder
// Build Num: 212918



using System.ComponentModel;
using System.Diagnostics;




namespace DataIngestionLib.ToolFunctions;





public sealed class PerformanceCounterSnapshot
{
    public double AvailableMemoryMb { get; init; }
    public double CpuPercent { get; init; }
    public double DiskQueueLength { get; init; }
    public double NetworkBytesReceivedPerSecond { get; init; }
    public double NetworkBytesSentPerSecond { get; init; }
    public string SampleWindow { get; init; } = string.Empty;
}





public sealed class PerformanceCounterTool
{
    private const int MaxSampleDelayMilliseconds = 2000;








    private static string GetPreferredNetworkInstanceName()
    {
        try
        {
            PerformanceCounterCategory category = new("Network Interface");
            return category.GetInstanceNames().FirstOrDefault() ?? string.Empty;
        }
        catch (InvalidOperationException)
        {
            return string.Empty;
        }
    }








    [Description("Read a bounded local performance snapshot using allowlisted Windows performance counters.")]
    public ToolResult<PerformanceCounterSnapshot> ReadSnapshot([Description("Sampling delay in milliseconds. Range: 250 to 2000.")] int sampleDelayMilliseconds = 500)
    {
        if (!OperatingSystem.IsWindows())
        {
            return ToolResult<PerformanceCounterSnapshot>.Fail("Performance counters are only supported on Windows.");
        }

        if (sampleDelayMilliseconds is < 250 or > MaxSampleDelayMilliseconds)
        {
            return ToolResult<PerformanceCounterSnapshot>.Fail($"sampleDelayMilliseconds must be between 250 and {MaxSampleDelayMilliseconds}.");
        }

        try
        {
            using PerformanceCounter cpuCounter = new("Processor", "% Processor Time", "_Total", true);
            using PerformanceCounter memoryCounter = new("Memory", "Available MBytes", readOnly: true);
            using PerformanceCounter diskQueueCounter = new("PhysicalDisk", "Avg. Disk Queue Length", "_Total", true);

            var networkInstance = GetPreferredNetworkInstanceName();
            using PerformanceCounter? receivedCounter = networkInstance == string.Empty ? null : new PerformanceCounter("Network Interface", "Bytes Received/sec", networkInstance, true);
            using PerformanceCounter? sentCounter = networkInstance == string.Empty ? null : new PerformanceCounter("Network Interface", "Bytes Sent/sec", networkInstance, true);

            _ = cpuCounter.NextValue();
            _ = diskQueueCounter.NextValue();
            _ = receivedCounter?.NextValue();
            _ = sentCounter?.NextValue();
            Thread.Sleep(sampleDelayMilliseconds);

            return ToolResult<PerformanceCounterSnapshot>.Ok(new PerformanceCounterSnapshot
            {
                    CpuPercent = Math.Round(cpuCounter.NextValue(), 2),
                    AvailableMemoryMb = Math.Round(memoryCounter.NextValue(), 2),
                    DiskQueueLength = Math.Round(diskQueueCounter.NextValue(), 2),
                    NetworkBytesReceivedPerSecond = Math.Round(receivedCounter?.NextValue() ?? 0, 2),
                    NetworkBytesSentPerSecond = Math.Round(sentCounter?.NextValue() ?? 0, 2),
                    SampleWindow = $"{sampleDelayMilliseconds}ms"
            });
        }
        catch (Exception ex) when (ex is InvalidOperationException or UnauthorizedAccessException or PlatformNotSupportedException)
        {
            return ToolResult<PerformanceCounterSnapshot>.Fail($"Performance counter read failed: {ex.Message}");
        }
    }
}