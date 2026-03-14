// Build Date: 2026/03/13
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         DataGridViewModel.cs
// Author: Kyle L. Crowder
// Build Num: 175112



using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DataIngestionLib.RAGModels;
using DataIngestionLib.Services;

using Microsoft.Extensions.Logging;

using RAGDataIngestionWPF.Contracts.ViewModels;
using DataIngestionLib.DocIngestion;



namespace RAGDataIngestionWPF.ViewModels;





public class DataGridViewModel : ObservableObject, INavigationAware
{
    private readonly ILogger<DataGridViewModel> _logger;
    private readonly LearningHtmlRunner _runner;

    private AsyncRelayCommand startIngestionCommand;








    public DataGridViewModel(ILogger<DataGridViewModel> logger, LearningHtmlRunner runner)
    {
        _logger = logger;
        _runner = runner;
    }








    public ObservableCollection<RemoteRag> Source { get; } = [];





    public IAsyncRelayCommand StartIngestionCommand
    {
        get { return startIngestionCommand ??= new AsyncRelayCommand(StartIngestion); }
    }








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








    private async Task StartIngestion()
    {

     //   _runner.IngestDocumentAsync("", CancellationToken.None);


    }
}