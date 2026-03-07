using DataIngestionLib.Models;

using Microsoft.Extensions.AI;

namespace DataIngestionLib.Contracts.Services;

public interface IChatHistorySummarizer
{
    ValueTask<ChatMessage?> SummarizeAsync(string conversationId, ChatHistory messages, CancellationToken cancellationToken = default);
}
