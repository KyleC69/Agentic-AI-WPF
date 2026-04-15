// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         ChatHistoryMessage.cs
// Author: Kyle L. Crowder
// Build Num: 194453



using Microsoft.Data.SqlTypes;




namespace AgentAILib.HistoryModels;





public class ChatHistoryMessage
{

    public string AgentId { get; set; } = null!;

    public string ApplicationId { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string ConversationId { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public SqlVector<float>? Embedding { get; set; }

    public bool? Enabled { get; set; }
    public Guid MessageId { get; set; }

    public string? Metadata { get; set; }

    public string Role { get; set; } = null!;

    public string? Summary { get; set; }
    public int TokenCnt { get; set; } = 0;

    public string UserId { get; set; } = null!;
}