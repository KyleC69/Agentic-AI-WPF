// Build Date: 2026/03/16
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         INavigationAware.cs
// Author: Kyle L. Crowder
// Build Num: 051857



namespace RAGDataIngestionWPF.Contracts.ViewModels;





public interface INavigationAware
{

    void OnNavigatedFrom();


    void OnNavigatedTo(object parameter);
}