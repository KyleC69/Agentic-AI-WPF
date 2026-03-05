using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;

namespace RAGDataIngestionWPF.ViewModels;

public class MainViewModel : ObservableObject
{
    private readonly IChatConversationService _chatConversationService;
    private ChatSessionState _sessionState;
    private CancellationTokenSource _responseCancellationTokenSource;
    private string _messageInput = string.Empty;
    private bool _isGenerating;
    private int _contextTokenCount;

    public MainViewModel(IChatConversationService chatConversationService)
    {
        ArgumentNullException.ThrowIfNull(chatConversationService);

        _chatConversationService = chatConversationService;
        _sessionState = _chatConversationService.LoadSession();
        Messages = [];

        foreach (var message in _sessionState.History)
        {
            Messages.Add(message);
        }

        ContextTokenCount = _sessionState.ContextTokenCount;

        SendMessageCommand = new AsyncRelayCommand(SendMessageAsync, CanSendMessage);
        CancelMessageCommand = new RelayCommand(CancelMessage, CanCancelMessage);
    }

    public ObservableCollection<ChatMessage> Messages { get; }

    public string MessageInput
    {
        get => _messageInput;
        set
        {
            if (SetProperty(ref _messageInput, value))
            {
                SendMessageCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool IsGenerating
    {
        get => _isGenerating;
        set
        {
            if (SetProperty(ref _isGenerating, value))
            {
                SendMessageCommand.NotifyCanExecuteChanged();
                CancelMessageCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public int ContextTokenCount
    {
        get => _contextTokenCount;
        set => SetProperty(ref _contextTokenCount, value);
    }

    public IAsyncRelayCommand SendMessageCommand { get; }

    public IRelayCommand CancelMessageCommand { get; }

    private bool CanSendMessage()
        => !IsGenerating && !string.IsNullOrWhiteSpace(MessageInput);

    private bool CanCancelMessage()
        => IsGenerating;

    private async Task SendMessageAsync()
    {
        var content = MessageInput.Trim();
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
            var assistantMessage = await _chatConversationService.GenerateAssistantMessageAsync(content, ContextTokenCount, _responseCancellationTokenSource.Token);
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

    private void CancelMessage()
        => _responseCancellationTokenSource?.Cancel();

    private void AppendMessage(ChatMessage message)
    {
        _sessionState = _chatConversationService.AppendMessage(_sessionState, message);
        Messages.Add(message);
        ContextTokenCount = _sessionState.ContextTokenCount;
        PersistSession();
    }

    private void PersistSession()
        => _chatConversationService.SaveSession(_sessionState);
}
