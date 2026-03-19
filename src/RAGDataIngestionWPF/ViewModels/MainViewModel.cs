// Build Date: 2026/03/16
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         MainViewModel.cs
// Author: Kyle L. Crowder
// Build Num: 051905



using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DataIngestionLib.Contracts.Services;

using Microsoft.Extensions.AI;

using RAGDataIngestionWPF.Models;




namespace RAGDataIngestionWPF.ViewModels;





public sealed partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly IChatConversationService _chatConversationService;
    private CancellationTokenSource _responseCancellationTokenSource;

    //Running Token counts for different categories
    [ObservableProperty] private int ragTokenCount;
    [ObservableProperty] private int toolTokenCount;
    [ObservableProperty] private int systemTokenCount;
    [ObservableProperty] private int sessionTokenCount;
    [ObservableProperty] private int totalTokenCount;




    public MainViewModel(IChatConversationService chatConversationService)
    {
        ArgumentNullException.ThrowIfNull(chatConversationService);

        _chatConversationService = chatConversationService;
        Messages = new ObservableCollection<ChatMessageDisplayItem>();

        SendMessageCommand = new AsyncRelayCommand(SendMessageAsync, CanSendMessage);
        CancelMessageCommand = new RelayCommand(CancelMessage, CanCancelMessage);

        _chatConversationService.BusyStateChanged += OnBusyStateChange;



    }








    private void OnBusyStateChange(object sender, bool e)
    {
        IsBusy = e;
    }








    public IRelayCommand CancelMessageCommand { get; }

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

    public ObservableCollection<ChatMessageDisplayItem> Messages { get; }

    public IAsyncRelayCommand SendMessageCommand { get; }








    /// <inheritdoc />
    public void Dispose()
    {

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
            TotalTokenCount = _chatConversationService.ContextTokenCount;
            SessionTokenCount= _chatConversationService.SessionTokenCount;
            RagTokenCount = _chatConversationService.RagTokenCount;
            ToolTokenCount = _chatConversationService.ToolTokenCount;
            SystemTokenCount = _chatConversationService.SystemTokenCount;
        }



    }
}