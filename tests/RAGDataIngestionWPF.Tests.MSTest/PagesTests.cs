// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         PagesTests.cs
// Author: Kyle L. Crowder
// Build Num: 091004



using System.Reflection;

using DataIngestionLib.Contracts.Services;

using JetBrains.Annotations;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
                .ConfigureAppConfiguration(c =>
                {
                    if (appLocation != null)
                    {
                        c.SetBasePath(appLocation);
                    }
                })
                .ConfigureServices(ConfigureServices)
                .Build();
    }








    private void ConfigureServices(HostBuilderContext context, [NotNull] IServiceCollection services)
    {
        // Core Services
        IServiceCollection unused14 = services.AddSingleton<IFileService, FileService>();
        IServiceCollection unused13 = services.AddSingleton<IIdentityService, IdentityService>();

        // Services
        SetAppSetting("Theme", "Dark");
        SetAppSetting("MinimumLogLevel", LogLevel.Trace.ToString());
        SetAppSetting("ApplicationId", Guid.NewGuid().ToString("D"));
        SetAppSetting("ChatModelName", "gpt-oss:20b-cloud");
        SetAppSetting("EmbeddingsModelName", "mxbai-embed-large-v1:latest");
        SetAppSetting("ChatHistoryConnectionString", "Server=.;Database=AIChatHistory;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;");
        SetAppSetting("MaxContextMessages", "40");
        SetAppSetting("MaxContextTokens", "120000");
        SetAppSetting("RagKnowledgeEnabled", bool.TrueString);
        SetAppSetting("ChatHistoryContextEnabled", bool.TrueString);
        SetAppSetting("PrivacyStatement", "https://example.test/privacy");

        IServiceCollection unused12 = services.AddSingleton<ISystemService, SystemService>();
        IServiceCollection unused11 = services.AddSingleton<ISampleDataService, SampleDataService>();
        var chatConversationServiceMock = new Mock<IChatConversationService>();
        chatConversationServiceMock.SetupGet(service => service.ContextTokenCount).Returns(0);
        services.AddSingleton(chatConversationServiceMock.Object);
        IServiceCollection unused10 = services.AddSingleton<IPersistAndRestoreService, PersistAndRestoreService>();
        IServiceCollection unused9 = services.AddSingleton<IUserDataService, UserDataService>();
        IServiceCollection unused8 = services.AddSingleton<IApplicationInfoService, ApplicationInfoService>();
        services.AddSingleton(new LoggingLevelSwitch());
        IServiceCollection unused7 = services.AddSingleton<IPageService, PageService>();
        IServiceCollection unused6 = services.AddSingleton<INavigationService, NavigationService>();

        // ViewModels
        IServiceCollection unused5 = services.AddTransient<WebViewViewModel>();
        IServiceCollection unused4 = services.AddTransient<SettingsViewModel>();
        IServiceCollection unused3 = services.AddTransient<MainViewModel>();
        IServiceCollection unused2 = services.AddTransient<ListDetailsViewModel>();
        IServiceCollection unused1 = services.AddTransient<DataGridViewModel>();
        IServiceCollection unused = services.AddTransient<BlankViewModel>();
    }








    private static void SetAppSetting(string key, string value)
    {
        System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
        if (config.AppSettings.Settings[key] is null)
        {
            config.AppSettings.Settings.Add(key, value);
        }
        else
        {
            config.AppSettings.Settings[key].Value = value;
        }

        config.Save(System.Configuration.ConfigurationSaveMode.Modified);
        System.Configuration.ConfigurationManager.RefreshSection("appSettings");
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