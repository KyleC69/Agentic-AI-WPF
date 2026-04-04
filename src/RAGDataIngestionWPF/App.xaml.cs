// Build Date: 2026/04/03
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         App.xaml.cs
// Author: Kyle L. Crowder
// Build Num: 095225



#nullable enable

using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

using DataIngestionLib.Agents;
using DataIngestionLib.Contracts;
using DataIngestionLib.EFModels;
using DataIngestionLib.Providers;
using DataIngestionLib.Services;
using DataIngestionLib.ToolFunctions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Uwp.Notifications;

using RAGDataIngestionWPF.Activation;
using RAGDataIngestionWPF.Contracts.Activation;
using RAGDataIngestionWPF.Contracts.Services;
using RAGDataIngestionWPF.Contracts.Views;
using RAGDataIngestionWPF.Core.Contracts.Services;
using RAGDataIngestionWPF.Core.Services;
using RAGDataIngestionWPF.Properties;
using RAGDataIngestionWPF.Services;
using RAGDataIngestionWPF.ViewModels;
using RAGDataIngestionWPF.Views;




namespace RAGDataIngestionWPF;





public sealed partial class App : Application
{
    private IHost? _host;
    private bool _isHostStarted;

    public static App CurrentApp
    {
        get { return (App)Current; }
    }








    private IHost BuildHost()
    {

        return Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(c =>
                {

                    IConfigurationBuilder unused4 = c.SetBasePath(Environment.CurrentDirectory);
                })
                .ConfigureServices(ConfigureServices)
                .ConfigureLogging(logging =>
                {
                    ILoggingBuilder unused3 = logging.AddDebug();
                    ILoggingBuilder unused2 = logging.AddConsole();
                    // Set the host-level minimum to Trace so every message reaches
                    // the dynamic filter below. The LoggingLevelSwitch controls the
                    // effective minimum at runtime and is user-configurable from the
                    // Settings page.
                    logging.AddJsonConsole(options => { options.JsonWriterOptions = new System.Text.Json.JsonWriterOptions { Indented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }; });
                    ILoggingBuilder unused1 = logging.SetMinimumLevel(LogLevel.Trace);
                    ILoggingBuilder unused = logging.AddFilter((_, level) => level >= LogLevel.Trace);
                })
                .Build();
    }








    private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(services);

