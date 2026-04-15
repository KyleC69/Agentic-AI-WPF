// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         DataGridViewModel.cs
// Author: Kyle L. Crowder
// Build Num: 194538



using System.Collections.ObjectModel;

using AgentAILib.DocIngestion;
using AgentAILib.EFModels;

using AgenticAIWPF.Contracts.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;




namespace AgenticAIWPF.ViewModels;





public sealed class DataGridViewModel : ObservableObject, INavigationAware
{
    private readonly DocIngestionPipeline _docIngest;
    private AsyncRelayCommand _startIngestionCommand;

    private CancellationTokenSource cts = new();








    public DataGridViewModel(DocIngestionPipeline docIngestionPipeline)
    {
        _docIngest = docIngestionPipeline;
    }








    private DataGridViewModel()
    {
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


        await _docIngest.DoIngestionAsync(Properties.Settings.Default.LearnBaseUrl, cts.Token).ConfigureAwait(false);



    }
}