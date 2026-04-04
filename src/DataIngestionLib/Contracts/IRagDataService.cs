// Build Date: 2026/04/04
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IRagDataService.cs
// Author: GitHub Copilot


using Microsoft.Extensions.AI;


namespace DataIngestionLib.Contracts;



public interface IRagDataService
{
    Task<IReadOnlyList<ChatMessage>?> GetChatHistoryByConversationId(Guid conversationId, CancellationToken cancellationToken = default);





    Task<IEnumerable<ChatMessage>> GetRagDataEntries(string query, CancellationToken cancellationToken = default);
}
