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

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class ViewModelAndConverterTests
{
    [TestMethod]
    public void EnumToBooleanConverterConvertMatchesExpectedEnumValue()
    {
        EnumToBooleanConverter converter = new()
        {
            EnumType = typeof(AppTheme)
        };

        object result = converter.Convert(AppTheme.Dark, typeof(bool), nameof(AppTheme.Dark), CultureInfo.InvariantCulture);

        Assert.AreEqual(true, result);
    }

    [TestMethod]
    public void EnumToBooleanConverterConvertReturnsFalseForMismatchedValue()
    {
        EnumToBooleanConverter converter = new()
        {
            EnumType = typeof(AppTheme)
        };

        object result = converter.Convert(AppTheme.Light, typeof(bool), nameof(AppTheme.Dark), CultureInfo.InvariantCulture);

        Assert.AreEqual(false, result);
    }

    [TestMethod]
    public void EnumToBooleanConverterConvertBackParsesEnum()
    {
        EnumToBooleanConverter converter = new()
        {
            EnumType = typeof(AppTheme)
        };

        object result = converter.ConvertBack(true, typeof(AppTheme), nameof(AppTheme.Default), CultureInfo.InvariantCulture);

        Assert.AreEqual(AppTheme.Default, (AppTheme)result);
    }



    [TestMethod]
    public async Task DataGridViewModelStartIngestionCommandExecutes()
    {
        DataGridViewModel viewModel = new();

        await viewModel.StartIngestionCommand.ExecuteAsync(null);

        Assert.IsNotNull(viewModel.StartIngestionCommand);
    }

    [TestMethod]
    public void ListDetailsViewModelOnNavigatedToLoadsItemsAndSelectsFirst()
    {
        Mock<ISampleDataService> sampleData = new();
        sampleData.Setup(service => service.GetListDetailsDataAsync())
            .ReturnsAsync([
                new SampleOrder { OrderId = 1, Company = "A", Status = "Open", Details = [] },
                new SampleOrder { OrderId = 2, Company = "B", Status = "Closed", Details = [] }
            ]);

        ListDetailsViewModel viewModel = new(sampleData.Object);

        viewModel.OnNavigatedTo(null);

        bool loaded = SpinWait.SpinUntil(() => viewModel.SampleItems.Count == 2, TimeSpan.FromSeconds(2));

        Assert.IsTrue(loaded);
        Assert.AreEqual(2, viewModel.SampleItems.Count);
        Assert.AreEqual(1L, viewModel.Selected.OrderId);
    }

    [TestMethod]
    public void LogInViewModelLoginCommandReflectsBusyState()
    {
        Mock<IIdentityService> identity = new();
        identity.Setup(service => service.LoginAsync()).ReturnsAsync(LoginResultType.Success);
        LogInViewModel viewModel = new(identity.Object)
        {
            IsBusy = true
        };

        Assert.IsFalse(viewModel.LoginCommand.CanExecute(null));
    }

    [TestMethod]
    public void LogInViewModelLoginSetsStatusMessageForUnauthorized()
    {
        Mock<IIdentityService> identity = new();
        identity.Setup(service => service.LoginAsync()).ReturnsAsync(LoginResultType.Unauthorized);
        LogInViewModel viewModel = new(identity.Object);

        viewModel.LoginCommand.Execute(null);

        bool completed = SpinWait.SpinUntil(() => !viewModel.IsBusy, TimeSpan.FromSeconds(2));

        Assert.IsTrue(completed);
        Assert.AreEqual(Resources.StatusUnauthorized, viewModel.StatusMessage);
    }

    [TestMethod]
    public void WebViewViewModelStateAndCommandsWorkWithoutWebView()
    {
        Mock<ISystemService> systemService = new();
        WebViewViewModel viewModel = new(systemService.Object)
        {
            Source = "https://contoso.test"
        };

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
    public void AgentLoggerWhitespaceMessageReturnsFailure()
    {
        AgentLogger logger = new();

        ToolResult<string> result = logger.LogMessage("  ");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("Message cannot be null or whitespace.", result.Error);
    }
}
