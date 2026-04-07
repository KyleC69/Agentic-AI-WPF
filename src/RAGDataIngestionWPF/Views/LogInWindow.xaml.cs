// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         LogInWindow.xaml.cs
// Author: Kyle L. Crowder
// Build Num: 212946



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








    public void CloseWindow()
    {
        this.Close();
    }








    public void ShowWindow()
    {
        this.Show();
    }
}