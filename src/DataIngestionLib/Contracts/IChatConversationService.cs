// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IChatConversationService.cs
// Author: Kyle L. Crowder
// Build Num: 212853



using Microsoft.Extensions.AI;




namespace DataIngestionLib.Contracts;





public interface IChatConversationService
{

    /// <summary>
    ///     Gets the active Semantic Kernel chat history for the current conversation.
    /// </summary>
    List<ChatMessage> AIHistory { get; }

    string ConversationId { get; }

    event EventHandler<bool> BusyStateChanged;


    ValueTask<IEnumerable<ChatMessage>> LoadConversationHistoryAsync(CancellationToken token);


    ValueTask<ChatMessage> SendRequestToModelAsync(string content, CancellationToken token);


    Task StartNewConversationAsync(CancellationToken cancellationToken);
}