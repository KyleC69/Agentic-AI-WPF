// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         ListDetailsPage.xaml.cs
// Author: Kyle L. Crowder
// Build Num: 091023



using RAGDataIngestionWPF.ViewModels;




namespace RAGDataIngestionWPF.Views;





public sealed partial class ListDetailsPage
{
    public ListDetailsPage(ListDetailsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}