// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         IChatConversationService.cs
// Author: Kyle L. Crowder
// Build Num: 194445



using AgentAILib.Models;

using Microsoft.Extensions.AI;




namespace AgentAILib.Contracts;





public interface IChatConversationService
{

    /// <summary>
    ///     Gets the active Semantic Kernel chat history for the current conversation.
    /// </summary>
    List<ChatMessage> AIHistory { get; }

    string ConversationId { get; }

    event EventHandler<bool> BusyStateChanged;








    /// <summary>
    ///     Switches the active chat model to the given descriptor. The change takes effect
    ///     on the next message send; the conversation history is preserved.
    /// </summary>
    Task ChangeModelAsync(AIModelDescriptor descriptor, CancellationToken token);








    ValueTask<IEnumerable<ChatMessage>> LoadConversationHistoryAsync(CancellationToken token);


    ValueTask<ChatMessage> SendRequestToModelAsync(string content, CancellationToken token);


    Task StartNewConversationAsync(CancellationToken cancellationToken);
}