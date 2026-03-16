// Build Date: 2026/03/16
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         ListDetailsPage.xaml.cs
// Author: Kyle L. Crowder
// Build Num: 051909



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