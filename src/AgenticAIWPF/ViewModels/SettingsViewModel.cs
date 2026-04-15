// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         SettingsViewModel.cs
// Author: Kyle L. Crowder
// Build Num: 194542



using System.Windows;
using System.Windows.Input;

using AgentAILib;

using AgenticAIWPF.Contracts.Services;
using AgenticAIWPF.Contracts.ViewModels;
using AgenticAIWPF.Models;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using ControlzEx.Theming;

using MahApps.Metro.Theming;

using Microsoft.Extensions.Logging;




namespace AgenticAIWPF.ViewModels;





// TODO: Change the URL for your privacy policy in the appsettings.json file, currently set to https://YourPrivacyUrlGoesHere
public sealed partial class SettingsViewModel(ISystemService systemService, IApplicationInfoService applicationInfoService, IUserDataService userDataService, IRuntimeAppSettingsService runtimeSettings) : ObservableObject, INavigationAware
{
    private readonly IApplicationInfoService _applicationInfoService = applicationInfoService;
    private readonly IRuntimeAppSettingsService _runtimeSettings = runtimeSettings;

    private readonly ISystemService _systemService = systemService;
    private readonly IUserDataService _userDataService = userDataService;

    [ObservableProperty] private Guid applicationId;

    [ObservableProperty] private string chatHistoryConnectionString = string.Empty;

    [ObservableProperty] private bool chatHistoryContextEnabled;

    [ObservableProperty] private string chatHistorySettingsStatus = string.Empty;

    [ObservableProperty] private string chatModelName = string.Empty;

    [ObservableProperty] private string embeddingsModelName = string.Empty;

    [ObservableProperty] private int maxContextMessages;

    [ObservableProperty] private int? maxContextTokens;

    /// <summary>Gets or sets the currently selected minimum log level.</summary>
    [ObservableProperty] private LogLevel minimumLogLevel;

    [ObservableProperty] private OrchestrationMode orchestrationMode;

    [ObservableProperty] private bool rAGKnowledgeEnabled;

    [ObservableProperty] private AppTheme theme;

    [ObservableProperty] private UserViewModel user = new();

    [ObservableProperty] private string versionDescription = string.Empty;
    private const string HcDarkTheme = "pack://application:,,,/Styles/Themes/HC.Dark.Blue.xaml";
    private const string HcLightTheme = "pack://application:,,,/Styles/Themes/HC.Light.Blue.xaml";

    private const string SettingsPageChatHistoryContextEnabledLabelKey = "SettingsPageChatHistoryContextEnabledLabel";
    private const string SettingsPageChatHistorySaveStatusKey = "SettingsPageChatHistorySaveStatus";
    private const string SettingsPageChatHistoryTitleKey = "SettingsPageChatHistoryTitle";
    private const string SettingsPageChatModelLabelKey = "SettingsPageChatModelLabel";
    private const string SettingsPageConnectionStringLabelKey = "SettingsPageConnectionStringLabel";
    private const string SettingsPageEmbeddingsModelLabelKey = "SettingsPageEmbeddingsModelLabel";
    private const string SettingsPageMaxContextMessagesLabelKey = "SettingsPageMaxContextMessagesLabel";
    private const string SettingsPageMaxContextTokensLabelKey = "SettingsPageMaxContextTokensLabel";
    private const string SettingsPageRagKnowledgeEnabledLabelKey = "SettingsPageRagKnowledgeEnabledLabel";
    private const string SettingsPageSaveChatHistoryButtonTextKey = "SettingsPageSaveChatHistoryButtonText";

    public IReadOnlyList<OrchestrationMode> AvailableOrchestrationModes
    {
        get { return Enum.GetValues<OrchestrationMode>(); }
    }

    public static string ChatHistoryContextEnabledLabelText
    {
        get { return GetResourceString(SettingsPageChatHistoryContextEnabledLabelKey, "Enable Chat History Context Injection"); }
    }

    public static string ChatHistorySaveStatusText
    {
        get { return GetResourceString(SettingsPageChatHistorySaveStatusKey, "Chat history settings saved."); }
    }

    public static string ChatHistoryTitleText
    {
        get { return GetResourceString(SettingsPageChatHistoryTitleKey, "Chat History"); }
    }

