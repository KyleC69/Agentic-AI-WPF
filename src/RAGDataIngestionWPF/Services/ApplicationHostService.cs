// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         ApplicationHostService.cs
// Author: Kyle L. Crowder
// Build Num: 212934



using System.Windows;

using ControlzEx.Theming;

using MahApps.Metro.Theming;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using RAGDataIngestionWPF.Contracts.Activation;
using RAGDataIngestionWPF.Contracts.Services;
using RAGDataIngestionWPF.Contracts.Views;
using RAGDataIngestionWPF.Models;
using RAGDataIngestionWPF.ViewModels;




namespace RAGDataIngestionWPF.Services;





public sealed class ApplicationHostService : IHostedService
{

    private readonly IEnumerable<IActivationHandler> _activationHandlers;
    private bool _isInitialized;
    private readonly INavigationService _navigationService;
    private readonly IServiceProvider _serviceProvider;
    private IShellWindow _shellWindow;
    private readonly IRuntimeAppSettingsService _runtimeSettings;
    private readonly IToastNotificationsService _toastNotificationsService;
    private readonly IUserDataService _userDataService;
    private const string HcDarkTheme = "pack://application:,,,/Styles/Themes/HC.Dark.Blue.xaml";
    private const string HcLightTheme = "pack://application:,,,/Styles/Themes/HC.Light.Blue.xaml";








    public ApplicationHostService(IServiceProvider serviceProvider, IEnumerable<IActivationHandler> activationHandlers, INavigationService navigationService, IToastNotificationsService toastNotificationsService, IUserDataService userDataService, IRuntimeAppSettingsService runtimeSettings)
    {
        _serviceProvider = serviceProvider;
        _activationHandlers = activationHandlers;
        _navigationService = navigationService;
        _toastNotificationsService = toastNotificationsService;
        _userDataService = userDataService;
        _runtimeSettings = runtimeSettings;
    }








    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await InitializeAsync();

        await HandleActivationAsync();

        // Tasks after activation
        await StartupAsync();
        _isInitialized = true;
    }








    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
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








    private async Task HandleActivationAsync()
    {
        IActivationHandler activationHandler = _activationHandlers.FirstOrDefault(h => h.CanHandle());

        if (activationHandler != null)
        {
            await activationHandler.HandleAsync();
        }

        await Task.CompletedTask;

        if (!Application.Current.Windows.OfType<IShellWindow>().Any())
        {
            // Default activation that navigates to the apps default page
            _shellWindow = _serviceProvider.GetRequiredService<IShellWindow>();
            _navigationService.Initialize(_shellWindow.GetNavigationFrame());
            _shellWindow.ShowWindow();
            var unused = _navigationService.NavigateTo(typeof(MainViewModel).FullName);
            await Task.CompletedTask;
        }
    }








    private async Task InitializeAsync()
    {
        if (!_isInitialized)
        {
            ApplyTheme(ParseTheme(_runtimeSettings.GetValue("Theme", "Dark")));
            _userDataService.Initialize();
            await Task.CompletedTask;
        }
    }








    private static AppTheme ParseTheme(string themeName)
    {
        return Enum.TryParse(themeName, out AppTheme theme) ? theme : AppTheme.Dark;
    }








    private async Task StartupAsync()
    {
        if (!_isInitialized)
        {
            _toastNotificationsService.ShowToastNotificationSample();
            await Task.CompletedTask;
        }
    }
}