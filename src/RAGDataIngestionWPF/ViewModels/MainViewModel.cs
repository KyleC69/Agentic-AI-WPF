// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         MainViewModel.cs
// Author: Kyle L. Crowder
// Build Num: 212941



using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DataIngestionLib.Contracts;
using DataIngestionLib.Services;

using Microsoft.Extensions.AI;

using RAGDataIngestionWPF.Contracts.ViewModels;

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
    private readonly IWorkflowConversationService _workflow;








    public MainViewModel(IChatConversationService chatConversationService, IHistoryIdentityService historyIdentityService, IWorkflowConversationService workflowConversationService)
    {
        ArgumentNullException.ThrowIfNull(chatConversationService);
        _historyIdentity = historyIdentityService;
        _chatConversationService = chatConversationService;
        Messages = new ObservableCollection<ChatMessage>();

        SendMessageCommand = new AsyncRelayCommand(SendMessageAsync, CanSendMessage);
        CancelMessageCommand = new RelayCommand(CancelMessage, CanCancelMessage);

        _chatConversationService.BusyStateChanged += OnBusyStateChange;

        NewConvoCommand = new AsyncRelayCommand(StartNewConversationAsync);

        // Need to link this back to applicaion lifecycle
        _tokenSource = new CancellationTokenSource();
        _workflow = workflowConversationService;
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
                SendMessageCommand.NotifyCanExecuteChanged();
                CancelMessageCommand.NotifyCanExecuteChanged();
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

    public ObservableCollection<ChatMessage> Messages { get; }
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
            foreach (ChatMessage historyMessage in historyMessages)
            {
                Messages.Add(historyMessage);
            }


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








    private void OnBusyStateChange(object sender, bool e)
    {
        RunOnUiThread(() => IsBusy = e);
    }








    private static void RunOnUiThread(Action updateAction)
    {
        ArgumentNullException.ThrowIfNull(updateAction);

        Dispatcher dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            updateAction();
            return;
        }

        _ = dispatcher.InvokeAsync(updateAction);
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
        Messages.Add(new ChatMessage(ChatRole.User, content));

        //Clear UI input
        MessageInput = string.Empty;

        int? liveAssistantMessageIndex = null;
        StringBuilder streamedAssistantText = new();

        try
        {
            //  ChatMessage assistantMessage = await _chatConversationService.SendRequestToModelAsync(content, _tokenSource.Token);
            //Messages.Add(assistantMessage);

            await _workflow.InitializeAsync();
            List<ChatMessage> response = await _workflow.RunStreamingWorkflowAsync(
                content,
                (updateMessage, cancellationToken) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string chunkText = updateMessage.Text;
                    if (string.IsNullOrWhiteSpace(chunkText))
                    {
                        return Task.CompletedTask;
                    }

                    lock (streamedAssistantText)
                    {
                        streamedAssistantText.Append(chunkText);

                        ChatMessage liveMessage = new(ChatRole.Assistant, streamedAssistantText.ToString());
                        RunOnUiThread(() =>
                        {
                            if (liveAssistantMessageIndex is null)
                            {
                                Messages.Add(liveMessage);
                                liveAssistantMessageIndex = Messages.Count - 1;
                                return;
                            }

                            Messages[liveAssistantMessageIndex.Value] = liveMessage;
                        });
                    }

                    return Task.CompletedTask;
                },
                _tokenSource.Token);

            if (liveAssistantMessageIndex is null)
            {
                StringBuilder fallbackAssistantText = new();
                foreach (ChatMessage message in response)
                {
                    if (message.Role == ChatRole.User || string.IsNullOrWhiteSpace(message.Text))
                    {
                        continue;
                    }

                    if (fallbackAssistantText.Length > 0)
                    {
                        fallbackAssistantText.AppendLine();
                    }

                    fallbackAssistantText.Append(message.Text.Trim());
                }

                if (fallbackAssistantText.Length > 0)
                {
                    Messages.Add(new ChatMessage(ChatRole.Assistant, fallbackAssistantText.ToString()));
                }
            }


        }
        catch (OperationCanceledException)
        {
            Messages.Add(new ChatMessage(ChatRole.Assistant, "Response cancelled."));
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
}