// 2026/03/10
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF
//  File:         App.xaml.cs
//   Author: Kyle L. Crowder



#nullable enable
// 2026/03/05
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF
//  File:         App.xaml.cs
//   Author: Kyle L. Crowder



using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

using DataIngestionLib.Agents;
using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Options;
using DataIngestionLib.Services;
using DataIngestionLib.Services.ContextInjectors;
using DataIngestionLib.ToolFunctions;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Toolkit.Uwp.Notifications;

using OllamaSharp;

using RAGDataIngestionWPF.Activation;
using RAGDataIngestionWPF.Contracts.Activation;
using RAGDataIngestionWPF.Contracts.Services;
using RAGDataIngestionWPF.Contracts.Views;
using RAGDataIngestionWPF.Core.Contracts.Services;
using RAGDataIngestionWPF.Core.Services;
using RAGDataIngestionWPF.Models;
using RAGDataIngestionWPF.Services;
using RAGDataIngestionWPF.ViewModels;
using RAGDataIngestionWPF.Views;




namespace RAGDataIngestionWPF;





// For more information about application lifecycle events see https://docs.microsoft.com/dotnet/framework/wpf/app-development/application-management-overview





// WPF UI elements use language en-US by default.
// If you need to support other cultures make sure you add converters and review dates and numbers in your UI to ensure everything adapts correctly.
// Tracking issue for improving this is https://github.com/dotnet/wpf/issues/1946
public partial class App : Application
{
    private readonly SemaphoreSlim _hostStartGate = new(1, 1);
    private IHost? _host;
    private bool _isHostStarted;
    private const string OllamaEndpoint = "http://localhost:11434";
    private const string OllamaModel = "gpt-oss:20b-cloud";








    private IHost BuildHost(string[] args, string appLocation, IDictionary<string, string?> activationArgs)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentException.ThrowIfNullOrWhiteSpace(appLocation);
        ArgumentNullException.ThrowIfNull(activationArgs);

        return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(c =>
                {
                    c.SetBasePath(appLocation);
                    c.AddInMemoryCollection(activationArgs);
                })
                .ConfigureServices(ConfigureServices)
                .ConfigureLogging(logging =>
                {
                    logging.AddDebug();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Trace);
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

        // Configuration
        services.Configure<AppSettings>(context.Configuration.GetSection(AppSettings.ConfigurationSectionName));
        services.Configure<ChatHistoryOptions>(context.Configuration.GetSection(ChatHistoryOptions.ConfigurationSectionName));
    }








    private async Task EnsureHostStartedAsync()
    {
        if (_host is null || _isHostStarted)
        {
            return;
        }

        await _hostStartGate.WaitAsync();
        try
        {
            if (_isHostStarted)
            {
                return;
            }

            await _host.StartAsync();
            _isHostStarted = true;
        }
        finally
        {
            _hostStartGate.Release();
        }
    }








    private static string GetAppLocation()
    {
        var entryAssemblyLocation = Assembly.GetEntryAssembly()?.Location;
        var appLocation = string.IsNullOrWhiteSpace(entryAssemblyLocation)
                ? AppContext.BaseDirectory
                : Path.GetDirectoryName(entryAssemblyLocation);

        return string.IsNullOrWhiteSpace(appLocation) ? AppContext.BaseDirectory : appLocation;
    }








    private async Task HandleToastActivationAsync(string toastArgument)
    {
        if (_host is null)
        {
            return;
        }

        IConfiguration configuration = _host.Services.GetRequiredService<IConfiguration>();
        configuration[ToastNotificationActivationHandler.ActivationArguments] = toastArgument;
        await EnsureHostStartedAsync();
    }








