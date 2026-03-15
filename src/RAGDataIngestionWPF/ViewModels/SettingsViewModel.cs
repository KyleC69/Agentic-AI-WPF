// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         SettingsViewModel.cs
// Author: Kyle L. Crowder
// Build Num: 091019



using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using ControlzEx.Theming;

using JetBrains.Annotations;

using MahApps.Metro.Theming;

using Microsoft.Extensions.Logging;

using RAGDataIngestionWPF.Contracts.Services;
using RAGDataIngestionWPF.Contracts.ViewModels;
using RAGDataIngestionWPF.Models;

using SystemConfigurationManager = System.Configuration.ConfigurationManager;




namespace RAGDataIngestionWPF.ViewModels;





// TODO: Change the URL for your privacy policy in the appsettings.json file, currently set to https://YourPrivacyUrlGoesHere
public sealed partial class SettingsViewModel(LoggingLevelSwitch loggingLevelSwitch, ISystemService systemService, IApplicationInfoService applicationInfoService, IUserDataService userDataService) : ObservableObject, INavigationAware
{
    private readonly IApplicationInfoService _applicationInfoService = applicationInfoService;

    private readonly LoggingLevelSwitch _loggingLevelSwitch = loggingLevelSwitch;
    private readonly ISystemService _systemService = systemService;
    private readonly IUserDataService _userDataService = userDataService;
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





    [ObservableProperty] public partial Guid ApplicationId { get; set; }





    /// <summary>
    ///     The available <see cref="LogLevel" /> values displayed in the log-level picker.
    ///     <see cref="LogLevel.None" /> is excluded because selecting it silences all logging,
    ///     which makes runtime diagnostics impossible.
    /// </summary>
    public IReadOnlyList<LogLevel> AvailableLogLevels { get; } =
        Enum.GetValues<LogLevel>().Where(l => l != LogLevel.None).ToList();





    [ObservableProperty] public partial string ChatHistoryConnectionString { get; set; }





    [ObservableProperty] public partial bool ChatHistoryContextEnabled { get; set; }





    public static string ChatHistoryContextEnabledLabelText
    {
        get { return GetResourceString(SettingsPageChatHistoryContextEnabledLabelKey, "Enable Chat History Context Injection"); }
    }





    public static string ChatHistorySaveStatusText
    {
        get { return GetResourceString(SettingsPageChatHistorySaveStatusKey, "Chat history settings saved."); }
    }





    [ObservableProperty] public partial string ChatHistorySettingsStatus { get; set; }





    public static string ChatHistoryTitleText
    {
        get { return GetResourceString(SettingsPageChatHistoryTitleKey, "Chat History"); }
    }





    public static string ChatModelLabelText
    {
        get { return GetResourceString(SettingsPageChatModelLabelKey, "Chat Model"); }
    }





    [ObservableProperty] public partial string ChatModelName { get; set; }





    public static string ConnectionStringLabelText
    {
        get { return GetResourceString(SettingsPageConnectionStringLabelKey, "Connection String"); }
    }





    public static string EmbeddingsModelLabelText
    {
        get { return GetResourceString(SettingsPageEmbeddingsModelLabelKey, "Embeddings Model"); }
    }





    [ObservableProperty] public partial string EmbeddingsModelName { get; set; }





    [ObservableProperty] public partial int MaxContextMessages { get; set; }





    public static string MaxContextMessagesLabelText
    {
        get { return GetResourceString(SettingsPageMaxContextMessagesLabelKey, "Max Context Messages"); }
    }





    [ObservableProperty] public partial int? MaxContextTokens { get; set; }





    public static string MaxContextTokensLabelText
    {
        get { return GetResourceString(SettingsPageMaxContextTokensLabelKey, "Max Context Tokens"); }
    }





    /// <summary>Gets or sets the currently selected minimum log level.</summary>
    [ObservableProperty]
    public partial LogLevel MinimumLogLevel { get; set; }





    [NotNull]
    public ICommand PrivacyStatementCommand
    {
        get { return field ??= new RelayCommand(this.OnPrivacyStatement); }
    }





    [ObservableProperty] public partial bool RAGKnowledgeEnabled { get; set; }





    public static string RagKnowledgeEnabledLabelText
    {
        get { return GetResourceString(SettingsPageRagKnowledgeEnabledLabelKey, "Enable RAG Knowledge Context Injection"); }
    }





    [NotNull]
    public ICommand RenewApplicationIdCommand
    {
        get { return field ??= new RelayCommand(this.OnRenewApplicationId); }
    }





    public static string SaveChatHistoryButtonText
    {
        get { return GetResourceString(SettingsPageSaveChatHistoryButtonTextKey, "Save Chat History Settings"); }
    }





    [NotNull]
    public ICommand SaveChatHistorySettingsCommand
    {
        get { return field ??= new RelayCommand(this.OnSaveChatHistorySettings); }
    }





