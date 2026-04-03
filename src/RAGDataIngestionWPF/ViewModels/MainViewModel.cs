// Build Date: 2026/04/03
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         MainViewModel.cs
// Author: Kyle L. Crowder
// Build Num: 095219



using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DataIngestionLib.Agents;
using DataIngestionLib.Contracts.Services;

using Microsoft.Extensions.AI;

using RAGDataIngestionWPF.Contracts.ViewModels;
using RAGDataIngestionWPF.Models;

using ChatMessage = Microsoft.Extensions.AI.ChatMessage;




namespace RAGDataIngestionWPF.ViewModels;





public sealed partial class MainViewModel : ObservableObject, IDisposable, INavigationAware
{
    private readonly IChatConversationService _chatConversationService;
    private readonly IHistoryIdentityService _historyIdentity;
    private bool _historyLoaded;
    private CancellationTokenSource _tokenSource;
    [ObservableProperty] private int cachedInputTokenCount;
    [ObservableProperty] private int inputTokenCount;
    [ObservableProperty] private int outputTokenCount;

    //Running Token counts for different categories
    [ObservableProperty] private int ragTokenCount;
    [ObservableProperty] private int reasoningTokenCount;
    [ObservableProperty] private int sessionTokenCount;
    [ObservableProperty] private int systemTokenCount;
    [ObservableProperty] private int toolTokenCount;
    [ObservableProperty] private int totalTokenCount;








    public MainViewModel(IChatConversationService chatConversationService, IHistoryIdentityService historyIdentityService)
    {
        ArgumentNullException.ThrowIfNull(chatConversationService);
        _historyIdentity = historyIdentityService;
        _chatConversationService = chatConversationService;
        Messages = new ObservableCollection<ChatMessageDisplayItem>();

        SendMessageCommand = new AsyncRelayCommand(SendMessageAsync, CanSendMessage);
        CancelMessageCommand = new RelayCommand(CancelMessage, CanCancelMessage);

        _chatConversationService.BusyStateChanged += OnBusyStateChange;

        NewConvoCommand = new AsyncRelayCommand(StartNewConversationAsync);

        // Need to link this back to applicaion lifecycle
        _tokenSource = new CancellationTokenSource();
        TokenAccountingMiddleware.SystemTokensChanged += (_, e) => UpdateSystemTokenCount(e);
        TokenAccountingMiddleware.CachedInputTokensChanged += (_, e) => UpdateCachedInputTokenCount(e);
        TokenAccountingMiddleware.InputTokensChanged += (_, e) => UpdateInputTokenCount(e);
        TokenAccountingMiddleware.OutputTokensChanged += (_, e) => UpdateOutputTokenCount(e);
        TokenAccountingMiddleware.RagTokensChanged += (_, e) => UpdateRagTokenCount(e);
        TokenAccountingMiddleware.ReasoningTokensChanged += (_, e) => UpdateReasoningTokenCount(e);
        TokenAccountingMiddleware.SessionTokensChanged += (_, e) => UpdateSessionTokenCount(e);
        TokenAccountingMiddleware.ToolTokensChanged += (_, e) => UpdateToolTokenCount(e);
        TokenAccountingMiddleware.TotalTokensChanged += (_, e) => UpdateTotalTokenCount(e);


    }








