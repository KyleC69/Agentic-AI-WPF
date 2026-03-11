// 2026/03/10
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF
//  File:         SettingsViewModel.cs
//   Author: Kyle L. Crowder



using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Options;

using RAGDataIngestionWPF.Contracts.Services;
using RAGDataIngestionWPF.Contracts.ViewModels;
using RAGDataIngestionWPF.Models;




namespace RAGDataIngestionWPF.ViewModels;





// TODO: Change the URL for your privacy policy in the appsettings.json file, currently set to https://YourPrivacyUrlGoesHere
public class SettingsViewModel(IOptions<AppSettings> appConfig, IThemeSelectorService themeSelectorService, ISystemService systemService, IApplicationInfoService applicationInfoService, IUserDataService userDataService, IApplicationIdService applicationIdService) : ObservableObject, INavigationAware
{
    private readonly AppSettings _appConfig = appConfig.Value;
    private readonly IApplicationIdService _applicationIdService = applicationIdService;
    private readonly IApplicationInfoService _applicationInfoService = applicationInfoService;
    private readonly ISystemService _systemService = systemService;
    private readonly IThemeSelectorService _themeSelectorService = themeSelectorService;
    private readonly IUserDataService _userDataService = userDataService;





    public Guid ApplicationId
    {
        get;
        set { this.SetProperty(ref field, value); }
    }





    public ICommand PrivacyStatementCommand
    {
        get { return field ??= new RelayCommand(OnPrivacyStatement); }
    }





    public ICommand RenewApplicationIdCommand
    {
        get { return field ??= new RelayCommand(OnRenewApplicationId); }
    }





    public ICommand SetThemeCommand
    {
        get { return field ??= new RelayCommand<string>(OnSetTheme); }
    }





    public AppTheme Theme
    {
        get;
        set { this.SetProperty(ref field, value); }
    }





    public UserViewModel User
    {
        get;
        set { this.SetProperty(ref field, value); }
    }





    public string VersionDescription
    {
        get;
        set { this.SetProperty(ref field, value); }
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








    private void OnUserDataUpdated(object sender, UserViewModel userData)
    {
        User = userData;
    }








    private void UnregisterEvents()
    {
        _userDataService.UserDataUpdated -= OnUserDataUpdated;
    }
}