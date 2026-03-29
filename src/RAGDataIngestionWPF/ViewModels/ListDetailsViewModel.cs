// Build Date: 2026/03/29
// Solution: File
// Project:   RAGDataIngestionWPF
// File:         ListDetailsViewModel.cs
// Author: Kyle L. Crowder
// Build Num: 052003



using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using RAGDataIngestionWPF.Contracts.ViewModels;
using RAGDataIngestionWPF.Core.Contracts.Services;
using RAGDataIngestionWPF.Core.Models;




namespace RAGDataIngestionWPF.ViewModels;





public sealed partial class ListDetailsViewModel : ObservableObject, INavigationAware
{
    private readonly ISampleDataService _sampleDataService;

    [ObservableProperty] private SampleOrder selected;








    public ListDetailsViewModel(ISampleDataService sampleDataService)
    {
        _sampleDataService = sampleDataService;
    }








    public ObservableCollection<SampleOrder> SampleItems { get; } = [];








    public async void OnNavigatedTo(object parameter)
    {
        SampleItems.Clear();

        var data = await _sampleDataService.GetListDetailsDataAsync();

        foreach (SampleOrder item in data) SampleItems.Add(item);

        Selected = SampleItems.First();
    }








    public void OnNavigatedFrom()
    {
    }
}