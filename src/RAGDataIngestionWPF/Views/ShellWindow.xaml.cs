// Build Date: 2026/04/06
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         ShellWindow.xaml.cs
// Author: Kyle L. Crowder
// Build Num: 212947



using System.Windows.Controls;

using AgenticAIWPF.Contracts.Views;
using AgenticAIWPF.ViewModels;




namespace AgenticAIWPF.Views;





public sealed partial class ShellWindow : IShellWindow
{
    public ShellWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }








    public void CloseWindow()
    {
        this.Close();
    }








    public Frame GetNavigationFrame()
    {
        return ShellFrame;
    }








    public void ShowWindow()
    {
        this.Show();
    }
}