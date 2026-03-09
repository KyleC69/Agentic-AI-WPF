using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.Logging;




namespace DataIngestionLib.ToolFunctions;

//Simple tool to read registry values, will fail gracefully on security errors or if the registry key doesn't exist. 
internal class RegistryReaderTool
{
    private static readonly ILogger<RegistryReaderTool> _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<RegistryReaderTool>();






    /// <summary>
    /// Reads a string value from the Windows Registry.
    /// </summary>
    /// <param name="keyPath">The full path to the registry key (e.g., "HKEY_CURRENT_USER\\Software\\MyApplication\\Setting").</param>
    /// <returns>The string value of the registry entry, or null if the key or value is not found or an error occurs.</returns>
    public static string? ReadStringValue(string keyPath)
    {
        if (string.IsNullOrWhiteSpace(keyPath))
        {
            _logger.LogError("Registry key path cannot be null or empty.");
            return null;
        }

        try
        {
            // Split the key path into the registry hive and the rest of the path
            int separatorIndex = keyPath.IndexOf('\\');
            if (separatorIndex <= 0)
            {
                _logger.LogError($"Invalid registry key path format: {keyPath}");
                return null;
            }

            string hiveName = keyPath.Substring(0, separatorIndex);
            string subKeyPath = keyPath.Substring(separatorIndex + 1);

            Microsoft.Win32.RegistryKey? baseKey = null;
            switch (hiveName.ToUpperInvariant())
            {
                case "HKEY_CLASSES_ROOT":
                    baseKey = Microsoft.Win32.Registry.ClassesRoot;
                    break;
                case "HKEY_CURRENT_USER":
                    baseKey = Microsoft.Win32.Registry.CurrentUser;
                    break;
                case "HKEY_LOCAL_MACHINE":
                    baseKey = Microsoft.Win32.Registry.LocalMachine;
                    break;
                case "HKEY_USERS":
                    baseKey = Microsoft.Win32.Registry.Users;
                    break;
                case "HKEY_CURRENT_CONFIG":
                    baseKey = Microsoft.Win32.Registry.CurrentConfig;
                    break;
                default:
                    _logger.LogError($"Unsupported registry hive: {hiveName}");
                    return null;
            }

            using (baseKey)
            {
                Microsoft.Win32.RegistryKey? subKey = baseKey.OpenSubKey(subKeyPath);
                using (subKey)
                {
                    if (subKey == null)
                    {
                        _logger.LogInformation($"Registry key not found: {keyPath}");
                        return null;
                    }

                    // The last part of the keyPath is the value name
                    string valueName = "";
                    int lastSlashIndex = subKeyPath.LastIndexOf('\\');
                    if (lastSlashIndex != -1)
                    {
                        valueName = subKeyPath.Substring(lastSlashIndex + 1);
                        // If the subKeyPath itself ends with a slash, it might imply a default value,
                        // but for string values, we usually expect a specific name.
                        // Here, we assume the value name is the last part.
                        // If the key path points to the key itself, not a value, "Default" is often implied.
                    }
                    else
                    {
                        valueName = "Default"; // Default value if no name is specified in the path after the hive
                    }

                    // If the keyPath ends in a backslash, it implies the default value
                    if (keyPath.EndsWith("\\"))
                    {
                        valueName = "Default";
                    }
                    else
                    {
                        // Extract the value name from the original keyPath
                        valueName = keyPath.Substring(keyPath.LastIndexOf('\\') + 1);
                        if (string.IsNullOrEmpty(valueName)) // Handle cases like HKEY_CURRENT_USER\Software\
                        {
                            valueName = "Default";
                        }
                    }

                    object? value = subKey.GetValue(valueName);

                    if (value == null)
                    {
                        _logger.LogInformation($"Registry value '{valueName}' not found in key '{keyPath}'.");
                        return null;
                    }

                    return value.ToString();
                }
            }
        }
        catch (System.Security.SecurityException ex)
        {
            _logger.LogError($"Security exception reading registry key '{keyPath}': {ex.Message}");
            return null;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError($"Unauthorized access exception reading registry key '{keyPath}': {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"An unexpected error occurred reading registry key '{keyPath}': {ex.Message}");
            return null;
        }
    }
}
