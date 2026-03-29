// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         RegistryReaderTool.cs
// Author: Kyle L. Crowder
// Build Num: 073014



using DataIngestionLib.Services;

using Microsoft.Extensions.Logging;




namespace DataIngestionLib.ToolFunctions;





public sealed class RegistryValueSnapshot
{
    public string Hive { get; init; } = string.Empty;
    public string KeyPath { get; init; } = string.Empty;
    public string ValueKind { get; init; } = string.Empty;
    public string ValueName { get; init; } = string.Empty;
    public string ValueText { get; init; } = string.Empty;
}





/// <summary>
///     Simple tool to read registry values, will fail gracefully on security errors or if the registry key doesn't exist.
/// </summary>
public class RegistryReaderTool
{
    private readonly ILogger<RegistryReaderTool> _logger;








    public RegistryReaderTool(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<RegistryReaderTool>();
    }








    private static string FormatRegistryValue(object value)
    {
        return value switch
        {
                string text => Truncate(text),
                string[] items => Truncate(string.Join("; ", items.Where(item => !string.IsNullOrWhiteSpace(item)))),
                byte[] bytes => Convert.ToHexString(bytes.AsSpan(0, Math.Min(bytes.Length, 64))),
                _ => Truncate(value.ToString() ?? string.Empty)
        };
    }








    /// <summary>
    ///     Reads a string value from the Windows Registry.
    /// </summary>
    /// <param name="keyPath">The full path to the registry key (e.g., "HKEY_CURRENT_USER\Software\MyApplication\Setting").</param>
    /// <returns>A <see cref="ToolResult{T}" /> with the value on success, or an error message on failure.</returns>
    public ToolResult<string> ReadStringValue(string keyPath)
    {
        var result = ReadValue(keyPath);

        return result.Success ? ToolResult<string>.Ok(result.Value!.ValueText) : ToolResult<string>.Fail(result.Error!);
    }








    /// <summary>
    ///     Reads a registry value and returns bounded metadata suitable for diagnostics.
    /// </summary>
    /// <param name="keyPath">The full path to the registry key and value (e.g., "HKEY_LOCAL_MACHINE\Software\Vendor\Value").</param>
    /// <returns>A <see cref="ToolResult{T}" /> that contains a normalized snapshot of the registry value.</returns>
    public ToolResult<RegistryValueSnapshot> ReadValue(string keyPath)
    {
        if (string.IsNullOrWhiteSpace(keyPath))
        {
            const string message = "Registry key path cannot be null or empty.";
            _logger.LogRegistryKeyPathCannotBeNullOrEmpty();
            return ToolResult<RegistryValueSnapshot>.Fail(message);
        }

        if (!OperatingSystem.IsWindows())
        {
            const string message = "Registry access is only supported on Windows.";
            _logger.LogRegistryAccessIsOnlySupportedOnWindows();
            return ToolResult<RegistryValueSnapshot>.Fail(message);
        }

        try
        {
            var separatorIndex = keyPath.IndexOf('\\');
            if (separatorIndex <= 0)
            {
                const string message = "Invalid registry key path format.";
                _logger.LogMessagePathKeypath(message, keyPath);
                return ToolResult<RegistryValueSnapshot>.Fail(message);
            }

            var hiveName = keyPath[..separatorIndex];
            var keyAndValuePath = keyPath[(separatorIndex + 1)..];
            if (string.IsNullOrWhiteSpace(keyAndValuePath))
            {
                const string message = "Registry subkey path cannot be empty.";
                _logger.LogMessagePathKeypath(message, keyPath);
                return ToolResult<RegistryValueSnapshot>.Fail(message);
            }

            if (!TryResolveBaseKey(hiveName, out Microsoft.Win32.RegistryKey? baseKey))
            {
                var message = $"Unsupported registry hive: {hiveName}";
                _logger.LogError(message);
                return ToolResult<RegistryValueSnapshot>.Fail(message);
            }

            var useDefaultValue = keyAndValuePath.EndsWith('\\');
            var trimmedPath = useDefaultValue ? keyAndValuePath.TrimEnd('\\') : keyAndValuePath;
            var lastSlashIndex = trimmedPath.LastIndexOf('\\');
            var subKeyPath = useDefaultValue ? trimmedPath : lastSlashIndex >= 0 ? trimmedPath[..lastSlashIndex] : trimmedPath;
            var valueName = useDefaultValue ? string.Empty : lastSlashIndex >= 0 ? trimmedPath[(lastSlashIndex + 1)..] : string.Empty;

            if (string.IsNullOrWhiteSpace(subKeyPath))
            {
                const string message = "Registry subkey path cannot be empty.";
                _logger.LogMessagePathKeypath(message, keyPath);
                return ToolResult<RegistryValueSnapshot>.Fail(message);
            }

            using Microsoft.Win32.RegistryKey? subKey = baseKey!.OpenSubKey(subKeyPath);
            if (subKey == null)
            {
                var message = $"Registry key not found: {subKeyPath}";
                _logger.LogInformation(message);
                return ToolResult<RegistryValueSnapshot>.Fail(message);
            }

            var value = subKey.GetValue(valueName);
            if (value == null)
            {
                var effectiveValueName = string.IsNullOrEmpty(valueName) ? "(Default)" : valueName;
                var message = $"Registry value '{effectiveValueName}' not found in key '{subKeyPath}'.";
                _logger.LogInformation(message);
                return ToolResult<RegistryValueSnapshot>.Fail(message);
            }

            return ToolResult<RegistryValueSnapshot>.Ok(new RegistryValueSnapshot
            {
                    Hive = hiveName,
                    KeyPath = subKeyPath,
                    ValueName = string.IsNullOrEmpty(valueName) ? "(Default)" : valueName,
                    ValueKind = subKey.GetValueKind(valueName).ToString(),
                    ValueText = FormatRegistryValue(value)
            });
        }
        catch (System.Security.SecurityException ex)
        {
            _logger.LogSecurityExceptionReadingRegistryKeyKeypath(ex.Message, keyPath);
            return ToolResult<RegistryValueSnapshot>.Fail("Security exception while reading the registry key.");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogUnauthorizedAccessExceptionReadingRegistryKeyKeypath(ex.Message, keyPath);
            return ToolResult<RegistryValueSnapshot>.Fail("Unauthorized access while reading the registry key.");
        }
    }








    private static string Truncate(string value)
    {
        const int maxLength = 512;

        if (value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength] + "...";
    }








    internal static bool TryResolveBaseKey(string hiveName, out Microsoft.Win32.RegistryKey? baseKey)
    {
        baseKey = hiveName.ToUpperInvariant() switch
        {
                "HKEY_CLASSES_ROOT" => Microsoft.Win32.Registry.ClassesRoot,
                "HKEY_CURRENT_USER" => Microsoft.Win32.Registry.CurrentUser,
                "HKEY_LOCAL_MACHINE" => Microsoft.Win32.Registry.LocalMachine,
                "HKEY_USERS" => Microsoft.Win32.Registry.Users,
                "HKEY_CURRENT_CONFIG" => Microsoft.Win32.Registry.CurrentConfig,
                _ => null
        };

        return baseKey != null;
    }
}