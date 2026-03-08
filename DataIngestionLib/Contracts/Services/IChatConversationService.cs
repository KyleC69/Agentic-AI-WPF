// 2026/03/07
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         IChatConversationService.cs
//   Author: Kyle L. Crowder



using DataIngestionLib.Models;





namespace DataIngestionLib.Contracts.Services;





public interface IChatConversationService
{

    /// <summary>
    ///     Gets the active Semantic Kernel chat history for the current conversation.
    /// </summary>
    ChatHistory ChatHistory { get; }





    /// <summary>
    ///     Gets the current context token count for the active chat history.
    /// </summary>
    int ContextTokenCount { get; }



    ValueTask<AIChatMessage> SendRequestToModelAsync(string content, CancellationToken token);


    //   ChatMessage AddAssistantMessage(string responseCanceled);
}