// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         WebViewViewModel.cs
// Author: Kyle L. Crowder
// Build Num: 194545



using System.Windows;
using System.Windows.Input;

using AgenticAIWPF.Contracts.Services;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;




namespace AgenticAIWPF.ViewModels;





public sealed partial class WebViewViewModel : ObservableObject
{

    private readonly ISystemService _systemService;
    private WebView2 _webView;

    [ObservableProperty] private Visibility failedMesageVisibility = Visibility.Collapsed;

    [ObservableProperty] private Visibility isLoadingVisibility = Visibility.Visible;

    [ObservableProperty] private string source = DefaultUrl;

    // TODO: Set the URI of the page to show by default
    private const string DefaultUrl = "https://docs.microsoft.com/windows/apps/";








    public WebViewViewModel(ISystemService systemService)
    {
        _systemService = systemService;
        Source = DefaultUrl;
    }








    public RelayCommand BrowserBackCommand
    {
        get { return field ??= new RelayCommand(() => _webView?.GoBack(), () => _webView?.CanGoBack ?? false); }
    }

    public RelayCommand BrowserForwardCommand
    {
        get { return field ??= new RelayCommand(() => _webView?.GoForward(), () => _webView?.CanGoForward ?? false); }
    }

    public bool IsLoading
    {
        get;
        set
        {
            _ = this.SetProperty(ref field, value);
            IsLoadingVisibility = value ? Visibility.Visible : Visibility.Collapsed;
        }
    } = true;

    public bool IsShowingFailedMessage
    {
        get;
        set
        {
            _ = this.SetProperty(ref field, value);
            FailedMesageVisibility = value ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public ICommand OpenInBrowserCommand
    {
        get { return field ??= new RelayCommand(OnOpenInBrowser); }
    }

    public ICommand RefreshCommand
    {
        get { return field ??= new RelayCommand(OnRefresh); }
    }








    public void Initialize(WebView2 webView)
    {
        _webView = webView;
    }








    public void OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        IsLoading = false;
        if (e != null && !e.IsSuccess)
        {
            // Use `e.WebErrorStatus` to vary the displayed message based on the error reason
            IsShowingFailedMessage = true;
        }

        BrowserBackCommand.NotifyCanExecuteChanged();
        BrowserForwardCommand.NotifyCanExecuteChanged();
    }








    private void OnOpenInBrowser()
    {
        _systemService.OpenInWebBrowser(Source);
    }








    private void OnRefresh()
    {
        IsShowingFailedMessage = false;
        IsLoading = true;
        _webView?.Reload();
    }
}