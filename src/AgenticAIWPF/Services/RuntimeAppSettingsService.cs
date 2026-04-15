// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         RuntimeAppSettingsService.cs
// Author: Kyle L. Crowder
// Build Num: 194534



#nullable enable

using AgenticAIWPF.Contracts.Services;

using SystemConfigurationManager = System.Configuration.ConfigurationManager;




namespace AgenticAIWPF.Services;





public sealed class RuntimeAppSettingsService : IRuntimeAppSettingsService
{
    public string GetValue(string key, string fallback)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var value = SystemConfigurationManager.AppSettings[key];
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }








    public void SetValue(string key, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        System.Configuration.Configuration config = SystemConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
        if (config.AppSettings.Settings[key] is null)
        {
            config.AppSettings.Settings.Add(key, value);
        }
        else
        {
            config.AppSettings.Settings[key].Value = value;
        }

        config.Save(System.Configuration.ConfigurationSaveMode.Modified);
        SystemConfigurationManager.RefreshSection("appSettings");
    }
}