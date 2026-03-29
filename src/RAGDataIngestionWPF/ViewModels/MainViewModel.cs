// Build Date: 2026/03/29
// Solution: File
// Project:   RAGDataIngestionWPF
// File:         MainViewModel.cs
// Author: Kyle L. Crowder
// Build Num: 051959



using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DataIngestionLib.Models;
using DataIngestionLib.Contracts.Services;

using Microsoft.Extensions.AI;

using RAGDataIngestionWPF.Contracts.ViewModels;
using RAGDataIngestionWPF.Models;




namespace RAGDataIngestionWPF.ViewModels;





public sealed partial class MainViewModel : ObservableObject, IDisposable, INavigationAware
{
    private readonly IChatConversationService _chatConversationService;
    private bool _historyLoaded;
    private CancellationTokenSource _responseCancellationTokenSource;

    //Running Token counts for different categories
    [ObservableProperty] private int ragTokenCount;
    [ObservableProperty] private int sessionTokenCount;
    [ObservableProperty] private int systemTokenCount;
    [ObservableProperty] private int toolTokenCount;
    [ObservableProperty] private int totalTokenCount;
    [ObservableProperty] private int inputTokenCount;
    [ObservableProperty] private int outputTokenCount;
    [ObservableProperty] private int cachedInputTokenCount;
    [ObservableProperty] private int reasoningTokenCount;








    public MainViewModel(IChatConversationService chatConversationService)
    {
        ArgumentNullException.ThrowIfNull(chatConversationService);

        _chatConversationService = chatConversationService;
        Messages = new ObservableCollection<ChatMessageDisplayItem>();

        SendMessageCommand = new AsyncRelayCommand(SendMessageAsync, CanSendMessage);
        CancelMessageCommand = new RelayCommand(CancelMessage, CanCancelMessage);

        _chatConversationService.BusyStateChanged += OnBusyStateChange;
        _chatConversationService.TokenUsageUpdated += OnTokenUsageUpdated;



    }




















    public IRelayCommand CancelMessageCommand { get; }



    /// <summary>
    /// Gets or sets a value indicating whether the application is currently in a busy state.
    /// </summary>
    /// <remarks>
    /// This property is used to manage the application's busy state, which affects the availability of commands
    /// such as <see cref="SendMessageCommand"/> and <see cref="CancelMessageCommand"/>. It also triggers the 
    /// <see cref="OnBusyStateChange"/> method to handle state changes.
    /// </remarks>
    public bool IsBusy
    {
        get;
        set
        {
            if (this.SetProperty(ref field, value))
            {
                this.OnPropertyChanged(nameof(CanSendMessage));
                this.OnPropertyChanged(nameof(CanCancelMessage));
                OnBusyStateChange(this, value);
            }
        }
    }

    public string MessageInput
    {
        get;
        set
        {
            if (this.SetProperty(ref field, value))
            {
                SendMessageCommand.NotifyCanExecuteChanged();
            }
        }
    } = string.Empty;

    public ObservableCollection<ChatMessageDisplayItem> Messages { get; }

    public IAsyncRelayCommand SendMessageCommand { get; }









    public void Dispose()
    {
        _responseCancellationTokenSource?.Dispose();
        _chatConversationService.BusyStateChanged -= OnBusyStateChange;
        _chatConversationService.TokenUsageUpdated -= OnTokenUsageUpdated;

    }








    /// <summary>
    /// Called when the view model is navigated away from.
    /// </summary>
    /// <remarks>
    /// This method is part of the <see cref="INavigationAware"/> interface and is invoked
    /// to handle any cleanup or state-saving logic when the associated view is no longer active.
    /// </remarks>
    public void OnNavigatedFrom()
    {
    }








    /// <summary>
    /// Called when the view model is navigated to.
    /// </summary>
    /// <param name="parameter">
    /// An optional parameter passed during navigation. This can be used to provide
    /// context or data required by the view model.
    /// </param>
    /// <remarks>
    /// This method is part of the <see cref="INavigationAware"/> interface and is invoked
    /// to handle initialization logic, such as loading data or setting up state, when the
    /// associated view becomes active.
    /// </remarks>
    public async void OnNavigatedTo(object parameter)
    {
        if (_historyLoaded)
        {
            return;
        }

        _historyLoaded = true;
        try
        {
            var historyMessages = await _chatConversationService.LoadConversationHistoryAsync().ConfigureAwait(true);

            Messages.Clear();
            foreach (ChatMessage historyMessage in historyMessages) Messages.Add(CreateUiMessage(historyMessage));

            RefreshTokenCountsFromService();
        }
        catch
        {
            Messages.Clear();
        }
    }








    private bool CanCancelMessage()
    {
        return IsBusy;
    }








    private void CancelMessage()
    {
        _responseCancellationTokenSource?.Cancel();
    }








    private bool CanSendMessage()
    {
        return !IsBusy && !string.IsNullOrWhiteSpace(MessageInput);
    }








    private static ChatMessageDisplayItem CreateUiMessage(ChatMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        return ChatMessageDisplayItem.Create(message.Role, message.Text);
    }








    private void OnBusyStateChange(object sender, bool e)
    {
        IsBusy = e;
    }








    private async Task SendMessageAsync()
    {
        //TODO: Need to link to lifecycle of view model and application lifetime.
        _responseCancellationTokenSource = new CancellationTokenSource();


        var content = MessageInput.Trim();
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        //Add Users message to UI collection
        Messages.Add(ChatMessageDisplayItem.Create(ChatRole.User, content));

        //Clear UI input
        MessageInput = string.Empty;



        try
        {
            ChatMessage assistantMessage = await _chatConversationService.SendRequestToModelAsync(content, _responseCancellationTokenSource.Token);
            Messages.Add(CreateUiMessage(assistantMessage));
        }
        catch (OperationCanceledException)
        {
            Messages.Add(ChatMessageDisplayItem.Create(ChatRole.Assistant, "Response cancelled."));
        }
        finally
        {
            _responseCancellationTokenSource?.Dispose();
            _responseCancellationTokenSource = null;
            RefreshTokenCountsFromService();
        }



    }






    private void ApplyTokenUsageSnapshot(TokenUsageSnapshot snapshot)
    {
        TotalTokenCount = snapshot.TotalTokens;
        SessionTokenCount = snapshot.SessionTokens;
        RagTokenCount = snapshot.RagTokens;
        ToolTokenCount = snapshot.ToolTokens;
        SystemTokenCount = snapshot.SystemTokens;
        InputTokenCount = snapshot.InputTokens;
        OutputTokenCount = snapshot.OutputTokens;
        CachedInputTokenCount = snapshot.CachedInputTokens;
        ReasoningTokenCount = snapshot.ReasoningTokens;
    }






    private void OnTokenUsageUpdated(object sender, TokenUsageSnapshot e)
    {
        ArgumentNullException.ThrowIfNull(e);

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            ApplyTokenUsageSnapshot(e);
            return;
        }

        dispatcher.Invoke(() => ApplyTokenUsageSnapshot(e));
    }






    private void RefreshTokenCountsFromService()
    {
        ApplyTokenUsageSnapshot(new TokenUsageSnapshot(
                _chatConversationService.ContextTokenCount,
                _chatConversationService.SessionTokenCount,
                _chatConversationService.RagTokenCount,
                _chatConversationService.ToolTokenCount,
                _chatConversationService.SystemTokenCount,
                0,
                0,
                0,
                0,
                "viewmodel.refresh",
                DateTimeOffset.UtcNow,
                new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase)));
    }
}