// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   AgentAILib
// File:         IChatHistoryProvider.cs
// Author: Kyle L. Crowder
// Build Num: 212853

// Build Date: 2026/04/03
// Solution: RAGDataIngestionWPF
// Project:   AgentAILib
// File:         IChatHistoryProvider.cs
// Author: Kyle L. Crowder
// Build Num: 095140

// Build Date: 2026/04/03
// Solution: RAGDataIngestionWPF
// Project:   AgentAILib
// File:         IChatHistoryProvider.cs
// Author: Kyle L. Crowder
// Build Num: 095140

// Build Date: 2026/04/03
// Solution: RAGDataIngestionWPF
// Project:   AgentAILib
// File:         IChatHistoryProvider.cs
// Author: Kyle L. Crowder
// Build Num: 095140



using AgentAILib.Models;




namespace AgentAILib.Contracts;





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