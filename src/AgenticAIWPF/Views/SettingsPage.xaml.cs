// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         SettingsPage.xaml.cs
// Author: Kyle L. Crowder
// Build Num: 194548



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