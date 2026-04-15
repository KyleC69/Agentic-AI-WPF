// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         ListDetailsViewModel.cs
// Author: Kyle L. Crowder
// Build Num: 194538



using System.Collections.ObjectModel;

using AgenticAIWPF.Contracts.ViewModels;
using AgenticAIWPF.Core.Contracts.Services;
using AgenticAIWPF.Core.Models;

using CommunityToolkit.Mvvm.ComponentModel;




namespace AgenticAIWPF.ViewModels;





public sealed partial class ListDetailsViewModel : ObservableObject, INavigationAware
{
    private readonly ISampleDataService _sampleDataService;

    [ObservableProperty] private SampleOrder selected;








    public ListDetailsViewModel(ISampleDataService sampleDataService)
    {
        _sampleDataService = sampleDataService;
    }








    public ObservableCollection<SampleOrder> SampleItems { get; } = [];








    public void OnNavigatedFrom()
    {
    }








    public async void OnNavigatedTo(object parameter)
    {
        SampleItems.Clear();

        var data = await _sampleDataService.GetListDetailsDataAsync();

        foreach (SampleOrder item in data) SampleItems.Add(item);

        Selected = SampleItems.First();
    }
}