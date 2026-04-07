// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         DataGridViewModel.cs
// Author: Kyle L. Crowder
// Build Num: 212939



using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DataIngestionLib.DocIngestion;
using DataIngestionLib.EFModels;

using Microsoft.Extensions.Logging;

using RAGDataIngestionWPF.Contracts.ViewModels;




namespace RAGDataIngestionWPF.ViewModels;





public sealed class DataGridViewModel : ObservableObject, INavigationAware
{
    private readonly DocIngestionPipeline _docIngest;
    private readonly ILogger<DataGridViewModel> _logger;
    private AsyncRelayCommand _startIngestionCommand;

    private CancellationTokenSource cts = new();








    public DataGridViewModel(DocIngestionPipeline docIngestionPipeline)
    {
        _docIngest = docIngestionPipeline;
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


        await _docIngest.DoIngestionAsync(Properties.Settings.Default.LearnBaseUrl, cts.Token);



    }
}