    [LoggerMessage(LogLevel.Error, "Unhandled UI exception.")]
    static partial void LogUnhandledUiException(ILogger<App> logger, Exception exception);








    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var logger = _host?.Services.GetService<ILogger<App>>();
        LogUnhandledUiException(logger, e.Exception);
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
            LogUnhandledUiException(logger, ex);
        }
        catch (OperationCanceledException ex)
        {
            LogUnhandledUiException(logger, ex);
        }
        finally
        {
            _host.Dispose();
            _host = null;
            _isHostStarted = false;
            _hostStartGate.Dispose();
        }
    }








    private async void OnStartup(object sender, StartupEventArgs e)
    {
        // https://docs.microsoft.com/windows/apps/design/shell/tiles-and-notifications/send-local-toast?tabs=desktop
        ToastNotificationManagerCompat.OnActivated += toastArgs => { Current.Dispatcher.Invoke(() => _ = HandleToastActivationAsync(toastArgs.Argument)); };
        // For more information about .NET generic host see  https://docs.microsoft.com/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-3.0

        Dictionary<string, string?> activationArgs = new()
        {
                { ToastNotificationActivationHandler.ActivationArguments, string.Empty }
        };
        var appLocation = GetAppLocation();

        _host = BuildHost(e.Args ?? Array.Empty<string>(), appLocation, activationArgs);

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

        services.AddSingleton<IActivationHandler, ToastNotificationActivationHandler>();
    }








    private static void RegisterAgentServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IOllamaApiClient>(_ => new OllamaApiClient(OllamaEndpoint, OllamaModel));
        services.AddSingleton<IChatClient>(sp => (IChatClient)sp.GetRequiredService<IOllamaApiClient>());
        services.AddSingleton(sp => sp.GetRequiredService<IChatClient>().AsAIAgent());

        services.AddSingleton<ISqlChatHistoryConnectionFactory, SqlChatHistoryConnectionFactory>();
        services.AddSingleton<IChatHistoryProvider, SQLChatHistoryProvider>();
        services.AddSingleton<ISQLChatHistoryProvider>(sp => (ISQLChatHistoryProvider)sp.GetRequiredService<IChatHistoryProvider>());
        services.AddSingleton<IRuntimeContextAccessor, RuntimeContextAccessor>();
        services.AddSingleton<IAgentFactory, AgentFactory>();

        services.AddSingleton<IAgentIdentityProvider>(new FixedAgentIdentityProvider("coding-assistant"));
        services.AddSingleton<IAIContextHistoryInjector, AIContextHistoryInjector>();
        services.AddSingleton<IChatHistoryMemoryProvider>(sp => sp.GetRequiredService<IAIContextHistoryInjector>());
        services.AddSingleton<AgentRunMiddleWare>();
    }








    private static void RegisterApplicationServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IToastNotificationsService, ToastNotificationsService>();
        services.AddSingleton<IApplicationInfoService, ApplicationInfoService>();
        services.AddSingleton<IPersistAndRestoreService, PersistAndRestoreService>();
        services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
        services.AddSingleton<ISystemService, SystemService>();
        services.AddSingleton<IApplicationIdService, ApplicationIdService>();
        services.AddSingleton<ISampleDataService, SampleDataService>();
        services.AddSingleton(sp =>
        {
            AppSettings appConfig = sp.GetRequiredService<IOptions<AppSettings>>().Value;
            return new ChatSessionOptions
            {
                    ConfigurationsFolder = appConfig.ConfigurationsFolder,
                    ChatSessionFileName = appConfig.ChatSessionFileName,
                    MaxContextTokens = 120000
            };
        });
        services.AddSingleton<IChatConversationService, ChatConversationService>();
        services.AddSingleton<IPageService, PageService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IUserDataService, UserDataService>();
    }








    private static void RegisterCoreServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IIdentityService, IdentityService>();
        services.AddSingleton<IFileService, FileService>();
    }








    private static void RegisterHostServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHostedService<ApplicationHostService>();
        services.AddHostedService<ChatHistoryInitializationService>();
        services.AddHttpClient("ollama", client => { client.BaseAddress = new Uri(OllamaEndpoint); });
        services.AddHttpClient("langsearch", client => { client.BaseAddress = new Uri("https://api.langsearch.com/"); });
    }








    private static void RegisterViewsAndViewModels(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient<IShellWindow, ShellWindow>();
        services.AddTransient<ShellViewModel>();

        services.AddTransient<MainViewModel>();
        services.AddTransient<MainPage>();

        services.AddTransient<BlankViewModel>();
        services.AddTransient<BlankPage>();

        services.AddTransient<ListDetailsViewModel>();
        services.AddTransient<ListDetailsPage>();

        services.AddTransient<DataGridViewModel>();
        services.AddTransient<DataGridPage>();

        services.AddTransient<WebViewViewModel>();
        services.AddTransient<WebViewPage>();

        services.AddTransient<SettingsViewModel>();
        services.AddTransient<SettingsPage>();

        services.AddTransient<ILogInWindow, LogInWindow>();
        services.AddTransient<LogInViewModel>();
    }
}