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
using DataIngestionLib.Providers;

using Microsoft.Extensions.AI;

using RAGDataIngestionWPF.Models;




namespace RAGDataIngestionWPF.ViewModels;





public sealed partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly IChatConversationService _chatConversationService;
    private CancellationTokenSource _responseCancellationTokenSource;

    [ObservableProperty] private int ragTokenCount;
    [ObservableProperty] private int toolTokenCount;
    [ObservableProperty] private int systemTokenCount;
    [ObservableProperty] private int sessionTokenCount;
    [ObservableProperty] private int totalcontextTokenCount;




    public MainViewModel(IChatConversationService chatConversationService)
    {
        ArgumentNullException.ThrowIfNull(chatConversationService);

        _chatConversationService = chatConversationService;
        Messages = new ObservableCollection<ChatMessageDisplayItem>();

        SendMessageCommand = new AsyncRelayCommand(SendMessageAsync, CanSendMessage);
        CancelMessageCommand = new RelayCommand(CancelMessage, CanCancelMessage);
        
        
        
        /// Wire up events thrown by the LLM service to update
        /// token countes and kepep UI accurate.
        _chatConversationService.SessionTokenChange += OnSessionTokenChange;
        _chatConversationService.SystemTokenChange += OnSystemTokenChange;
        _chatConversationService.RagTokenChange += OnRagTokenChange;
        _chatConversationService.ToolTokenChange += OnToolTokenChange;
        _chatConversationService.MaximumContextWarning += OnMaximumContextWarning;
        _chatConversationService.SessionBugetExceeded += OnSessionBudgetExceeded;
        
        
    }

    
    
    
    
    
    
    
    
    /// <summary>
    /// Main conversational history containker is nearing token budget.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnSessionBudgetExceeded(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }
    ///<summary>Triggered when the maximum context warning is reached. Will trigger context reducer</summary>    
    private void OnMaximumContextWarning(object sender, int e)
    {
        throw new NotImplementedException();
    }
     /// <summary>
     /// Updates tool token UI
     /// </summary>
     /// <param name="sender"></param>
     /// <param name="e"></param>
     /// <exception cref="NotImplementedException"></exception>
    private void OnToolTokenChange(object sender, int e)
    {
        throw new NotImplementedException();
    }
    /// <summary>
    /// Updates RAG token UI
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnRagTokenChange(object sender, int e)
    {
        throw new NotImplementedException();
    }
    /// <summary>
    /// Updates system token UI
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnSystemTokenChange(object sender, int e)
    {
        throw new NotImplementedException();
    }
    /// <summary>
    /// Handles changes to the session token when triggered by an event.
    /// </summary>
    /// <param name="sender">The source of the event that triggered the session token change.</param>
    /// <param name="e">An integer value associated with the session token change event.</param>
    /// <exception cref="NotImplementedException">Always thrown as this method is not yet implemented.</exception>
    private void OnSessionTokenChange(object sender, int e)
    {
        throw new NotImplementedException();
    }








    













public IRelayCommand CancelMessageCommand { get; }

    public bool IsGenerating
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

        //Set UI state to busy TODO: add bool IsBusy prop
        IsGenerating = true;

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
            IsGenerating = false;
            _responseCancellationTokenSource?.Dispose();
            _responseCancellationTokenSource = null;
            TotalcontextTokenCount = _chatConversationService.ContextTokenCount;
        }



    }
}