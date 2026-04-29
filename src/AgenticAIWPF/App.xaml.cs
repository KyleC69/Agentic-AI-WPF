// Build Date: 2026/04/29
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         App.xaml.cs
// Author: Kyle L. Crowder
// Build Num: 002916



#nullable enable

using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

using AgenticAIWPF.Activation;
using AgenticAIWPF.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Uwp.Notifications;




namespace AgenticAIWPF;





public sealed partial class App : Application
{
    private IHost? _host;
    private bool _isHostStarted;
    private TerminalLogForwarder? _terminalLogForwarder;

    public static App CurrentApp
    {
        get => (App)Current;
    }

    public IServiceProvider Services
    {
        get => _host?.Services ?? throw new InvalidOperationException("The application host is not available.");
    }








    private IHost BuildHost()
    {
        _terminalLogForwarder = new TerminalLogForwarder(GetAppLocation());

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
                    if (_terminalLogForwarder != null)
                    {
                        ILoggingBuilder unused0 = logging.AddProvider(new TerminalLoggerProvider(_terminalLogForwarder));
                    }
                    // Set the host-level minimum to Trace so every message reaches
                    // the dynamic filter below. The LoggingLevelSwitch controls the
                    // effective minimum at runtime and is user-configurable from the
                    // Settings page.
                    logging.AddJsonConsole(options => { options.JsonWriterOptions = new System.Text.Json.JsonWriterOptions { Indented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }; });
                    ILoggingBuilder unused1 = logging.SetMinimumLevel(LogLevel.Trace);
                    ILoggingBuilder unused = logging.AddFilter("Microsoft.Extensions.Http", LogLevel.Information);
                })
                .Build();
    }








    private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(services);

        _ = services.AddHostServicesModule();
        _ = services.AddAgentServicesModule();
        _ = services.AddActivationHandlersModule();
        _ = services.AddCoreServicesModule();
        _ = services.AddApplicationServicesModule();
        _ = services.AddViewsAndViewModelsModule();
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
            _terminalLogForwarder?.Dispose();
            _terminalLogForwarder = null;
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
}