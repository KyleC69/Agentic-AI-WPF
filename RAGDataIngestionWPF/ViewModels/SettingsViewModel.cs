// Build Date: 2026/03/12
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         SettingsViewModel.cs
// Author: Kyle L. Crowder
// Build Num: 013439



using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DataIngestionLib.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RAGDataIngestionWPF.Contracts.Services;
using RAGDataIngestionWPF.Contracts.ViewModels;
using RAGDataIngestionWPF.Models;




namespace RAGDataIngestionWPF.ViewModels;





// TODO: Change the URL for your privacy policy in the appsettings.json file, currently set to https://YourPrivacyUrlGoesHere
public class SettingsViewModel(IOptions<AppSettings> appConfig, IThemeSelectorService themeSelectorService, ISystemService systemService, IApplicationInfoService applicationInfoService, IUserDataService userDataService, IApplicationIdService applicationIdService, IChatHistorySettingsService chatHistorySettingsService, ILoggingLevelService loggingLevelService) : ObservableObject, INavigationAware
{
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

    private readonly AppSettings _appConfig = appConfig.Value;
    private readonly IApplicationIdService _applicationIdService = applicationIdService;
    private readonly IApplicationInfoService _applicationInfoService = applicationInfoService;
    private readonly ISystemService _systemService = systemService;
    private readonly IThemeSelectorService _themeSelectorService = themeSelectorService;
    private readonly IUserDataService _userDataService = userDataService;
    private readonly IChatHistorySettingsService _chatHistorySettingsService = chatHistorySettingsService;
    private readonly ILoggingLevelService _loggingLevelService = loggingLevelService;





    public Guid ApplicationId
    {
        get; set => SetProperty(ref field, value);
    }





    public ICommand PrivacyStatementCommand => field ??= new RelayCommand(OnPrivacyStatement);





    public ICommand RenewApplicationIdCommand => field ??= new RelayCommand(OnRenewApplicationId);






    public ICommand SaveChatHistorySettingsCommand => field ??= new RelayCommand(OnSaveChatHistorySettings);





    public ICommand SetThemeCommand => field ??= new RelayCommand<string>(OnSetTheme);





    public AppTheme Theme
    {
        get; set => SetProperty(ref field, value);
    }





    public string ChatModelName
    {
        get; set => SetProperty(ref field, value);
    }





    public string ChatHistoryConnectionString
    {
        get; set => SetProperty(ref field, value);
    }





    public string EmbeddingsModelName
    {
        get; set => SetProperty(ref field, value);
    }





    public int MaxContextMessages
    {
        get; set => SetProperty(ref field, value);
    }





    public int? MaxContextTokens
    {
        get; set => SetProperty(ref field, value);
    }





    public bool RAGKnowledgeEnabled
    {
        get; set => SetProperty(ref field, value);
    }





    public bool ChatHistoryContextEnabled
    {
        get; set => SetProperty(ref field, value);
    }




    /// <summary>
    /// The available <see cref="LogLevel"/> values displayed in the log-level picker.
    /// <see cref="LogLevel.None"/> is excluded because selecting it silences all logging,
    /// which makes runtime diagnostics impossible.
    /// </summary>
    public IReadOnlyList<LogLevel> AvailableLogLevels { get; } =
        Enum.GetValues<LogLevel>().Where(l => l != LogLevel.None).ToList();




    /// <summary>Gets or sets the currently selected minimum log level.</summary>
    public LogLevel MinimumLogLevel
    {
        get; set => SetProperty(ref field, value);
    }




    public ICommand SetLogLevelCommand => field ??= new RelayCommand(OnSetLogLevel);





    public string ChatHistorySettingsStatus
    {
        get; set => SetProperty(ref field, value);
    }





    public string ChatHistoryTitleText => GetResourceString(SettingsPageChatHistoryTitleKey, "Chat History");





    public string ChatModelLabelText => GetResourceString(SettingsPageChatModelLabelKey, "Chat Model");





    public string EmbeddingsModelLabelText => GetResourceString(SettingsPageEmbeddingsModelLabelKey, "Embeddings Model");