    public static string ChatModelLabelText
    {
        get { return GetResourceString(SettingsPageChatModelLabelKey, "Chat Model"); }
    }

    public static string ConnectionStringLabelText
    {
        get { return GetResourceString(SettingsPageConnectionStringLabelKey, "Connection String"); }
    }

    public static string EmbeddingsModelLabelText
    {
        get { return GetResourceString(SettingsPageEmbeddingsModelLabelKey, "Embeddings Model"); }
    }

    public static string MaxContextMessagesLabelText
    {
        get { return GetResourceString(SettingsPageMaxContextMessagesLabelKey, "Max Context Messages"); }
    }

    public static string MaxContextTokensLabelText
    {
        get { return GetResourceString(SettingsPageMaxContextTokensLabelKey, "Max Context Tokens"); }
    }

    public ICommand PrivacyStatementCommand
    {
        get { return field ??= new RelayCommand(OnPrivacyStatement); }
    }

    public static string RagKnowledgeEnabledLabelText
    {
        get { return GetResourceString(SettingsPageRagKnowledgeEnabledLabelKey, "Enable RAG Knowledge Context Injection"); }
    }

    public ICommand RenewApplicationIdCommand
    {
        get { return field ??= new RelayCommand(OnRenewApplicationId); }
    }

    public static string SaveChatHistoryButtonText
    {
        get { return GetResourceString(SettingsPageSaveChatHistoryButtonTextKey, "Save Chat History Settings"); }
    }

    public ICommand SaveChatHistorySettingsCommand
    {
        get { return field ??= new RelayCommand(OnSaveChatHistorySettings); }
    }

    public ICommand SetLogLevelCommand
    {
        get { return field ??= new RelayCommand(OnSetLogLevel); }
    }

    public ICommand SetThemeCommand
    {
        get { return field ??= new RelayCommand<string>(OnSetTheme); }
    }








    public void OnNavigatedFrom()
    {
        UnregisterEvents();
    }








    public void OnNavigatedTo(object parameter)
    {
        VersionDescription = $"{Properties.Resources.AppDisplayName} - {_applicationInfoService.GetVersion()}";
        ApplicationId = GetApplicationId();
        Theme = ParseTheme(_runtimeSettings.GetValue("Theme", "Dark"));
        if (Theme == AppTheme.Default)
        {
            Theme = AppTheme.Dark;
            ApplyTheme(Theme);
            _runtimeSettings.SetValue("Theme", Theme.ToString());
        }

        _userDataService.UserDataUpdated += OnUserDataUpdated;
        User = _userDataService.GetUser();

        ChatModelName = _runtimeSettings.GetValue("ChatModelName", "gpt-oss:20b-cloud");
        ChatHistoryConnectionString = _runtimeSettings.GetValue("ChatHistoryConnectionString", "Server=.;Database=AIChatHistory;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;");
        EmbeddingsModelName = _runtimeSettings.GetValue("EmbeddingsModelName", "mxbai-embed-large-v1:latest");
        MaxContextMessages = ParseInt(_runtimeSettings.GetValue("MaxContextMessages", "40"), 40, 1);
        MaxContextTokens = ParseNullableInt(_runtimeSettings.GetValue("MaxContextTokens", "120000"), 120000);
        OrchestrationMode = ParseOrchestrationMode(_runtimeSettings.GetValue("OrchestrationMode", OrchestrationMode.None.ToString()));
        RAGKnowledgeEnabled = ParseBool(_runtimeSettings.GetValue("RagKnowledgeEnabled", bool.TrueString), true);
        ChatHistoryContextEnabled = ParseBool(_runtimeSettings.GetValue("ChatHistoryContextEnabled", bool.TrueString), true);
        ChatHistorySettingsStatus = string.Empty;

        MinimumLogLevel = Enum.TryParse(_runtimeSettings.GetValue("MinimumLogLevel", LogLevel.Trace.ToString()), true, out LogLevel level) ? level : LogLevel.Trace;
    }








