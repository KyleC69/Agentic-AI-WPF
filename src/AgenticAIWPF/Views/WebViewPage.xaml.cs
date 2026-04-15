// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         WebViewPage.xaml.cs
// Author: Kyle L. Crowder
// Build Num: 194549



using AgenticAIWPF.ViewModels;

using Microsoft.Web.WebView2.Core;




namespace AgenticAIWPF.Views;





public sealed partial class WebViewPage
{
    private readonly WebViewViewModel _viewModel;








    public WebViewPage(WebViewViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _viewModel = viewModel;
        _viewModel.Initialize(WebView);
    }








    private void OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        _viewModel.OnNavigationCompleted(sender, e);
    }
}