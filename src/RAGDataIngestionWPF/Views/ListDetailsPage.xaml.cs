// Build Date: 2026/04/06
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         ListDetailsPage.xaml.cs
// Author: Kyle L. Crowder
// Build Num: 212946



using AgenticAIWPF.ViewModels;




namespace AgenticAIWPF.Views;





public sealed partial class ListDetailsPage
{
    public ListDetailsPage(ListDetailsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}