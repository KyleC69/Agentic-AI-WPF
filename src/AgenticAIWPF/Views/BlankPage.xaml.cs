// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         BlankPage.xaml.cs
// Author: Kyle L. Crowder
// Build Num: 194546



using System.Windows;

using AgenticAIWPF.ViewModels;




namespace AgenticAIWPF.Views;





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
        AddAgent win = new(ViewModel);
        win.Show();
    }
}