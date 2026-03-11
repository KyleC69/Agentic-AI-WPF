// Build Date: 2026/03/11
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         RegistryReaderTool.cs
// Author: Kyle L. Crowder
// Build Num: 105651



using Microsoft.Extensions.Logging;




namespace DataIngestionLib.ToolFunctions;





//Simple tool to read registry values, will fail gracefully on security errors or if the registry key doesn't exist. 
public class RegistryReaderTool
{
    private readonly ILogger<RegistryReaderTool> _logger;








    public RegistryReaderTool(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<RegistryReaderTool>();
    }








    /// <summary>
    ///     Reads a string value from the Windows Registry.
    /// </summary>
    /// <param name="keyPath">The full path to the registry key (e.g., "HKEY_CURRENT_USER\\Software\\MyApplication\\Setting").</param>
    /// <returns>A <see cref="ToolResult{T}" /> with the value on success, or an error message on failure.</returns>
    public ToolResult<string> ReadStringValue(string keyPath)
    {
        if (string.IsNullOrWhiteSpace(keyPath))
        {
            const string message = "Registry key path cannot be null or empty.";
            _logger.LogError(message);
            return ToolResult<string>.Fail(message);
        }

        if (!OperatingSystem.IsWindows())
        {
            const string message = "Registry access is only supported on Windows.";
            _logger.LogError(message);
            return ToolResult<string>.Fail(message);
        }

        try
        {
            var separatorIndex = keyPath.IndexOf('\\');
            if (separatorIndex <= 0)
            {
                const string message = "Invalid registry key path format.";
                _logger.LogError("{Message} Path: {KeyPath}", message, keyPath);
                return ToolResult<string>.Fail(message);
            }

            var hiveName = keyPath[..separatorIndex];
            var keyAndValuePath = keyPath[(separatorIndex + 1)..];
            if (string.IsNullOrWhiteSpace(keyAndValuePath))
            {
                const string message = "Registry subkey path cannot be empty.";
                _logger.LogError("{Message} Path: {KeyPath}", message, keyPath);
                return ToolResult<string>.Fail(message);
            }

            if (!TryResolveBaseKey(hiveName, out Microsoft.Win32.RegistryKey? baseKey))
            {
                var message = $"Unsupported registry hive: {hiveName}";
                _logger.LogError(message);
                return ToolResult<string>.Fail(message);
            }

            var useDefaultValue = keyAndValuePath.EndsWith("\\", StringComparison.Ordinal);
            var trimmedPath = useDefaultValue ? keyAndValuePath.TrimEnd('\\') : keyAndValuePath;

            var lastSlashIndex = trimmedPath.LastIndexOf('\\');
            var subKeyPath = useDefaultValue
                    ? trimmedPath
                    : lastSlashIndex >= 0
                            ? trimmedPath[..lastSlashIndex]
                            : trimmedPath;
            var valueName = useDefaultValue
                    ? string.Empty
                    : lastSlashIndex >= 0
                            ? trimmedPath[(lastSlashIndex + 1)..]
                            : string.Empty;

            if (string.IsNullOrWhiteSpace(subKeyPath))
            {
                const string message = "Registry subkey path cannot be empty.";
                _logger.LogError("{Message} Path: {KeyPath}", message, keyPath);
                return ToolResult<string>.Fail(message);
            }

            Microsoft.Win32.RegistryKey? subKey = baseKey!.OpenSubKey(subKeyPath);
            using (subKey)
            {
                if (subKey == null)
                {
                    var message = $"Registry key not found: {subKeyPath}";
                    _logger.LogInformation(message);
                    return ToolResult<string>.Fail(message);
                }

                var value = subKey.GetValue(valueName);
                if (value == null)
                {
                    var effectiveValueName = string.IsNullOrEmpty(valueName) ? "(Default)" : valueName;
                    var message = $"Registry value '{effectiveValueName}' not found in key '{subKeyPath}'.";
                    _logger.LogInformation(message);
                    return ToolResult<string>.Fail(message);
                }

                var valueText = value.ToString();
                if (valueText == null)
                {
                    var message = $"Registry value '{valueName}' in key '{subKeyPath}' could not be converted to text.";
                    _logger.LogInformation(message);
                    return ToolResult<string>.Fail(message);
                }

                return ToolResult<string>.Ok(valueText);
            }
        }
        catch (System.Security.SecurityException ex)
        {
            _logger.LogError(ex, "Security exception reading registry key '{KeyPath}'.", keyPath);
            return ToolResult<string>.Fail("Security exception while reading the registry key.");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Unauthorized access exception reading registry key '{KeyPath}'.", keyPath);
            return ToolResult<string>.Fail("Unauthorized access while reading the registry key.");
        }
    }








    private static bool TryResolveBaseKey(string hiveName, out Microsoft.Win32.RegistryKey? baseKey)
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