// 2026/03/07
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         IChatHistoryMemoryProvider.cs
//   Author: Kyle L. Crowder



using DataIngestionLib.Models;




namespace DataIngestionLib.Services;





public interface IChatHistoryMemoryProvider
{
    ValueTask<IEnumerable<AIChatMessage>> BuildContextMessagesAsync(
            string conversationId,
            ChatHistory currentRequestMessages,
            CancellationToken cancellationToken = default);








    ValueTask StoreMessagesAsync(
            string conversationId,
            string sessionId,
            string agentId,
            string userId,
            string applicationId,
            ChatHistory requestMessages,
            ChatHistory responseMessages,
            CancellationToken cancellationToken = default);








    ValueTask<int> PruneConversationAsync(string conversationId, CancellationToken cancellationToken = default);


    ValueTask<PersistedChatMessage?> UpdateMessageContentAsync(Guid messageId, string content, DateTimeOffset timestampUtc, CancellationToken cancellationToken = default);


    ValueTask<bool> DeleteMessageAsync(Guid messageId, CancellationToken cancellationToken = default);
}