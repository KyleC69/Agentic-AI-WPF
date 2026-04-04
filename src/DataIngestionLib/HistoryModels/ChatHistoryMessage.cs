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



using Microsoft.Data.SqlTypes;




namespace DataIngestionLib.HistoryModels;





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

    public DateTime TimestampUtc { get; set; }

    public string UserId { get; set; } = null!;
    public int TokenCnt { get; set; } = 0;
}