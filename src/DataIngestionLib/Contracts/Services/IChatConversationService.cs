// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//
//
//
//



using Microsoft.Extensions.AI;




namespace DataIngestionLib.Contracts.Services;





public interface IChatConversationService
{

    /// <summary>
    ///     Gets the active Semantic Kernel chat history for the current conversation.
    /// </summary>
    List<ChatMessage> AIHistory { get; }

    /// <summary>
    ///     Gets the total current context token count for the active chat history.
    /// </summary>
    int ContextTokenCount { get; }

    string ConversationId { get; }

    //Tokens used for RAG context, including prompt and response tokens affecting overall context size.
    int RagTokenCount { get; }

    //All token not otherwise accounted for including user
    int SessionTokenCount { get; }

    //Tokens used for system instructions, including prompt and response tokens affecting overall context size.
    int SystemTokenCount { get; }

    //Tokens used for tool calls, including prompt and response tokens affecting overall context size.
    int ToolTokenCount { get; }

    event EventHandler<bool> BusyStateChanged;


    ValueTask<IReadOnlyList<ChatMessage>> LoadConversationHistoryAsync(CancellationToken token);


    ValueTask<ChatMessage> SendRequestToModelAsync(string content, CancellationToken token);


    Task StartNewConversationAsync(CancellationToken cancellationToken);


}