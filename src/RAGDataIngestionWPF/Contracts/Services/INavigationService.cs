// Build Date: 2026/03/29
// Solution: File
// Project:   RAGDataIngestionWPF
// File:         INavigationService.cs
// Author: Kyle L. Crowder
// Build Num: 051949



using System.Windows.Controls;




namespace RAGDataIngestionWPF.Contracts.Services;





/// <summary>
///     Provides an interface for handling navigation within the application.
/// </summary>
/// <remarks>
///     This service facilitates navigation between different pages, manages navigation history,
///     and provides events for navigation-related actions. It is designed to be used with a
///     <see cref="System.Windows.Controls.Frame" /> as the navigation container.
/// </remarks>
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