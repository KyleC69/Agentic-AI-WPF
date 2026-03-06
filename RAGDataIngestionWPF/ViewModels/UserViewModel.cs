// 2026/03/05
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF
//  File:         UserViewModel.cs
//   Author: Kyle L. Crowder



using System.Windows.Media.Imaging;

using CommunityToolkit.Mvvm.ComponentModel;




namespace RAGDataIngestionWPF.ViewModels;





public class UserViewModel : ObservableObject
{
    public string Name
    {
        get; set => SetProperty(ref field, value);
    }





    public BitmapImage Photo
    {
        get; set => SetProperty(ref field, value);
    }





    public string UserPrincipalName
    {
        get; set => SetProperty(ref field, value);
    }
}