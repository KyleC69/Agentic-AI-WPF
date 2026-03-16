// Build Date: 2026/03/16
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         PersistedChatMessage.cs
// Author: Kyle L. Crowder
// Build Num: 051925



using System.Text.Json;




namespace DataIngestionLib.Models;





public sealed record PersistedChatMessage
{

    public string AgentId { get; init; } = string.Empty;

    public string ApplicationId { get; init; } = string.Empty;

    public string Content { get; init; } = string.Empty;

    public string ConversationId { get; init; } = string.Empty;
    public Guid MessageId { get; init; }

    public JsonDocument? Metadata { get; init; }

    public string Role { get; init; } = string.Empty;

    public string SessionId { get; init; } = string.Empty;

    public DateTimeOffset TimestampUtc { get; init; }

    public string UserId { get; init; } = string.Empty;
}