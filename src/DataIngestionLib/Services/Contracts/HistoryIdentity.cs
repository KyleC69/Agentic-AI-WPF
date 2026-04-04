// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//
//
//
//



using System.Text.Json.Serialization;

using Microsoft.Extensions.AI;




namespace DataIngestionLib.Services.Contracts;





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