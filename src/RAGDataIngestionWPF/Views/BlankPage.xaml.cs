// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         BlankPage.xaml.cs
// Author: Kyle L. Crowder
// Build Num: 212945



using System.Windows;

using RAGDataIngestionWPF.ViewModels;




namespace RAGDataIngestionWPF.Views;





public sealed partial class BlankPage
{
    public BlankPage(BlankViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        ViewModel = viewModel;
    }



    public BlankViewModel ViewModel { get; set; }











    private void AddAgent_OnClick(object sender, RoutedEventArgs e)
    {
        var win = new AddAgent(ViewModel);
        win.Show();
    }
}