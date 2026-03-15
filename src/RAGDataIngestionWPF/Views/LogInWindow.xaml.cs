// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         LogInWindow.xaml.cs
// Author: Kyle L. Crowder
// Build Num: 091023



using RAGDataIngestionWPF.Contracts.Views;
using RAGDataIngestionWPF.ViewModels;




namespace RAGDataIngestionWPF.Views;





public sealed partial class LogInWindow : ILogInWindow
{
    public LogInWindow(LogInViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }








    public void ShowWindow()
    {
        this.Show();
    }








    public void CloseWindow()
    {
        this.Close();
    }
}