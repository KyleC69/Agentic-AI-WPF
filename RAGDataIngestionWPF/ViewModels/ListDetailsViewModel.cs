// 2026/03/05
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF
//  File:         ListDetailsViewModel.cs
//   Author: Kyle L. Crowder



using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using RAGDataIngestionWPF.Contracts.ViewModels;
using RAGDataIngestionWPF.Core.Contracts.Services;
using RAGDataIngestionWPF.Core.Models;




namespace RAGDataIngestionWPF.ViewModels;





public class ListDetailsViewModel : ObservableObject, INavigationAware
{
    private readonly ISampleDataService _sampleDataService;

    public ListDetailsViewModel(ISampleDataService sampleDataService)
    {
        _sampleDataService = sampleDataService;
    }








    public ObservableCollection<SampleOrder> SampleItems { get; } = [];





    public SampleOrder Selected
    {
        get; set => SetProperty(ref field, value);
    }








    public async void OnNavigatedTo(object parameter)
    {
        SampleItems.Clear();

        IEnumerable<SampleOrder> data = await _sampleDataService.GetListDetailsDataAsync();

        foreach (SampleOrder item in data)
        {
            SampleItems.Add(item);
        }

        Selected = SampleItems.First();
    }








    public void OnNavigatedFrom()
    {
    }
}