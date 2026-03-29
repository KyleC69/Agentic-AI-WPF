// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ServiceHealthTool.cs
// Author: Kyle L. Crowder
// Build Num: 073016



using System.ComponentModel;
using System.Management;
using System.ServiceProcess;




namespace DataIngestionLib.ToolFunctions;





public sealed class ServiceHealthSnapshot
{
    public string DisplayName { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string StartMode { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}





public sealed class ServiceHealthTool
{
    private const int MaxServices = 25;








    private static string GetStartMode(string serviceName)
    {
        try
        {
            using ManagementObjectSearcher searcher = new("root\\cimv2", $"SELECT StartMode FROM Win32_Service WHERE Name='{serviceName.Replace("'", "''", StringComparison.Ordinal)}'");
            using ManagementObjectCollection? results = searcher.Get();
            ManagementBaseObject? service = results.Cast<ManagementBaseObject>().FirstOrDefault();
            return service?["StartMode"]?.ToString() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }








    [Description("Read a bounded snapshot of local Windows services. Optional name filter is matched against service and display names.")]
    public ToolResult<IReadOnlyList<ServiceHealthSnapshot>> ReadServices([Description("Optional substring filter for service name or display name.")] string? filter = null, [Description("Maximum number of services to return. Range: 1 to 25.")] int maxResults = 10)
    {
        if (!OperatingSystem.IsWindows())
        {
            return ToolResult<IReadOnlyList<ServiceHealthSnapshot>>.Fail("Service inspection is only supported on Windows.");
        }

        if (maxResults < 1 || maxResults > MaxServices)
        {
            return ToolResult<IReadOnlyList<ServiceHealthSnapshot>>.Fail($"maxResults must be between 1 and {MaxServices}.");
        }

        var normalizedFilter = filter?.Trim() ?? string.Empty;

        try
        {
            var services = ServiceController.GetServices();
            var snapshots = services.Where(service => string.IsNullOrEmpty(normalizedFilter) || service.ServiceName.Contains(normalizedFilter, StringComparison.OrdinalIgnoreCase) || service.DisplayName.Contains(normalizedFilter, StringComparison.OrdinalIgnoreCase)).OrderBy(service => service.ServiceName, StringComparer.OrdinalIgnoreCase).Take(maxResults).Select(service => new ServiceHealthSnapshot { Name = service.ServiceName, DisplayName = DiagnosticsText.Truncate(service.DisplayName, 128), Status = service.Status.ToString(), StartMode = GetStartMode(service.ServiceName) }).ToList().AsReadOnly();

            return ToolResult<IReadOnlyList<ServiceHealthSnapshot>>.Ok(snapshots);
        }
        catch (InvalidOperationException ex)
        {
            return ToolResult<IReadOnlyList<ServiceHealthSnapshot>>.Fail($"Service inspection failed: {ex.Message}");
        }
        catch (Win32Exception ex)
        {
            return ToolResult<IReadOnlyList<ServiceHealthSnapshot>>.Fail($"Service inspection failed: {ex.Message}");
        }
    }
}