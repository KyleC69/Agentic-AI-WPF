#nullable enable

using DataIngestionLib;
using DataIngestionLib.Agents;
using DataIngestionLib.Boundaries;
using DataIngestionLib.Contracts;
using DataIngestionLib.EFModels;
using DataIngestionLib.Providers;
using DataIngestionLib.Services;
using DataIngestionLib.ToolFunctions;
using DataIngestionLib.ToolFunctions.FileSystemReaders;
using DataIngestionLib.ToolFunctions.FileSystemWriters;
using DataIngestionLib.ToolFunctions.General;
using DataIngestionLib.ToolFunctions.OSTools;

using Microsoft.Extensions.DependencyInjection;

using RAGDataIngestionWPF.Activation;
using RAGDataIngestionWPF.Contracts.Activation;
using RAGDataIngestionWPF.Contracts.Services;
using RAGDataIngestionWPF.Contracts.Views;
using RAGDataIngestionWPF.Core.Contracts.Services;
using RAGDataIngestionWPF.Core.Services;
using RAGDataIngestionWPF.ViewModels;
using RAGDataIngestionWPF.Views;

namespace RAGDataIngestionWPF.Services;

public static class ServiceCollectionRegistrationExtensions
{
    public static IServiceCollection AddActivationHandlersModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.AddSingleton<IActivationHandler, ToastNotificationActivationHandler>();
        return services;
    }

    public static IServiceCollection AddAgentServicesModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.AddSingleton<IAppSettings, AppSettings>();
        _ = services.AddDbContextFactory<AIChatHistoryDb>();
        _ = services.AddDbContextFactory<AIRemoteRagContext>();

        _ = services.AddSingleton<HistoryIdentityService>();
        _ = services.AddSingleton<IHistoryIdentityService>(provider => provider.GetRequiredService<HistoryIdentityService>());
        // Single shared executor instance
        var executor = new CommandExecutor(defaultTimeoutMs: 30_000);

        // Register all tools with DI container
        services.AddSingleton(executor);
        services.AddTransient<PsListTool>();
        services.AddTransient<PsInfoTool>();
        services.AddTransient<HandleTool>();
        services.AddTransient<NetStatTool>();
        services.AddTransient<NsLookupTool>();
        _ = services.AddHttpClient<WebSearchPlugin>();
        _ = services.AddSingleton<WebSearchPlugin>();
        _ = services.AddSingleton<SandboxEventLogReader>();
        _ = services.AddSingleton<FileContentsReadingTool>(_ => new FileContentsReadingTool(HostWhitelist.AllowedRoots));
        _ = services.AddSingleton<FileSystemWriterTool>(_ => new FileSystemWriterTool(HostWhitelist.AllowedRoots));
        _ = services.AddSingleton<InstalledUpdatesTool>();
        _ = services.AddSingleton<ListFolderContentsTool>(_ => new ListFolderContentsTool(HostWhitelist.AllowedRoots));
        _ = services.AddSingleton<LogFileListingTool>(_ => new LogFileListingTool(OSWhitelist.AllowedPaths));
        _ = services.AddSingleton<LogFileReader>(_ => new LogFileReader(OSWhitelist.AllowedPaths));
        _ = services.AddSingleton<NetworkConfigurationTool>();
        _ = services.AddSingleton<PerformanceCounterTool>();
        _ = services.AddSingleton<RegistryReaderTool>();
        _ = services.AddSingleton<ReliabilityHistoryTool>();
        _ = services.AddSingleton<ServiceHealthTool>();
        _ = services.AddSingleton<StartupInventoryTool>();
        _ = services.AddSingleton<StorageHealthTool>();
        _ = services.AddSingleton<WindowsEventChannelReaderTool>();
        _ = services.AddSingleton<WindowsWmiReaderTool>();
        _ = services.AddSingleton<ToolBuilder>();
        _ = services.AddSingleton<IAIToolCatalog>(provider => provider.GetRequiredService<ToolBuilder>());
        _ = services.AddSingleton<IRagDataService, RagDataService>();
        _ = services.AddSingleton<SqlChatHistoryProvider>();
        _ = services.AddSingleton<AIContextRAGInjector>();
        _ = services.AddSingleton<IAgentFactory, AgentFactory>();
        _ = services.AddSingleton<IWorkflowConversationService, WorkflowConversationService>();
        _ = services.AddSingleton<ChatHistoryContextInjector>();

        return services;
    }

    public static IServiceCollection AddApplicationServicesModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.AddSingleton<IRuntimeAppSettingsService, RuntimeAppSettingsService>();
        _ = services.AddSingleton<IToastNotificationsService, ToastNotificationsService>();
        _ = services.AddSingleton<IApplicationInfoService, ApplicationInfoService>();
        _ = services.AddSingleton<IPersistAndRestoreService, PersistAndRestoreService>();
        _ = services.AddSingleton<ISystemService, SystemService>();
        _ = services.AddSingleton<IChatConversationService, ChatConversationService>();
        _ = services.AddSingleton<IPageService, PageService>();
        _ = services.AddSingleton<INavigationService, NavigationService>();
        _ = services.AddSingleton<IUserDataService, UserDataService>();

        return services;
    }

    public static IServiceCollection AddCoreServicesModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.AddSingleton<IIdentityService, IdentityService>();
        _ = services.AddSingleton<IFileService, FileService>();
        return services;
    }

    public static IServiceCollection AddHostServicesModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.AddHostedService<ApplicationHostService>();
        return services;
    }

    public static IServiceCollection AddViewsAndViewModelsModule(this IServiceCollection services)
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

        return services;
    }
}
