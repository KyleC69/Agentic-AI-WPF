// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IChatConversationService.cs
// Author: Kyle L. Crowder
// Build Num: 212853



using Microsoft.Extensions.AI;

using DataIngestionLib.Models;




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


    /// <summary>
    ///     Switches the active chat model to the given descriptor. The change takes effect
    ///     on the next message send; the conversation history is preserved.
    /// </summary>
    Task ChangeModelAsync(AIModelDescriptor descriptor, CancellationToken token);


    Task StartNewConversationAsync(CancellationToken cancellationToken);
}