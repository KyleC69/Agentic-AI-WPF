// Build Date: 2026/04/04
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ISqlChatHistoryProvider.cs
// Author: GitHub Copilot


using Microsoft.Extensions.AI;


namespace DataIngestionLib.Contracts;



public interface ISqlChatHistoryProvider
{
    IReadOnlyList<string> StateKeys { get; }





    ValueTask<string?> GetLatestConversationIdAsync(CancellationToken cancellationToken = default);





    ValueTask<IEnumerable<ChatMessage>?> GetMessagesAsync(string conversationId, CancellationToken cancellationToken = default);
}
