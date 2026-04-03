// Build Date: 2026/04/03
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         DataGridViewModel.cs
// Author: Kyle L. Crowder
// Build Num: 095218



using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DataIngestionLib.EFModels;

using Microsoft.Extensions.Logging;

using RAGDataIngestionWPF.Contracts.ViewModels;




namespace RAGDataIngestionWPF.ViewModels;





public sealed class DataGridViewModel : ObservableObject, INavigationAware
{
    private readonly ILogger<DataGridViewModel> _logger;
    private AsyncRelayCommand _startIngestionCommand;








    public DataGridViewModel()
    {
    }








    public DataGridViewModel(ILogger<DataGridViewModel> logger)
    {
        _logger = logger;
    }








    public ObservableCollection<RemoteRag> Source { get; } = [];

    public IAsyncRelayCommand StartIngestionCommand
    {
        get { return _startIngestionCommand ??= new AsyncRelayCommand(StartIngestion); }
    }








    public void OnNavigatedFrom()
    {
    }








    public void OnNavigatedTo(object parameter)
    {
        Source.Clear();

    }








    private async Task StartIngestion()
    {






    }
}