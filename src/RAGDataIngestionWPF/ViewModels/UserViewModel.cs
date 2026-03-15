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
    [ObservableProperty] public partial string Name { get; set; }





    [ObservableProperty] public partial BitmapImage Photo { get; set; }





    [ObservableProperty] public partial string UserPrincipalName { get; set; }
}