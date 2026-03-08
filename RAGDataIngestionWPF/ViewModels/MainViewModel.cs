// 2026/03/05
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF
//  File:         MainViewModel.cs
//   Author: Kyle L. Crowder



using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;



namespace RAGDataIngestionWPF.ViewModels;





public class MainViewModel : ObservableObject
{
    private readonly IChatConversationService _chatConversationService;
    private CancellationTokenSource _responseCancellationTokenSource;








    public MainViewModel(IChatConversationService chatConversationService)
    {
        ArgumentNullException.ThrowIfNull(chatConversationService);

        _chatConversationService = chatConversationService;
        Messages = [];
        ContextTokenCount = _chatConversationService.ContextTokenCount;

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





    public ChatHistory Messages { get; }





    public IAsyncRelayCommand SendMessageCommand { get; }








    private void AppendMessage(AIChatMessage message)
    {
        Messages.Add(message);

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








    private async Task SendMessageAsync()
    {
        string content = MessageInput.Trim();
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }
        //Add Users message to UI collection
        Messages.AddUserMessage(content);

        // AppendMessage(_chatConversationService.AddUserMessage(content)); 
        //Message is being sent to service already we don't need to add it to the collection again here. The service will add the message to the conversation and then return the assistant response which we will add to the collection in the next step.


        //Clear UI input
        MessageInput = string.Empty;
        //Set UI state to busy TODO: add bool IsBusy prop
        IsGenerating = true;
        //TODO: need to refactor all cancellation token and ensure they are all linked to lifecycle of view model and application lifetime.
        _responseCancellationTokenSource = new CancellationTokenSource();

        try
        {
            AIChatMessage assistantMessage = await _chatConversationService.SendRequestToModelAsync(content, _responseCancellationTokenSource.Token);
            Messages.Add(assistantMessage);
        }
        catch (OperationCanceledException)
        {

            //   AppendMessage(_chatConversationService.AddAssistantMessage("Response canceled."));
        }
        finally
        {
            IsGenerating = false;
            _responseCancellationTokenSource?.Dispose();
            _responseCancellationTokenSource = null;
            //update token count last
            ContextTokenCount = _chatConversationService.ContextTokenCount;
        }



    }






}