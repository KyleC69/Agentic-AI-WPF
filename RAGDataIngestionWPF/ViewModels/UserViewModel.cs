// Build Date: 2026/03/11
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         UserViewModel.cs
// Author: Kyle L. Crowder
// Build Num: 105621



using System.Windows.Media.Imaging;

using CommunityToolkit.Mvvm.ComponentModel;




namespace RAGDataIngestionWPF.ViewModels;





public class UserViewModel : ObservableObject
{
    public string Name
    {
        get;
        set { this.SetProperty(ref field, value); }
    }





    public BitmapImage Photo
    {
        get;
        set { this.SetProperty(ref field, value); }
    }





    public string UserPrincipalName
    {
        get;
        set { this.SetProperty(ref field, value); }
    }
}