// Build Date: 2026/04/03
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         Settings.cs
// Author: Kyle L. Crowder
// Build Num: 095228



namespace RAGDataIngestionWPF.Properties;





// This class allows you to handle specific events on the settings class:
//  The SettingChanging event is raised before a setting's value is changed.
//  The PropertyChanged event is raised after a setting's value is changed.
//  The SettingsLoaded event is raised after the setting values are loaded.
//  The SettingsSaving event is raised before the setting values are saved.
public sealed partial class Settings
{

    public Settings()
    {
        SettingChanging += SettingChangingEventHandler;
        PropertyChanged += PropertyChangedEventHandler;
        SettingsLoaded += SettingsLoadedEventHandler;
        SettingsSaving += SettingsSavingEventHandler;
    }








    private bool AreCurrentSettingsValid()
    {
        return IsValidSettingValue(nameof(OllamaHost), OllamaHost) && IsValidSettingValue(nameof(OllamaPort), OllamaPort) && IsValidSettingValue(nameof(EmbeddingModel), EmbeddingModel) && IsValidSettingValue(nameof(SessionBudget), SessionBudget) && IsValidSettingValue(nameof(SystemBudget), SystemBudget) && IsValidSettingValue(nameof(RAGBudget), RAGBudget) && IsValidSettingValue(nameof(ToolBudget), ToolBudget) && IsValidSettingValue(nameof(MetaBudget), MetaBudget) && IsValidSettingValue(nameof(LogDirectory), LogDirectory) && IsValidSettingValue(nameof(LogName), LogName) && IsValidSettingValue(nameof(MaximumContext), MaximumContext) && IsValidSettingValue(nameof(AgentId), AgentId) && IsValidSettingValue(nameof(ChatHistoryConnectionString), ChatHistoryConnectionString) && IsValidSettingValue(nameof(RemoteRAGConnectionString), RemoteRAGConnectionString) && IsValidSettingValue(nameof(ChatModel), ChatModel) && IsValidSettingValue(nameof(LearnBaseUrl), LearnBaseUrl) && IsValidSettingValue(nameof(ApplicationId), ApplicationId) && IsValidSettingValue(nameof(UserName), UserName);
    }








    private static bool IsValidSettingValue(string settingName, object value)
    {
        switch (settingName)
        {
            case nameof(OllamaPort):
                return TryGetInt32(value, out var port) && port is >= 1 and <= 65535;
            case nameof(SessionBudget):
            case nameof(SystemBudget):
            case nameof(RAGBudget):
            case nameof(ToolBudget):
            case nameof(MetaBudget):
            case nameof(MaximumContext):
                return TryGetInt32(value, out var budget) && budget > 0;
            case nameof(LearnBaseUrl):
                return value is string learnBaseUrl && Uri.TryCreate(learnBaseUrl, UriKind.Absolute, out Uri uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
            case nameof(ApplicationId):
                return value is string applicationId && Guid.TryParse(applicationId, out _);
            case nameof(OllamaHost):
            case nameof(EmbeddingModel):
            case nameof(LogDirectory):
            case nameof(LogName):
            case nameof(AgentId):
            case nameof(ChatHistoryConnectionString):
            case nameof(RemoteRAGConnectionString):
            case nameof(ChatModel):
            case nameof(UserName):
                return value is string stringValue && !string.IsNullOrWhiteSpace(stringValue);
            default:
                return true;
        }
    }








    private void PropertyChangedEventHandler(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        if (string.IsNullOrWhiteSpace(e.PropertyName))
        {
            return;
        }

        if (!IsValidSettingValue(e.PropertyName, this[e.PropertyName]))
        {
            throw new InvalidOperationException($"The '{e.PropertyName}' setting value is invalid.");
        }
    }








    private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        if (!IsValidSettingValue(e.SettingName, e.NewValue))
        {
            e.Cancel = true;
        }
    }








    private void SettingsLoadedEventHandler(object sender, System.Configuration.SettingsLoadedEventArgs e)
    {
        if (!AreCurrentSettingsValid())
        {
            throw new InvalidOperationException("One or more application settings are invalid.");
        }
    }








    private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        if (!AreCurrentSettingsValid())
        {
            e.Cancel = true;
        }
    }








    private static bool TryGetInt32(object value, out int result)
    {
        switch (value)
        {
            case int intValue:
                result = intValue;
                return true;
            case string stringValue:
                return int.TryParse(stringValue, out result);
            default:
                result = default;
                return false;
        }
    }
}