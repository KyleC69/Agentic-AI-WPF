using System.Text.Json;

namespace DataIngestionLib.Models;

public sealed record PersistedChatMessage
{
    public Guid MessageId { get; init; }

    public string ConversationId { get; init; } = string.Empty;

    public string SessionId { get; init; } = string.Empty;

    public string AgentId { get; init; } = string.Empty;

    public string UserId { get; init; } = string.Empty;

    public string ApplicationId { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;

    public string Content { get; init; } = string.Empty;

    public DateTimeOffset TimestampUtc { get; init; }

    public JsonDocument? Metadata { get; init; }
}
