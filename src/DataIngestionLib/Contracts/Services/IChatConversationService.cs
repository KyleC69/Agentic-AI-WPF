// Build Date: 2026/03/16
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IChatConversationService.cs
// Author: Kyle L. Crowder
// Build Num: 051919



using Microsoft.Extensions.AI;




namespace DataIngestionLib.Contracts.Services;





public interface IChatConversationService
{

    /// <summary>
    ///     Gets the active Semantic Kernel chat history for the current conversation.
    /// </summary>
    List<ChatMessage> AIHistory { get; }

    /// <summary>
    ///     Gets the current context token count for the active chat history.
    /// </summary>
    int ContextTokenCount { get; }


    ValueTask<ChatMessage> SendRequestToModelAsync(string content, CancellationToken token);
    
    
    public event EventHandler<int>? SessionTokenChange;
    public event EventHandler<int>? SystemTokenChange;
    public event EventHandler<int>? RagTokenChange;
    public event EventHandler<int>? ToolTokenChange;
    public event EventHandler<int>? MaximumContextWarning; // Event to signal when the context token count is approaching the maximum limit, providing the current token count as an argument.
    //Thrown when the session token budget is near exhausted, allowing for immediate window trimming.
    //This is intended to be a proactive measure to prevent hitting hard limits on the model and allow
    // for some trimming on the backend before throwning. Ideal scenario is if a tool or context injector
    //suddenly bloats the session history.
    public  event EventHandler? SessionBugetExceeded;
    public  event EventHandler? TokenBudgetExceeded;

}