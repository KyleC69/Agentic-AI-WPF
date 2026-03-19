// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//


namespace DataIngestionLib.Services.Contracts;

public record HistoryIdentity
{
    public string ApplicationId { get; init; } = string.Empty;
    public string SessionId { get; init; } = string.Empty;
    public string ConversationId { get; init; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string MessageId { get; set; } = string.Empty;

    public string AgentId { get; set; } = string.Empty;
    
    
    
    
    
}
