// Build Date: 2026/03/31
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         WindowsWmiReaderTool.cs
// Author: Kyle L. Crowder
// Build Num: 232114



using System.ComponentModel;
using System.Management;




namespace DataIngestionLib.ToolFunctions;





public sealed class WindowsWmiInstanceDto
{
    public string ClassName { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> Properties { get; init; } = new Dictionary<string, string>();
}





internal sealed class AllowedWmiClassDefinition(string className, string[] defaultProperties, string[] allowedProperties)
{
    public IReadOnlyList<string> AllowedProperties { get; } = allowedProperties;
    public string ClassName { get; } = className;
    public IReadOnlyList<string> DefaultProperties { get; } = defaultProperties;
}





public sealed class WindowsWmiReaderTool
{
    private const int DefaultMaxResults = 5;
    private const int MaxAllowedResults = 20;
    private const int MaxValueLength = 256;

    private static readonly Dictionary<string, AllowedWmiClassDefinition> AllowedClasses = new(StringComparer.OrdinalIgnoreCase)
    {
            ["Win32_OperatingSystem"] = new("Win32_OperatingSystem", ["Caption", "Version", "LastBootUpTime", "FreePhysicalMemory", "TotalVisibleMemorySize"], ["Caption", "Version", "LastBootUpTime", "FreePhysicalMemory", "TotalVisibleMemorySize", "BuildNumber"]),
            ["Win32_ComputerSystem"] = new("Win32_ComputerSystem", ["Manufacturer", "Model", "TotalPhysicalMemory"], ["Manufacturer", "Model", "TotalPhysicalMemory", "Domain", "SystemType"]),
            ["Win32_Processor"] = new("Win32_Processor", ["Name", "NumberOfCores", "NumberOfLogicalProcessors", "LoadPercentage"], ["Name", "NumberOfCores", "NumberOfLogicalProcessors", "LoadPercentage", "MaxClockSpeed"]),
            ["Win32_LogicalDisk"] = new("Win32_LogicalDisk", ["DeviceID", "DriveType", "FileSystem", "FreeSpace", "Size"], ["DeviceID", "DriveType", "FileSystem", "FreeSpace", "Size", "VolumeName"]),
            ["Win32_Service"] = new("Win32_Service", ["Name", "State", "StartMode", "Status"], ["Name", "State", "StartMode", "Status", "DisplayName"]),
            ["Win32_NetworkAdapterConfiguration"] = new("Win32_NetworkAdapterConfiguration", ["Description", "IPEnabled", "IPAddress", "DefaultIPGateway"], ["Description", "IPEnabled", "IPAddress", "DefaultIPGateway", "DNSServerSearchOrder"])
    };








    private static string NormalizeValue(object? value)
    {
        return value switch
        {
                null => string.Empty,
                string text => Truncate(text),
                string[] items => Truncate(string.Join("; ", items.Where(item => !string.IsNullOrWhiteSpace(item)))),
                Array array => Truncate(string.Join("; ", array.Cast<object?>().Select(item => item?.ToString()).Where(item => !string.IsNullOrWhiteSpace(item)))),
                _ => Truncate(value.ToString() ?? string.Empty)
        };
    }








    [Description("Read bounded WMI data from an allowlisted set of Win32 classes for local diagnostics.")]
    public ToolResult<IReadOnlyList<WindowsWmiInstanceDto>> ReadClass([Description("Allowed WMI class name, such as Win32_OperatingSystem or Win32_Service.")] string className, [Description("Optional comma-separated property names. If omitted, a safe default set is used.")] string? properties = null, [Description("Maximum number of WMI rows to return. Range: 1 to 20.")] int maxResults = DefaultMaxResults)
    {
        if (string.IsNullOrWhiteSpace(className))
        {
            return ToolResult<IReadOnlyList<WindowsWmiInstanceDto>>.Fail("Class name cannot be empty.");
        }

        if (!OperatingSystem.IsWindows())
        {
            return ToolResult<IReadOnlyList<WindowsWmiInstanceDto>>.Fail("WMI is only supported on Windows.");
        }

        if (maxResults < 1 || maxResults > MaxAllowedResults)
        {
            return ToolResult<IReadOnlyList<WindowsWmiInstanceDto>>.Fail($"maxResults must be between 1 and {MaxAllowedResults}.");
        }

        if (!AllowedClasses.TryGetValue(className.Trim(), out AllowedWmiClassDefinition? definition))
        {
            return ToolResult<IReadOnlyList<WindowsWmiInstanceDto>>.Fail("Class is not allowlisted for diagnostics.");
        }

        var selectedProperties = ResolveProperties(definition, properties, out var error);
        if (error != null)
        {
            return ToolResult<IReadOnlyList<WindowsWmiInstanceDto>>.Fail(error);
        }

        try
        {
            var query = $"SELECT {string.Join(", ", selectedProperties)} FROM {definition.ClassName}";
            using ManagementObjectSearcher searcher = new(@"root\cimv2", query);
            using ManagementObjectCollection results = searcher.Get();

            List<WindowsWmiInstanceDto> rows = [];
            foreach (ManagementBaseObject instance in results)
            {
                Dictionary<string, string> values = new(StringComparer.OrdinalIgnoreCase);

                foreach (var propertyName in selectedProperties) values[propertyName] = NormalizeValue(instance[propertyName]);

                rows.Add(new WindowsWmiInstanceDto { ClassName = definition.ClassName, Properties = values });

                if (rows.Count >= maxResults)
                {
                    break;
                }
            }

            return ToolResult<IReadOnlyList<WindowsWmiInstanceDto>>.Ok(rows.AsReadOnly());
        }
        catch (ManagementException ex)
        {
            return ToolResult<IReadOnlyList<WindowsWmiInstanceDto>>.Fail($"WMI query failed: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            return ToolResult<IReadOnlyList<WindowsWmiInstanceDto>>.Fail($"Access denied while reading WMI: {ex.Message}");
        }
    }








    private static string[] ResolveProperties(AllowedWmiClassDefinition definition, string? properties, out string? error)
    {
        error = null;

        if (string.IsNullOrWhiteSpace(properties))
        {
            return definition.DefaultProperties.ToArray();
        }

        var requestedProperties = properties.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        if (requestedProperties.Length == 0)
        {
            error = "At least one property name must be provided when properties is specified.";
            return [];
        }

        var unsupportedProperty = requestedProperties.FirstOrDefault(property => !definition.AllowedProperties.Contains(property, StringComparer.OrdinalIgnoreCase));
        if (unsupportedProperty != null)
        {
            error = $"Property '{unsupportedProperty}' is not allowlisted for class '{definition.ClassName}'.";
            return [];
        }

        return requestedProperties;
    }








    private static string Truncate(string value)
    {
        if (value.Length <= MaxValueLength)
        {
            return value;
        }

        return value[..MaxValueLength] + "...";
    }
}