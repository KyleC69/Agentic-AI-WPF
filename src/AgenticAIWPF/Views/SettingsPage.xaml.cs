// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         SettingsPage.xaml.cs
// Author: Kyle L. Crowder
// Build Date: 2026/05/24



using AgenticAIWPF.ViewModels;




namespace AgenticAIWPF.Views;





public sealed partial class SettingsPage
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}