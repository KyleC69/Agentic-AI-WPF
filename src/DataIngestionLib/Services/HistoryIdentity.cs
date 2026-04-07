// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         HistoryIdentity.cs
// Author: Kyle L. Crowder
// Build Num: 212915



using System.Text.Json.Serialization;

using Microsoft.Extensions.AI;




namespace DataIngestionLib.Services;





public class HistoryIdentity
{
    public HistoryIdentity(string conversationId)
    {
        ConversationId = conversationId;
    }








    [JsonPropertyName("agentid")] public string AgentId { get; set; } = string.Empty;
    [JsonPropertyName("applicationid")] public string ApplicationId { get; set; } = string.Empty;
    [JsonPropertyName("conversationid")] public string ConversationId { get; set; } = string.Empty;

    [JsonPropertyName("messages")] public List<ChatMessage> Messages { get; set; } = [];

    [JsonPropertyName("userid")] public string UserId { get; set; } = string.Empty;
}