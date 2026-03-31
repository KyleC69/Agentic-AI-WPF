// Build Date: 2026/03/30
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         BlankPage.xaml.cs
// Author: Kyle L. Crowder
// Build Num: 233210



using RAGDataIngestionWPF.ViewModels;




namespace RAGDataIngestionWPF.Views;





public sealed partial class BlankPage
{
    public BlankPage(BlankViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}