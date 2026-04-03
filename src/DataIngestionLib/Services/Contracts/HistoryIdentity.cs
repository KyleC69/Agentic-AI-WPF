// Build Date: 2026/03/31
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         HistoryIdentity.cs
// Author: Kyle L. Crowder
// Build Num: 232102



using System.Text.Json.Serialization;

using Microsoft.Extensions.AI;




namespace DataIngestionLib.Services.Contracts;





public record HistoryIdentity
{

    [JsonPropertyName("agentid")] public string AgentId { get; set; } = string.Empty;
    [JsonPropertyName("applicationid")] public string ApplicationId { get; set; } = string.Empty;
    [JsonPropertyName("conversationid")] public string ConversationId { get; set; } = string.Empty;

    [JsonPropertyName("messages")] public List<ChatMessage> Messages { get; set; } = [];

    [JsonPropertyName("userid")] public string UserId { get; set; } = string.Empty;
}