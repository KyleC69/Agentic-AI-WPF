// Build Date: 2026/03/16
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IChatHistoryProvider.cs
// Author: Kyle L. Crowder
// Build Num: 051919



using DataIngestionLib.Models;




namespace DataIngestionLib.Contracts.Services;





public interface IChatHistoryProvider
{

    ValueTask<PersistedChatMessage> CreateMessageAsync(PersistedChatMessage message, CancellationToken cancellationToken = default);


    ValueTask<int> DeleteConversationAsync(string conversationId, CancellationToken cancellationToken = default);


    ValueTask<bool> DeleteMessageAsync(Guid messageId, CancellationToken cancellationToken = default);


    ValueTask EnsureInitializedAsync(CancellationToken cancellationToken = default);


    ValueTask<PersistedChatMessage?> GetMessageAsync(Guid messageId, CancellationToken cancellationToken = default);


    ValueTask<IReadOnlyList<PersistedChatMessage>> GetMessagesAsync(string conversationId, int? take, CancellationToken cancellationToken = default);


    ValueTask<PersistedChatMessage?> UpdateMessageAsync(Guid messageId, string content, DateTimeOffset timestampUtc, CancellationToken cancellationToken = default);
}