// Build Date: 2026/03/19
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IChatConversationService.cs
// Author: Kyle L. Crowder
// Build Num: 044229



using Microsoft.Extensions.AI;




namespace DataIngestionLib.Contracts.Services;





public interface IChatConversationService
{

    string ConversationId { get; }

    /// <summary>
    ///     Gets the active Semantic Kernel chat history for the current conversation.
    /// </summary>
    List<ChatMessage> AIHistory { get; }

    /// <summary>
    ///     Gets the total current context token count for the active chat history.
    /// </summary>
    int ContextTokenCount { get; }

    //Tokens used for RAG context, including prompt and response tokens affecting overall context size.
    int RagTokenCount { get; }

    //All token not otherwise accounted for including user
    int SessionTokenCount { get; }

    //Tokens used for system instructions, including prompt and response tokens affecting overall context size.
    int SystemTokenCount { get; }

    //Tokens used for tool calls, including prompt and response tokens affecting overall context size.
    int ToolTokenCount { get; }

    event EventHandler<bool> BusyStateChanged;
    ValueTask<IReadOnlyList<ChatMessage>> LoadConversationHistoryAsync(CancellationToken token = default);
    ValueTask<ChatMessage> SendRequestToModelAsync(string content, CancellationToken token);
}