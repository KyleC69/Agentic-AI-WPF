// Build Date: 2026/03/11
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         DataGridViewModel.cs
// Author: Kyle L. Crowder
// Build Num: 105617



using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using DataIngestionLib.ExternalKnowledge.RAGModels;
using DataIngestionLib.Services;

using RAGDataIngestionWPF.Contracts.ViewModels;




namespace RAGDataIngestionWPF.ViewModels;





public class DataGridViewModel : ObservableObject, INavigationAware
{





    public ObservableCollection<RemoteRag> Source { get; } = [];








    public void OnNavigatedTo(object parameter)
    {
        Source.Clear();

        ObservableCollection<RemoteRag> entries = RagDataService.GetRagDataEntries();
        foreach (RemoteRag entry in entries)
        {
            Source.Add(entry);
        }

    }








    public void OnNavigatedFrom()
    {
    }
}