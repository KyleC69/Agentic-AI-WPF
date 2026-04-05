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