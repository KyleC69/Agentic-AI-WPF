// Build Date: 2026/03/16
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         WebViewPage.xaml.cs
// Author: Kyle L. Crowder
// Build Num: 051911



using Microsoft.Web.WebView2.Core;

using RAGDataIngestionWPF.ViewModels;




namespace RAGDataIngestionWPF.Views;





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