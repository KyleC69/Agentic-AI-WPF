// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         UserViewModel.cs
// Author: Kyle L. Crowder
// Build Num: 091022



using System.Windows.Media.Imaging;

using CommunityToolkit.Mvvm.ComponentModel;




namespace RAGDataIngestionWPF.ViewModels;





public sealed partial class UserViewModel : ObservableObject
{
    [ObservableProperty] private string name = string.Empty;





    [ObservableProperty] private BitmapImage photo;





    [ObservableProperty] private string userPrincipalName = string.Empty;
}