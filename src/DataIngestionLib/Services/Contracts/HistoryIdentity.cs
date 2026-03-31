// Build Date: 2026/03/30
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         HistoryIdentity.cs
// Author: Kyle L. Crowder
// Build Num: 233137



using System.Text.Json.Serialization;

using Microsoft.Extensions.AI;




namespace DataIngestionLib.Services.Contracts;





public record HistoryIdentity
{

    public string AgentId { get; set; } = string.Empty;
    public string ApplicationId { get; set; } = string.Empty;
    public string ConversationId { get; set; } = string.Empty;

    [JsonPropertyName("messages")] public List<ChatMessage> Messages { get; set; } = [];

    public string UserId { get; set; } = string.Empty;
}