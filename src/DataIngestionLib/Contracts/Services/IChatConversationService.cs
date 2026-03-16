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


    //   ChatMessage AddAssistantMessage(string responseCanceled);
}