    [NotNull]
    public ICommand SetLogLevelCommand
    {
        get { return field ??= new RelayCommand(this.OnSetLogLevel); }
    }





    [NotNull]
    public ICommand SetThemeCommand
    {
        get { return field ??= new RelayCommand<string>(this.OnSetTheme); }
    }





    [ObservableProperty] public partial AppTheme Theme { get; set; }





    [ObservableProperty] public partial UserViewModel User { get; set; }





    [ObservableProperty] public partial string VersionDescription { get; set; }








    public void OnNavigatedTo(object parameter)
    {
        VersionDescription = $"{Properties.Resources.AppDisplayName} - {_applicationInfoService.GetVersion()}";
        ApplicationId = GetApplicationId();
        Theme = ParseTheme(GetAppSetting("Theme", "Dark"));
        if (Theme == AppTheme.Default)
        {
            Theme = AppTheme.Dark;
            ApplyTheme(Theme);
            SetAppSetting("Theme", Theme.ToString());
        }

        _userDataService.UserDataUpdated += OnUserDataUpdated;
        User = _userDataService.GetUser();

        ChatModelName = GetAppSetting("ChatModelName", "gpt-oss:20b-cloud");
        ChatHistoryConnectionString = GetAppSetting("ChatHistoryConnectionString", "Server=.;Database=AIChatHistory;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;");
        EmbeddingsModelName = GetAppSetting("EmbeddingsModelName", "mxbai-embed-large-v1:latest");
        MaxContextMessages = ParseInt(GetAppSetting("MaxContextMessages", "40"), 40, 1);
        MaxContextTokens = ParseNullableInt(GetAppSetting("MaxContextTokens", "120000"), 120000);
        RAGKnowledgeEnabled = ParseBool(GetAppSetting("RagKnowledgeEnabled", bool.TrueString), true);
        ChatHistoryContextEnabled = ParseBool(GetAppSetting("ChatHistoryContextEnabled", bool.TrueString), true);
        ChatHistorySettingsStatus = string.Empty;

        MinimumLogLevel = Enum.TryParse(GetAppSetting("MinimumLogLevel", LogLevel.Trace.ToString()), true, out LogLevel level) ? level : LogLevel.Trace;
        _loggingLevelSwitch.MinimumLevel = MinimumLogLevel;
    }








    public void OnNavigatedFrom()
    {
        UnregisterEvents();
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








    private static Guid GetApplicationId()
    {
        var raw = GetAppSetting("ApplicationId", string.Empty);
        if (Guid.TryParse(raw, out Guid applicationId))
        {
            return applicationId;
        }

        Guid created = Guid.NewGuid();
        SetAppSetting("ApplicationId", created.ToString("D"));
        return created;
    }








    private static string GetAppSetting(string key, string fallback)
    {
        var value = SystemConfigurationManager.AppSettings[key];
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }








    private static string GetResourceString([NotNull] string key, string fallback)
    {
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
        SetAppSetting("RagKnowledgeEnabled", RAGKnowledgeEnabled.ToString());
        SetAppSetting("ChatHistoryContextEnabled", ChatHistoryContextEnabled.ToString());
        ChatHistorySettingsStatus = ChatHistorySaveStatusText;
    }








    private void OnSetLogLevel()
    {
        _loggingLevelSwitch.MinimumLevel = MinimumLogLevel;
        SetAppSetting("MinimumLogLevel", MinimumLogLevel.ToString());
    }








    private void OnSetTheme([NotNull] string themeName)
    {
        AppTheme theme = Enum.Parse<AppTheme>(themeName);
        ApplyTheme(theme);
        SetAppSetting("Theme", theme.ToString());
    }








    private void OnUserDataUpdated(object sender, UserViewModel userData)
    {
        User = userData;
    }








    private static bool ParseBool([CanBeNull] string value, bool fallback)
    {
        return bool.TryParse(value, out var parsed) ? parsed : fallback;
    }








    private static int ParseInt([CanBeNull] string value, int fallback, int min)
    {
        return int.TryParse(value, out var parsed) && parsed >= min ? parsed : fallback;
    }








    private static int? ParseNullableInt([CanBeNull] string value, int fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? null : int.TryParse(value, out var parsed) && parsed > 0 ? parsed : fallback;
    }








    private static AppTheme ParseTheme([CanBeNull] string themeName)
    {
        return Enum.TryParse(themeName, out AppTheme theme) ? theme : AppTheme.Default;
    }








    private static Guid RenewApplicationId()
    {
        Guid created = Guid.NewGuid();
        SetAppSetting("ApplicationId", created.ToString("D"));
        return created;
    }








    private static void SetAppSetting(string key, string value)
    {
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








    private void UnregisterEvents()
    {
        _userDataService.UserDataUpdated -= OnUserDataUpdated;
    }
}