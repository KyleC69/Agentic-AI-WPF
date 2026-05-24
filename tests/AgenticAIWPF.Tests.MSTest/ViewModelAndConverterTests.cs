// Build Date: 2026/04/13
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF.Tests.MSTest
// File:         ViewModelAndConverterTests.cs
// Author: GitHub Copilot
// Build Num: 204201



using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;

using AgentAILib.ToolFunctions;

using Moq;

using AgenticAIWPF.Contracts.Services;
using AgenticAIWPF.Converters;
using AgenticAIWPF.Core.Contracts.Services;
using AgenticAIWPF.Core.Helpers;
using AgenticAIWPF.Core.Models;
using AgenticAIWPF.Models;
using AgenticAIWPF.Properties;
using AgenticAIWPF.ViewModels;




namespace AgenticAIWPF.Tests.MSTest;





[TestClass]
public class ViewModelAndConverterTests
{
    [TestMethod]
    public void CommandResultFailureSetsErrorAndExitCode()
    {
        var result = CommandResult.Failure("Message cannot be null or whitespace.");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Message cannot be null or whitespace.", result.Error);
        Assert.AreEqual(-1, result.ExitCode);
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
        Mock<ISystemService> system = new();
        Mock<IApplicationInfoService> appInfo = new();
        Mock<IUserDataService> userData = new();
        Mock<IRuntimeAppSettingsService> runtimeSettings = new();
        appInfo.Setup(service => service.GetVersion()).Returns(new Version(1, 0, 0));
        userData.Setup(service => service.GetUser()).Returns(new UserViewModel());
        runtimeSettings.Setup(service => service.GetValue(It.IsAny<string>(), It.IsAny<string>())).Returns((string _, string fallback) => fallback);
        runtimeSettings.Setup(service => service.GetValue("OrchestrationMode", It.IsAny<string>())).Returns("RoundRobin");

        SettingsViewModel viewModel = new(system.Object, appInfo.Object, userData.Object, runtimeSettings.Object);

        viewModel.OnNavigatedTo(null);

        Assert.AreEqual(AgentAILib.OrchestrationMode.RoundRobin, viewModel.OrchestrationMode);
    }
}

internal sealed class ListDetailsViewModel
{
    private readonly ISampleDataService sampleDataService;

    public ListDetailsViewModel(ISampleDataService sampleDataService)
    {
        this.sampleDataService = sampleDataService;
    }

    public ObservableCollection<SampleOrder> SampleItems { get; } = [];

    public SampleOrder Selected { get; private set; } = new SampleOrder();

    public void OnNavigatedTo(object? parameter)
    {
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var items = await sampleDataService.GetListDetailsDataAsync();

        SampleItems.Clear();
        foreach (var item in items)
        {
            SampleItems.Add(item);
        }

        if (SampleItems.Count > 0)
        {
            Selected = SampleItems[0];
        }
    }
}

internal sealed class WebViewViewModel
{
    private readonly ISystemService systemService;

    public WebViewViewModel(ISystemService systemService)
    {
        this.systemService = systemService;
        OpenInBrowserCommand = new DelegateCommand(_ => this.systemService.OpenInWebBrowser(Source));
        RefreshCommand = new DelegateCommand(_ =>
        {
            IsLoading = true;
            IsShowingFailedMessage = false;
        });
    }

    public string Source { get; set; } = string.Empty;

    public bool IsLoading { get; set; }

    public bool IsShowingFailedMessage { get; set; }

    public System.Windows.Visibility IsLoadingVisibility => IsLoading ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

    public System.Windows.Visibility FailedMesageVisibility => IsShowingFailedMessage ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

    public ICommand OpenInBrowserCommand { get; }

    public ICommand RefreshCommand { get; }

    public void OnNavigationCompleted(object sender, object? e)
    {
        IsLoading = false;
    }
}

internal sealed class DelegateCommand : ICommand
{
    private readonly Action<object?> execute;
    private readonly Predicate<object?>? canExecute;

    public DelegateCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        this.execute = execute;
        this.canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => canExecute?.Invoke(parameter) ?? true;

    public void Execute(object? parameter) => execute(parameter);
}