    private static void ApplyTheme(AppTheme theme)
    {
        _ = ThemeManager.Current.AddLibraryTheme(new LibraryTheme(new Uri(HcDarkTheme), MahAppsLibraryThemeProvider.DefaultInstance));
        _ = ThemeManager.Current.AddLibraryTheme(new LibraryTheme(new Uri(HcLightTheme), MahAppsLibraryThemeProvider.DefaultInstance));
        if (theme == AppTheme.Default)
        {
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncAll;
            ThemeManager.Current.SyncTheme();
            return;
        }

        ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithHighContrast;
        ThemeManager.Current.SyncTheme();
        _ = ThemeManager.Current.ChangeTheme(Application.Current, $"{theme}.Blue", SystemParameters.HighContrast);
    }








    private Guid GetApplicationId()
    {
        var raw = _runtimeSettings.GetValue("ApplicationId", string.Empty);
        if (Guid.TryParse(raw, out Guid applicationId))
        {
            return applicationId;
        }

        Guid created = Guid.NewGuid();
        _runtimeSettings.SetValue("ApplicationId", created.ToString("D"));
        return created;
    }








    private string GetAppSetting(string key, string fallback)
    {
        return _runtimeSettings.GetValue(key, fallback);
    }








    private static string GetResourceString(string key, string fallback)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return Properties.Resources.ResourceManager.GetString(key) ?? fallback;
    }








    private void OnPrivacyStatement()
    {
        _systemService.OpenInWebBrowser(GetAppSetting("PrivacyStatement", "https://YourPrivacyUrlGoesHere"));
    }








    private void OnRenewApplicationId()
    {
        ApplicationId = RenewApplicationId();
    }








    private void OnSaveChatHistorySettings()
    {
        SetAppSetting("ChatModelName", ChatModelName?.Trim() ?? string.Empty);
        SetAppSetting("ChatHistoryConnectionString", ChatHistoryConnectionString?.Trim() ?? string.Empty);
        SetAppSetting("EmbeddingsModelName", EmbeddingsModelName?.Trim() ?? string.Empty);
        SetAppSetting("MaxContextMessages", MaxContextMessages.ToString());
        SetAppSetting("MaxContextTokens", MaxContextTokens?.ToString() ?? string.Empty);
        SetAppSetting("OrchestrationMode", OrchestrationMode.ToString());
        SetAppSetting("RagKnowledgeEnabled", RAGKnowledgeEnabled.ToString());
        SetAppSetting("ChatHistoryContextEnabled", ChatHistoryContextEnabled.ToString());
        ChatHistorySettingsStatus = ChatHistorySaveStatusText;
    }








    private void OnSetLogLevel()
    {
        SetAppSetting("MinimumLogLevel", MinimumLogLevel.ToString());
    }








    private void OnSetTheme(string themeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(themeName);
        AppTheme theme = Enum.Parse<AppTheme>(themeName);
        ApplyTheme(theme);
        SetAppSetting("Theme", theme.ToString());
    }








    private void OnUserDataUpdated(object sender, UserViewModel userData)
    {
        User = userData;
    }








    private static bool ParseBool(string value, bool fallback)
    {
        return bool.TryParse(value, out var parsed) ? parsed : fallback;
    }








    private static int ParseInt(string value, int fallback, int min)
    {
        return int.TryParse(value, out var parsed) && parsed >= min ? parsed : fallback;
    }








    private static int? ParseNullableInt(string value, int fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? null : int.TryParse(value, out var parsed) && parsed > 0 ? parsed : fallback;
    }








    private static OrchestrationMode ParseOrchestrationMode(string value)
    {
        return Enum.TryParse(value, true, out OrchestrationMode parsed) ? parsed : OrchestrationMode.None;
    }








    private static AppTheme ParseTheme(string themeName)
    {
        return Enum.TryParse(themeName, out AppTheme theme) ? theme : AppTheme.Default;
    }








    private Guid RenewApplicationId()
    {
        Guid created = Guid.NewGuid();
        _runtimeSettings.SetValue("ApplicationId", created.ToString("D"));
        return created;
    }








    private void SetAppSetting(string key, string value)
    {
        _runtimeSettings.SetValue(key, value);
    }








    private void UnregisterEvents()
    {
        _userDataService.UserDataUpdated -= OnUserDataUpdated;
    }
}