    public string ConnectionStringLabelText => GetResourceString(SettingsPageConnectionStringLabelKey, "Connection String");





    public string MaxContextMessagesLabelText => GetResourceString(SettingsPageMaxContextMessagesLabelKey, "Max Context Messages");





    public string MaxContextTokensLabelText => GetResourceString(SettingsPageMaxContextTokensLabelKey, "Max Context Tokens");





    public string RagKnowledgeEnabledLabelText => GetResourceString(SettingsPageRagKnowledgeEnabledLabelKey, "Enable RAG Knowledge Context Injection");





    public string ChatHistoryContextEnabledLabelText => GetResourceString(SettingsPageChatHistoryContextEnabledLabelKey, "Enable Chat History Context Injection");





    public string SaveChatHistoryButtonText => GetResourceString(SettingsPageSaveChatHistoryButtonTextKey, "Save Chat History Settings");





    public string ChatHistorySaveStatusText => GetResourceString(SettingsPageChatHistorySaveStatusKey, "Chat history settings saved.");





    public UserViewModel User
    {
        get; set => SetProperty(ref field, value);
    }





    public string VersionDescription
    {
        get; set => SetProperty(ref field, value);
    }








    public void OnNavigatedTo(object parameter)
    {
        VersionDescription = $"{Properties.Resources.AppDisplayName} - {_applicationInfoService.GetVersion()}";
        ApplicationId = _applicationIdService.GetApplicationId();
        Theme = _themeSelectorService.GetCurrentTheme();
        if (Theme == AppTheme.Default)
        {
            Theme = AppTheme.Dark;
            _themeSelectorService.SetTheme(Theme);
        }

        _userDataService.UserDataUpdated += OnUserDataUpdated;
        User = _userDataService.GetUser();

        ChatHistoryOptions settings = _chatHistorySettingsService.GetCurrentSettings();
        ChatModelName = settings.ChatModelName;
        ChatHistoryConnectionString = settings.ConnectionString;
        EmbeddingsModelName = settings.EmbeddingsModelName;
        MaxContextMessages = settings.MaxContextMessages;
        MaxContextTokens = settings.MaxContextTokens;
        RAGKnowledgeEnabled = settings.RAGKnowledgeEnabled;
        ChatHistoryContextEnabled = settings.ChatHistoryContextEnabled;
        ChatHistorySettingsStatus = string.Empty;

        MinimumLogLevel = _loggingLevelService.GetMinimumLevel();
    }








    public void OnNavigatedFrom()
    {
        UnregisterEvents();
    }








    private void OnPrivacyStatement()
    {
        _systemService.OpenInWebBrowser(_appConfig.PrivacyStatement);
    }








    private void OnRenewApplicationId()
    {
        ApplicationId = _applicationIdService.RenewApplicationId();
    }








    private void OnSetTheme(string themeName)
    {
        AppTheme theme = Enum.Parse<AppTheme>(themeName);
        _themeSelectorService.SetTheme(theme);
    }




    private void OnSetLogLevel()
    {
        _loggingLevelService.SetMinimumLevel(MinimumLogLevel);
    }






    private void OnSaveChatHistorySettings()
    {
        ChatHistoryOptions options = new()
        {
            ChatModelName = ChatModelName,
            ConnectionString = ChatHistoryConnectionString,
            EmbeddingsModelName = EmbeddingsModelName,
            MaxContextMessages = MaxContextMessages,
            MaxContextTokens = MaxContextTokens,
            RAGKnowledgeEnabled = RAGKnowledgeEnabled,
            ChatHistoryContextEnabled = ChatHistoryContextEnabled
        };

        _chatHistorySettingsService.SaveSettings(options);
        ChatHistorySettingsStatus = ChatHistorySaveStatusText;
    }








    private void OnUserDataUpdated(object sender, UserViewModel userData)
    {
        User = userData;
    }








    private void UnregisterEvents()
    {
        _userDataService.UserDataUpdated -= OnUserDataUpdated;
    }








    private static string GetResourceString(string key, string fallback)
    {
        return Properties.Resources.ResourceManager.GetString(key) ?? fallback;
    }
}