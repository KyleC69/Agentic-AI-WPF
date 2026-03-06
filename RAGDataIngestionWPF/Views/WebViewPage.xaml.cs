// 2026/03/05
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF
//  File:         WebViewPage.xaml.cs
//   Author: Kyle L. Crowder



using System.Windows.Controls;

using Microsoft.Web.WebView2.Core;

using RAGDataIngestionWPF.ViewModels;




namespace RAGDataIngestionWPF.Views;





public partial class WebViewPage : Page
{
    private readonly WebViewViewModel _viewModel;








    public WebViewPage(WebViewViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _viewModel = viewModel;
        _viewModel.Initialize(webView);
    }








    private void OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        _viewModel.OnNavigationCompleted(sender, e);
    }
}