// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         INavigationAware.cs
// Author: Kyle L. Crowder
// Build Date: 2026/05/24



namespace AgenticAIWPF.Contracts.ViewModels;





public interface INavigationAware
{

    void OnNavigatedFrom();


    void OnNavigatedTo(object parameter);
}