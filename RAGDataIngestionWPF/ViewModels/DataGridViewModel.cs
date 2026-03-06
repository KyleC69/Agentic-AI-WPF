// 2026/03/05
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF
//  File:         DataGridViewModel.cs
//   Author: Kyle L. Crowder



using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using RAGDataIngestionWPF.Contracts.ViewModels;
using RAGDataIngestionWPF.Core.Contracts.Services;
using RAGDataIngestionWPF.Core.Models;




namespace RAGDataIngestionWPF.ViewModels;





public class DataGridViewModel : ObservableObject, INavigationAware
{
    private readonly ISampleDataService _sampleDataService;








    public DataGridViewModel(ISampleDataService sampleDataService)
    {
        _sampleDataService = sampleDataService;
    }








    public ObservableCollection<SampleOrder> Source { get; } = [];








    public async void OnNavigatedTo(object parameter)
    {
        Source.Clear();

        // Replace this with your actual data
        IEnumerable<SampleOrder> data = await _sampleDataService.GetGridDataAsync();

        foreach (SampleOrder item in data)
        {
            Source.Add(item);
        }
    }








    public void OnNavigatedFrom()
    {
    }
}