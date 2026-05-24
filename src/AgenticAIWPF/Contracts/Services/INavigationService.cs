// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         INavigationService.cs
// Author: Kyle L. Crowder
// Build Date: 2026/05/24



using System.Windows.Controls;




namespace AgenticAIWPF.Contracts.Services;





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


    bool NavigateTo(string pageKey, object parameter = null, bool clearNavigation = false);


    event EventHandler<string> Navigated;


    void UnsubscribeNavigation();
}