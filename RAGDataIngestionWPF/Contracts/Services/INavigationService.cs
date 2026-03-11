// Build Date: 2026/03/11
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         INavigationService.cs
// Author: Kyle L. Crowder
// Build Num: 105608



using System.Windows.Controls;




namespace RAGDataIngestionWPF.Contracts.Services;





public interface INavigationService
{

    bool CanGoBack { get; }


    void CleanNavigation();


    void GoBack();


    void Initialize(Frame shellFrame);


    event EventHandler<string> Navigated;


    bool NavigateTo(string pageKey, object parameter = null, bool clearNavigation = false);


    void UnsubscribeNavigation();
}