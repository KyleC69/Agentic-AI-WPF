using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;

using Microsoft.Agents.AI;

namespace DataIngestionLib.Services;

public sealed class NullRagContextSource : IRagContextSource
{
    public ValueTask<ChatHistory> GetContextMessagesAsync(ChatHistory requestMessages, AgentSession? session, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(requestMessages);

        return ValueTask.FromResult<ChatHistory>([]);
    }
}