        RegisterHostServices(services);
        RegisterAgentServices(services);
        RegisterActivationHandlers(services);
        RegisterCoreServices(services);
        RegisterApplicationServices(services);
        RegisterViewsAndViewModels(services);

    }








    private async Task EnsureHostStartedAsync()
    {
        if (_host is null || _isHostStarted)
        {
            return;
        }


        try
        {
            if (_isHostStarted)
            {
                return;
            }

            await _host.StartAsync();
            _isHostStarted = true;
        }
        catch (Exception ex)
        {
            var logger = _host?.Services.GetService<ILogger<App>>();
            if (logger != null)
            {
                LogUnhandledUiException(logger, ex);
            }
        }
    }








    private static string GetAppLocation()
    {
        var entryAssemblyLocation = Assembly.GetEntryAssembly()?.Location;
        var appLocation = string.IsNullOrWhiteSpace(entryAssemblyLocation) ? AppContext.BaseDirectory : Path.GetDirectoryName(entryAssemblyLocation);

        return string.IsNullOrWhiteSpace(appLocation) ? AppContext.BaseDirectory : appLocation;
    }








    private async Task HandleToastActivationAsync(string toastArgument)
    {
        if (_host is null)
        {
            return;
        }

        IConfiguration configuration = _host.Services.GetRequiredService<IConfiguration>();
        configuration[ToastNotificationActivationHandler.ACTIVATION_ARGUMENTS] = toastArgument;
        await EnsureHostStartedAsync();
    }








    [LoggerMessage(LogLevel.Error, "Unhandled UI exception.")]
    static partial void LogUnhandledUiException(ILogger<App> logger, Exception exception);








    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var logger = _host?.Services.GetService<ILogger<App>>();
        if (logger != null)
        {
            LogUnhandledUiException(logger, e.Exception);
        }

        e.Handled = false;
    }








    private async void OnExit(object sender, ExitEventArgs e)
    {
        if (_host is null)
        {
            return;
        }

        var logger = _host.Services.GetService<ILogger<App>>();

        try
        {
            if (_isHostStarted)
            {
                await _host.StopAsync();
            }
        }
        catch (InvalidOperationException ex)
        {
            if (logger != null)
            {
                LogUnhandledUiException(logger, ex);
            }
        }
        catch (OperationCanceledException ex)
        {
            Debug.Assert(logger != null, nameof(logger) + " != null");
            LogUnhandledUiException(logger, ex);
        }
        finally
        {
            _host.Dispose();
            _host = null;
            _isHostStarted = false;

        }
    }








    private async void OnStartup(object sender, StartupEventArgs e)
    {
        // https://docs.microsoft.com/windows/apps/design/shell/tiles-and-notifications/send-local-toast?tabs=desktop
        ToastNotificationManagerCompat.OnActivated += toastArgs =>
        {
            Task unused = Current.Dispatcher.Invoke(() => _ = HandleToastActivationAsync(toastArgs.Argument));
        };
        // For more information about .NET generic host see  https://docs.microsoft.com/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-3.0

        _host = BuildHost();

        if (ToastNotificationManagerCompat.WasCurrentProcessToastActivated())
        {
            // ToastNotificationActivator code will run after this completes and will show a window if necessary.
            return;
        }

        await EnsureHostStartedAsync();
    }








    private static void RegisterActivationHandlers(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        _ = services.AddSingleton<IActivationHandler, ToastNotificationActivationHandler>();
    }








    private static void RegisterAgentServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.AddDbContextFactory<AIChatHistoryDb>();
        _ = services.AddDbContextFactory<AIRemoteRagContext>();

        _ = services.AddSingleton<HistoryIdentityService>();
        _ = services.AddSingleton<IHistoryIdentityService>(provider => provider.GetRequiredService<HistoryIdentityService>());
        _ = services.AddHttpClient<WebSearchPlugin>();
        _ = services.AddSingleton<SandboxEventLogReader>();
        _ = services.AddSingleton<InstalledUpdatesTool>();
        _ = services.AddSingleton<NetworkConfigurationTool>();
        _ = services.AddSingleton<PerformanceCounterTool>();
        _ = services.AddSingleton<ProcessSnapshotTool>();
        _ = services.AddSingleton<RegistryReaderTool>();
        _ = services.AddSingleton<ReliabilityHistoryTool>();
        _ = services.AddSingleton<SafeCommandRunner>(_ => new SafeCommandRunner(Environment.CurrentDirectory));
        _ = services.AddSingleton<ServiceHealthTool>();
        _ = services.AddSingleton<StartupInventoryTool>();
        _ = services.AddSingleton<StorageHealthTool>();
        _ = services.AddSingleton<WindowsEventChannelReaderTool>();
        _ = services.AddSingleton<WindowsWmiReaderTool>();
        _ = services.AddSingleton<ToolBuilder>();
        _ = services.AddSingleton<IAIToolCatalog>(provider => provider.GetRequiredService<ToolBuilder>());
        _ = services.AddSingleton<IRagDataService, RagDataService>();
        _ = services.AddSingleton<SqlChatHistoryProvider>();
        _ = services.AddSingleton<ISqlChatHistoryProvider>(provider => provider.GetRequiredService<SqlChatHistoryProvider>());
        _ = services.AddSingleton<AIContextRAGInjector>();
        _ = services.AddSingleton<IAgentFactory>(provider =>
        {
            Settings settings = Settings.Default;

            return new AgentFactory(provider.GetRequiredService<ILoggerFactory>(), settings.OllamaHost, settings.OllamaPort, provider.GetRequiredService<SqlChatHistoryProvider>(), provider.GetRequiredService<AIContextRAGInjector>(), provider.GetRequiredService<IAIToolCatalog>());
        });

        _ = services.AddSingleton<ChatHistoryContextInjector>();
    }








    private static void RegisterApplicationServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        _ = services.AddSingleton<IToastNotificationsService, ToastNotificationsService>();
        _ = services.AddSingleton<IApplicationInfoService, ApplicationInfoService>();
        _ = services.AddSingleton<IPersistAndRestoreService, PersistAndRestoreService>();
        _ = services.AddSingleton<ISystemService, SystemService>();
        _ = services.AddSingleton<IChatConversationService, ChatConversationService>();
        _ = services.AddSingleton<IPageService, PageService>();
        _ = services.AddSingleton<INavigationService, NavigationService>();
        _ = services.AddSingleton<IUserDataService, UserDataService>();
    }








    private static void RegisterCoreServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        _ = services.AddSingleton<IIdentityService, IdentityService>();
        _ = services.AddSingleton<IFileService, FileService>();
    }








    private static void RegisterHostServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        _ = services.AddHostedService<ApplicationHostService>();
    }








    private static void RegisterViewsAndViewModels(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        _ = services.AddTransient<IShellWindow, ShellWindow>();
        _ = services.AddTransient<ShellViewModel>();
        _ = services.AddTransient<MainViewModel>();
        _ = services.AddTransient<MainPage>();
        _ = services.AddTransient<BlankViewModel>();
        _ = services.AddTransient<BlankPage>();
        _ = services.AddTransient<ListDetailsViewModel>();
        _ = services.AddTransient<ListDetailsPage>();
        _ = services.AddTransient<DataGridViewModel>();
        _ = services.AddTransient<DataGridPage>();
        _ = services.AddTransient<WebViewViewModel>();
        _ = services.AddTransient<WebViewPage>();
        _ = services.AddTransient<SettingsViewModel>();
        _ = services.AddTransient<SettingsPage>();
        _ = services.AddTransient<ILogInWindow, LogInWindow>();
        _ = services.AddTransient<LogInViewModel>();
    }
}