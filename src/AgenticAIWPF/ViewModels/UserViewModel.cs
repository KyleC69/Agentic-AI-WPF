// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         UserViewModel.cs
// Author: Kyle L. Crowder
// Build Num: 194544



using System.Windows.Media.Imaging;

using CommunityToolkit.Mvvm.ComponentModel;




namespace AgenticAIWPF.ViewModels;





public sealed partial class UserViewModel : ObservableObject
{
    [ObservableProperty] private string name = string.Empty;

    [ObservableProperty] private BitmapImage photo;

    [ObservableProperty] private string userPrincipalName = string.Empty;
}