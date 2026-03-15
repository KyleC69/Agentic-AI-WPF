// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         DataGridViewModel.cs
// Author: Kyle L. Crowder
// Build Num: 091015



using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DataIngestionLib.DocIngestion;
using DataIngestionLib.RAGModels;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

using RAGDataIngestionWPF.Contracts.ViewModels;




namespace RAGDataIngestionWPF.ViewModels;





public sealed class DataGridViewModel : ObservableObject, INavigationAware
{
    private AsyncRelayCommand _startIngestionCommand;








    public DataGridViewModel()
    {
    }








    public DataGridViewModel(ILogger<DataGridViewModel> logger, LearningHtmlRunner runner)
    {
        _ = logger;
        _ = runner;
    }








    public ObservableCollection<RemoteRag> Source { get; } = [];





    [NotNull]
    public IAsyncRelayCommand StartIngestionCommand
    {
        get { return _startIngestionCommand ??= new AsyncRelayCommand(StartIngestion); }
    }








    public void OnNavigatedTo(object parameter)
    {
        Source.Clear();

    }








    public void OnNavigatedFrom()
    {
    }








    [NotNull]
    private Task StartIngestion()
    {
        return Task.CompletedTask;
    }
}