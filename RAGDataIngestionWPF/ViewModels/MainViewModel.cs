// 2026/03/05
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF
//  File:         MainViewModel.cs
//   Author: Kyle L. Crowder



using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;




namespace RAGDataIngestionWPF.ViewModels;





public class MainViewModel : ObservableObject
{
    private readonly IChatConversationService _chatConversationService;
    private CancellationTokenSource _responseCancellationTokenSource;
    private ChatSessionState _sessionState;








    public MainViewModel(IChatConversationService chatConversationService)
    {
        ArgumentNullException.ThrowIfNull(chatConversationService);

        _chatConversationService = chatConversationService;
        _sessionState = _chatConversationService.LoadSession();
        Messages = [];

        foreach (ChatMessage message in _sessionState.History)
        {
            Messages.Add(message);
        }

        ContextTokenCount = _sessionState.ContextTokenCount;

        SendMessageCommand = new AsyncRelayCommand(SendMessageAsync, CanSendMessage);
        CancelMessageCommand = new RelayCommand(CancelMessage, CanCancelMessage);
    }








    public IRelayCommand CancelMessageCommand { get; }





    public int ContextTokenCount
    {
        get; set => SetProperty(ref field, value);
    }





    public bool IsGenerating
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
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
            if (SetProperty(ref field, value))
            {
                SendMessageCommand.NotifyCanExecuteChanged();
            }
        }
    } = string.Empty;





    public ObservableCollection<ChatMessage> Messages { get; }





    public IAsyncRelayCommand SendMessageCommand { get; }








    private void AppendMessage(ChatMessage message)
    {
        _sessionState = _chatConversationService.AppendMessage(_sessionState, message);
        Messages.Add(message);
        ContextTokenCount = _sessionState.ContextTokenCount;
        PersistSession();
    }








    private bool CanCancelMessage()
    {
        return IsGenerating;
    }

    private void CancelMessage()
    {
        _responseCancellationTokenSource?.Cancel();
    }

    private bool CanSendMessage()
    {
        return !IsGenerating && !string.IsNullOrWhiteSpace(MessageInput);
    }

    private void PersistSession()
    {
        _chatConversationService.SaveSession(_sessionState);
    }








    private async Task SendMessageAsync()
    {
        string content = MessageInput.Trim();
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        AppendMessage(_chatConversationService.CreateUserMessage(content));
        MessageInput = string.Empty;
        IsGenerating = true;
        _responseCancellationTokenSource = new CancellationTokenSource();

        try
        {
            ChatMessage assistantMessage = await _chatConversationService.GenerateAssistantMessageAsync(content, ContextTokenCount, _responseCancellationTokenSource.Token);
            AppendMessage(assistantMessage);
        }
        catch (OperationCanceledException)
        {
            AppendMessage(_chatConversationService.CreateAssistantMessage("Response canceled."));
        }
        finally
        {
            IsGenerating = false;
            _responseCancellationTokenSource?.Dispose();
            _responseCancellationTokenSource = null;
        }
    }
}