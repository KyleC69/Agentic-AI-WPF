// 2026/03/05
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF
//  File:         ApplicationHostService.cs
//   Author: Kyle L. Crowder



using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using RAGDataIngestionWPF.Contracts.Activation;
using RAGDataIngestionWPF.Contracts.Services;
using RAGDataIngestionWPF.Contracts.Views;
using RAGDataIngestionWPF.Models;
using RAGDataIngestionWPF.ViewModels;




namespace RAGDataIngestionWPF.Services;





public class ApplicationHostService : IHostedService
{

    private readonly IEnumerable<IActivationHandler> _activationHandlers;
    private readonly AppConfig _appConfig;
    private readonly INavigationService _navigationService;
    private readonly IPersistAndRestoreService _persistAndRestoreService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly IToastNotificationsService _toastNotificationsService;
    private readonly IUserDataService _userDataService;
    private bool _isInitialized;
    private IShellWindow _shellWindow;








    public ApplicationHostService(IServiceProvider serviceProvider, IEnumerable<IActivationHandler> activationHandlers, INavigationService navigationService, IThemeSelectorService themeSelectorService, IPersistAndRestoreService persistAndRestoreService, IToastNotificationsService toastNotificationsService, IUserDataService userDataService, IOptions<AppConfig> config)
    {
        _serviceProvider = serviceProvider;
        _activationHandlers = activationHandlers;
        _navigationService = navigationService;
        _themeSelectorService = themeSelectorService;
        _persistAndRestoreService = persistAndRestoreService;
        _toastNotificationsService = toastNotificationsService;
        _userDataService = userDataService;
        _appConfig = config.Value;
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
        _persistAndRestoreService.PersistData();
        await Task.CompletedTask;
    }








    private async Task HandleActivationAsync()
    {
        IActivationHandler activationHandler = _activationHandlers.FirstOrDefault(h => h.CanHandle());

        if (activationHandler != null)
        {
            await activationHandler.HandleAsync();
        }

        await Task.CompletedTask;

        if (App.Current.Windows.OfType<IShellWindow>().Count() == 0)
        {
            // Default activation that navigates to the apps default page
            _shellWindow = _serviceProvider.GetService(typeof(IShellWindow)) as IShellWindow;
            _navigationService.Initialize(_shellWindow.GetNavigationFrame());
            _shellWindow.ShowWindow();
            _navigationService.NavigateTo(typeof(MainViewModel).FullName);
            await Task.CompletedTask;
        }
    }








    private async Task InitializeAsync()
    {
        if (!_isInitialized)
        {
            _persistAndRestoreService.RestoreData();
            _themeSelectorService.InitializeTheme();
            _userDataService.Initialize();
            await Task.CompletedTask;
        }
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