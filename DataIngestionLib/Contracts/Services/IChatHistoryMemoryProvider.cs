using DataIngestionLib.Models;

using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace DataIngestionLib.Contracts.Services;

public interface IChatHistoryMemoryProvider
{
    ValueTask<ChatHistory> BuildContextMessagesAsync(string conversationId, ChatHistory currentRequestMessages, CancellationToken cancellationToken = default);

    ValueTask StoreMessagesAsync(string conversationId, string sessionId, string agentId, string userId, string applicationId, ChatHistory requestMessages, ChatHistory responseMessages, CancellationToken cancellationToken = default);

    ValueTask<int> PruneConversationAsync(string conversationId, CancellationToken cancellationToken = default);

    ValueTask<PersistedChatMessage?> UpdateMessageContentAsync(Guid messageId, string content, DateTimeOffset timestampUtc, CancellationToken cancellationToken = default);

    ValueTask<bool> DeleteMessageAsync(Guid messageId, CancellationToken cancellationToken = default);
}
