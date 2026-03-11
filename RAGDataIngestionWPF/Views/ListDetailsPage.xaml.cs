// 2026/03/10
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF
//  File:         ListDetailsPage.xaml.cs
//   Author: Kyle L. Crowder



using System.Windows.Controls;

using RAGDataIngestionWPF.ViewModels;




namespace RAGDataIngestionWPF.Views;





public partial class ListDetailsPage : Page
{
    public ListDetailsPage(ListDetailsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}