    public IRelayCommand CancelMessageCommand { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether the application is currently in a busy state.
    /// </summary>
    /// <remarks>
    ///     This property is used to manage the application's busy state, which affects the availability of commands
    ///     such as <see cref="SendMessageCommand" /> and <see cref="CancelMessageCommand" />. It also triggers the
    ///     <see cref="OnBusyStateChange" /> method to handle state changes.
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
    public IAsyncRelayCommand NewConvoCommand { get; }

    public IAsyncRelayCommand SendMessageCommand { get; }








    public void Dispose()
    {
        _tokenSource?.Dispose();
        _chatConversationService.BusyStateChanged -= OnBusyStateChange;

    }








    /// <summary>
    ///     Called when the view model is navigated away from.
    /// </summary>
    /// <remarks>
    ///     This method is part of the <see cref="INavigationAware" /> interface and is invoked
    ///     to handle any cleanup or state-saving logic when the associated view is no longer active.
    /// </remarks>
    public void OnNavigatedFrom()
    {
    }








    /// <summary>
    ///     Called when the view model is navigated to.
    /// </summary>
    /// <param name="parameter">
    ///     An optional parameter passed during navigation. This can be used to provide
    ///     context or data required by the view model.
    /// </param>
    /// <remarks>
    ///     This method is part of the <see cref="INavigationAware" /> interface and is invoked
    ///     to handle initialization logic, such as loading data or setting up state, when the
    ///     associated view becomes active.
    /// </remarks>
    public async void OnNavigatedTo(object parameter)
    {

        if (_historyLoaded)
        {
            return;
        }

        try
        {
            Messages.Clear();

            var historyMessages = await _chatConversationService.LoadConversationHistoryAsync(_tokenSource.Token).ConfigureAwait(true);

            //Add the history messages to the UI collection
            foreach (ChatMessage historyMessage in historyMessages) Messages.Add(CreateUiMessage(historyMessage));

            //Add old messages to the context so agent can pick up where it left off.
            _historyIdentity.Current.Messages.AddRange(historyMessages);

            _historyLoaded = true;
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
        _tokenSource?.Cancel();
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
        _tokenSource = new CancellationTokenSource();

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
            ChatMessage assistantMessage = await _chatConversationService.SendRequestToModelAsync(content, _tokenSource.Token);
            Messages.Add(CreateUiMessage(assistantMessage));
        }
        catch (OperationCanceledException)
        {
            Messages.Add(ChatMessageDisplayItem.Create(ChatRole.Assistant, "Response cancelled."));
        }
        finally
        {
            _tokenSource?.Dispose();
            _tokenSource = null;
        }



    }








    private async Task StartNewConversationAsync(CancellationToken arg)
    {
        // Clear the current conversation in the service, which should trigger the UI to clear as well.
        await _chatConversationService.StartNewConversationAsync(arg);
        Messages.Clear();
    }








    private void UpdateCachedInputTokenCount(TokenAccountingMiddleware.TokenCategoryChangedEventArgs e)
    {
        CachedInputTokenCount = e.CurrentValue;
    }








    private void UpdateInputTokenCount(TokenAccountingMiddleware.TokenCategoryChangedEventArgs e)
    {
        InputTokenCount = e.CurrentValue;
    }








    private void UpdateOutputTokenCount(TokenAccountingMiddleware.TokenCategoryChangedEventArgs e)
    {
        OutputTokenCount = e.CurrentValue;
    }








    private void UpdateRagTokenCount(TokenAccountingMiddleware.TokenCategoryChangedEventArgs e)
    {
        RagTokenCount = e.CurrentValue;
    }








    private void UpdateReasoningTokenCount(TokenAccountingMiddleware.TokenCategoryChangedEventArgs e)
    {
        ReasoningTokenCount = e.CurrentValue;
    }








    private void UpdateSessionTokenCount(TokenAccountingMiddleware.TokenCategoryChangedEventArgs e)
    {
        SessionTokenCount = e.CurrentValue;
    }








    private void UpdateSystemTokenCount(TokenAccountingMiddleware.TokenCategoryChangedEventArgs e)
    {
        SystemTokenCount = e.CurrentValue;
    }








    private void UpdateToolTokenCount(TokenAccountingMiddleware.TokenCategoryChangedEventArgs e)
    {
        ToolTokenCount = e.CurrentValue;
    }








    private void UpdateTotalTokenCount(TokenAccountingMiddleware.TokenCategoryChangedEventArgs e)
    {
        TotalTokenCount = e.CurrentValue;
    }
}