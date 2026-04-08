// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         ViewModelAndConverterTests.cs
// Author: Kyle L. Crowder
// Build Num: 213004



using System.Globalization;

using DataIngestionLib.ToolFunctions;

using Moq;

using RAGDataIngestionWPF.Contracts.Services;
using RAGDataIngestionWPF.Converters;
using RAGDataIngestionWPF.Core.Contracts.Services;
using RAGDataIngestionWPF.Core.Helpers;
using RAGDataIngestionWPF.Core.Models;
using RAGDataIngestionWPF.Models;
using RAGDataIngestionWPF.Properties;
using RAGDataIngestionWPF.ViewModels;

using SystemConfigurationManager = System.Configuration.ConfigurationManager;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class ViewModelAndConverterTests
{

    [TestMethod]
    public void AgentLoggerWhitespaceMessageReturnsFailure()
    {
        AgentLogger logger = new();

        var result = logger.LogMessage("  ");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Message cannot be null or whitespace.", result.Error);
    }








    [TestMethod]
    public void EnumToBooleanConverterConvertBackParsesEnum()
    {
        EnumToBooleanConverter converter = new EnumToBooleanConverter { EnumType = typeof(AppTheme) };

        var result = converter.ConvertBack(true, typeof(AppTheme), nameof(AppTheme.Default), CultureInfo.InvariantCulture);

        Assert.AreEqual(AppTheme.Default, (AppTheme)result);
    }








    [TestMethod]
    public void EnumToBooleanConverterConvertMatchesExpectedEnumValue()
    {
        EnumToBooleanConverter converter = new EnumToBooleanConverter { EnumType = typeof(AppTheme) };

        var result = converter.Convert(AppTheme.Dark, typeof(bool), nameof(AppTheme.Dark), CultureInfo.InvariantCulture);

        Assert.AreEqual(true, result);
    }








    [TestMethod]
    public void EnumToBooleanConverterConvertReturnsFalseForMismatchedValue()
    {
        EnumToBooleanConverter converter = new EnumToBooleanConverter { EnumType = typeof(AppTheme) };

        var result = converter.Convert(AppTheme.Light, typeof(bool), nameof(AppTheme.Dark), CultureInfo.InvariantCulture);

        Assert.AreEqual(false, result);
    }








    [TestMethod]
    public void ListDetailsViewModelOnNavigatedToLoadsItemsAndSelectsFirst()
    {
        Mock<ISampleDataService> sampleData = new Mock<ISampleDataService>();
        sampleData.Setup(service => service.GetListDetailsDataAsync())
                .ReturnsAsync([
                        new SampleOrder { OrderId = 1, Company = "A", Status = "Open", Details = [] },
                        new SampleOrder { OrderId = 2, Company = "B", Status = "Closed", Details = [] }
                ]);

        ListDetailsViewModel viewModel = new ListDetailsViewModel(sampleData.Object);

        viewModel.OnNavigatedTo(null);

        var loaded = SpinWait.SpinUntil(() => viewModel.SampleItems.Count == 2, TimeSpan.FromSeconds(2));

        Assert.IsTrue(loaded);
        Assert.AreEqual(2, viewModel.SampleItems.Count);
        Assert.AreEqual(1L, viewModel.Selected.OrderId);
    }








    [TestMethod]
    public void LogInViewModelLoginCommandReflectsBusyState()
    {
        Mock<IIdentityService> identity = new Mock<IIdentityService>();
        identity.Setup(service => service.LoginAsync()).ReturnsAsync(LoginResultType.Success);
        LogInViewModel viewModel = new LogInViewModel(identity.Object) { IsBusy = true };

        Assert.IsFalse(viewModel.LoginCommand.CanExecute(null));
    }








    [TestMethod]
    public void LogInViewModelLoginSetsStatusMessageForUnauthorized()
    {
        Mock<IIdentityService> identity = new Mock<IIdentityService>();
        identity.Setup(service => service.LoginAsync()).ReturnsAsync(LoginResultType.Unauthorized);
        LogInViewModel viewModel = new LogInViewModel(identity.Object);

        viewModel.LoginCommand.Execute(null);

        var completed = SpinWait.SpinUntil(() => !viewModel.IsBusy, TimeSpan.FromSeconds(2));

        Assert.IsTrue(completed);
        Assert.AreEqual(Resources.StatusUnauthorized, viewModel.StatusMessage);
    }








    [TestMethod]
    public void WebViewViewModelStateAndCommandsWorkWithoutWebView()
    {
        Mock<ISystemService> systemService = new Mock<ISystemService>();
        WebViewViewModel viewModel = new WebViewViewModel(systemService.Object) { Source = "https://contoso.test" };

        Assert.AreEqual("https://contoso.test", viewModel.Source);

        viewModel.IsLoading = false;
        viewModel.IsShowingFailedMessage = true;

        Assert.AreEqual(System.Windows.Visibility.Collapsed, viewModel.IsLoadingVisibility);
        Assert.AreEqual(System.Windows.Visibility.Visible, viewModel.FailedMesageVisibility);

        viewModel.OpenInBrowserCommand.Execute(null);
        systemService.Verify(service => service.OpenInWebBrowser(viewModel.Source), Times.Once);

        viewModel.RefreshCommand.Execute(null);
        Assert.IsTrue(viewModel.IsLoading);
        Assert.IsFalse(viewModel.IsShowingFailedMessage);

        viewModel.OnNavigationCompleted(this, null);
        Assert.IsFalse(viewModel.IsLoading);
    }




    [TestMethod]
    public void SettingsViewModelOnNavigatedToLoadsOrchestrationModeFromConfig()
    {
        SetAppSettingForTest("OrchestrationMode", "RoundRobin");

        Mock<ISystemService> system = new();
        Mock<IApplicationInfoService> appInfo = new();
        Mock<IUserDataService> userData = new();
        appInfo.Setup(service => service.GetVersion()).Returns(new Version(1, 0, 0));
        userData.Setup(service => service.GetUser()).Returns(new UserViewModel());

        SettingsViewModel viewModel = new(system.Object, appInfo.Object, userData.Object);

        viewModel.OnNavigatedTo(null);

        Assert.AreEqual(DataIngestionLib.OrchestrationMode.RoundRobin, viewModel.OrchestrationMode);
    }




    [TestMethod]
    public void SettingsViewModelSaveChatHistorySettingsPersistsOrchestrationMode()
    {
        Mock<ISystemService> system = new();
        Mock<IApplicationInfoService> appInfo = new();
        Mock<IUserDataService> userData = new();
        appInfo.Setup(service => service.GetVersion()).Returns(new Version(1, 0, 0));
        userData.Setup(service => service.GetUser()).Returns(new UserViewModel());

        SettingsViewModel viewModel = new(system.Object, appInfo.Object, userData.Object);
        viewModel.OnNavigatedTo(null);
        viewModel.OrchestrationMode = DataIngestionLib.OrchestrationMode.None;

        viewModel.SaveChatHistorySettingsCommand.Execute(null);

        var savedMode = SystemConfigurationManager.AppSettings["OrchestrationMode"];
        Assert.AreEqual("None", savedMode);
    }




    private static void SetAppSettingForTest(string key, string value)
    {
        System.Configuration.Configuration config = SystemConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
        if (config.AppSettings.Settings[key] is null)
        {
            config.AppSettings.Settings.Add(key, value);
        }
        else
        {
            config.AppSettings.Settings[key].Value = value;
        }

        config.Save(System.Configuration.ConfigurationSaveMode.Modified);
        SystemConfigurationManager.RefreshSection("appSettings");
    }
}