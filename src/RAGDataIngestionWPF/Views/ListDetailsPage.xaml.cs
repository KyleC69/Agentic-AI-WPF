// Build Date: 2026/03/30
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         ListDetailsPage.xaml.cs
// Author: Kyle L. Crowder
// Build Num: 233210



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