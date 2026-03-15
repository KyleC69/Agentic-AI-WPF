// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         INavigationAware.cs
// Author: Kyle L. Crowder
// Build Num: 091010



namespace RAGDataIngestionWPF.Contracts.ViewModels;





public interface INavigationAware
{

    void OnNavigatedFrom();


    void OnNavigatedTo(object parameter);
}