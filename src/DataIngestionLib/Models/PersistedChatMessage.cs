// Build Date: 2026/04/03
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         PersistedChatMessage.cs
// Author: Kyle L. Crowder
// Build Num: 095148



using System.Text.Json;




namespace DataIngestionLib.Models;





public sealed record PersistedChatMessage
{

    public string AgentId { get; init; } = string.Empty;

    public string ApplicationId { get; init; } = string.Empty;

    public string Content { get; init; } = string.Empty;

    public string ConversationId { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public Guid MessageId { get; init; }

    public JsonDocument? Metadata { get; init; }

    public string Role { get; init; } = string.Empty;

    public DateTime TimestampUtc { get; init; }

    public string UserId { get; init; } = string.Empty;
}