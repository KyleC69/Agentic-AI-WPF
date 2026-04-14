// Build Date: 2026/04/06
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         LogInWindow.xaml.cs
// Author: Kyle L. Crowder
// Build Num: 212946



using AgenticAIWPF.Contracts.Views;
using AgenticAIWPF.ViewModels;




namespace AgenticAIWPF.Views;





public sealed partial class LogInWindow : ILogInWindow
{
    public LogInWindow(LogInViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }








    public void CloseWindow()
    {
        this.Close();
    }








    public void ShowWindow()
    {
        this.Show();
    }
}