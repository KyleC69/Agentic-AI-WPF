// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         ShellWindow.xaml.cs
// Author: Kyle L. Crowder
// Build Num: 194548



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