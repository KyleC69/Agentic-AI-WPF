// Build Date: 2026/03/12
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         PagesTests.cs
// Author: Kyle L. Crowder
// Build Num: 013421



using System.Reflection;

using DataIngestionLib.Agents;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using RAGDataIngestionWPF.Contracts.Services;
using RAGDataIngestionWPF.Core.Contracts.Services;
using RAGDataIngestionWPF.Core.Services;
using RAGDataIngestionWPF.Models;
using RAGDataIngestionWPF.Services;
using RAGDataIngestionWPF.ViewModels;
using RAGDataIngestionWPF.Views;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class PagesTests
{
    private readonly IHost _host;








    public PagesTests()
    {
        var appLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
        _host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(c => c.SetBasePath(appLocation))
                .ConfigureServices(ConfigureServices)
                .Build();
    }








    private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // Core Services
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<IIdentityService, IdentityService>();

        // Services
        services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
        services.AddSingleton<ISystemService, SystemService>();
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
        services.AddSingleton<IPersistAndRestoreService, PersistAndRestoreService>();
        services.AddSingleton<IUserDataService, UserDataService>();
        //       services.AddSingleton<IIdentityCacheService, IdentityCacheService>();
        services.AddSingleton<IApplicationInfoService, ApplicationInfoService>();
        services.AddSingleton<IApplicationIdService, ApplicationIdService>();
        Mock<IChatHistorySettingsService> chatHistorySettingsServiceMock = new();
        chatHistorySettingsServiceMock
                .Setup(service => service.GetCurrentSettings())
                .Returns(new ChatHistoryOptions());
        services.AddSingleton(chatHistorySettingsServiceMock.Object);

        Mock<ILoggingLevelService> loggingLevelServiceMock = new();
        loggingLevelServiceMock.Setup(s => s.GetMinimumLevel()).Returns(LogLevel.Trace);
        services.AddSingleton(loggingLevelServiceMock.Object);
        services.AddSingleton<IPageService, PageService>();
        services.AddSingleton<INavigationService, NavigationService>();

        // ViewModels
        services.AddTransient<WebViewViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<ListDetailsViewModel>();
        services.AddTransient<DataGridViewModel>();
        services.AddTransient<BlankViewModel>();

        // Configuration
        services.Configure<AppSettings>(context.Configuration.GetSection(AppSettings.ConfigurationSectionName));
    }








    // TODO: Add tests for functionality you add to BlankViewModel.
    [TestMethod]
    public void TestBlankViewModelCreation()
    {
        var vm = _host.Services.GetService(typeof(BlankViewModel));
        Assert.IsNotNull(vm);
    }








    // TODO: Add tests for functionality you add to DataGridViewModel.
    [TestMethod]
    public void TestDataGridViewModelCreation()
    {
        var vm = _host.Services.GetService(typeof(DataGridViewModel));
        Assert.IsNotNull(vm);
    }








    [TestMethod]
    public void TestGetBlankPageType()
    {
        if (_host.Services.GetService(typeof(IPageService)) is IPageService pageService)
        {
            Type pageType = pageService.GetPageType(typeof(BlankViewModel).FullName);
            Assert.AreEqual(typeof(BlankPage), pageType);
        }
        else
        {
            Assert.Fail($"Can't resolve {nameof(IPageService)}");
        }
    }








    [TestMethod]
    public void TestGetDataGridPageType()
    {
        if (_host.Services.GetService(typeof(IPageService)) is IPageService pageService)
        {
            Type pageType = pageService.GetPageType(typeof(DataGridViewModel).FullName);
            Assert.AreEqual(typeof(DataGridPage), pageType);
        }
        else
        {
            Assert.Fail($"Can't resolve {nameof(IPageService)}");
        }
    }








    [TestMethod]
    public void TestGetListDetailsPageType()
    {
        if (_host.Services.GetService(typeof(IPageService)) is IPageService pageService)
        {
            Type pageType = pageService.GetPageType(typeof(ListDetailsViewModel).FullName);
            Assert.AreEqual(typeof(ListDetailsPage), pageType);
        }
        else
        {
            Assert.Fail($"Can't resolve {nameof(IPageService)}");
        }
    }








    [TestMethod]
    public void TestGetMainPageType()
    {
        if (_host.Services.GetService(typeof(IPageService)) is IPageService pageService)
        {
            Type pageType = pageService.GetPageType(typeof(MainViewModel).FullName);
            Assert.AreEqual(typeof(MainPage), pageType);
        }
        else
        {
            Assert.Fail($"Can't resolve {nameof(IPageService)}");
        }
    }








    [TestMethod]
    public void TestGetSettingsPageType()
    {
        if (_host.Services.GetService(typeof(IPageService)) is IPageService pageService)
        {
            Type pageType = pageService.GetPageType(typeof(SettingsViewModel).FullName);
            Assert.AreEqual(typeof(SettingsPage), pageType);
        }
        else
        {
            Assert.Fail($"Can't resolve {nameof(IPageService)}");
        }
    }








    [TestMethod]
    public void TestGetWebViewPageType()
    {
        if (_host.Services.GetService(typeof(IPageService)) is IPageService pageService)
        {
            Type pageType = pageService.GetPageType(typeof(WebViewViewModel).FullName);
            Assert.AreEqual(typeof(WebViewPage), pageType);
        }
        else
        {
            Assert.Fail($"Can't resolve {nameof(IPageService)}");
        }
    }








    // TODO: Add tests for functionality you add to ListDetailsViewModel.
    [TestMethod]
    public void TestListDetailsViewModelCreation()
    {
        var vm = _host.Services.GetService(typeof(ListDetailsViewModel));
        Assert.IsNotNull(vm);
    }








    // TODO: Add tests for functionality you add to MainViewModel.
    [TestMethod]
    public void TestMainViewModelCreation()
    {
        var vm = _host.Services.GetService(typeof(MainViewModel));
        Assert.IsNotNull(vm);
    }








    // TODO: Add tests for functionality you add to SettingsViewModel.
    [TestMethod]
    public void TestSettingsViewModelCreation()
    {
        var vm = _host.Services.GetService(typeof(SettingsViewModel));
        Assert.IsNotNull(vm);
    }








    // TODO: Add tests for functionality you add to WebViewViewModel.
    [TestMethod]
    public void TestWebViewViewModelCreation()
    {
        var vm = _host.Services.GetService(typeof(WebViewViewModel));
        Assert.IsNotNull(vm);
    }
}