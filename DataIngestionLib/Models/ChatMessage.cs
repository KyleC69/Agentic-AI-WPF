// 2026/03/05
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         ChatMessage.cs
//   Author: Kyle L. Crowder



namespace DataIngestionLib.Models;





public sealed record ChatMessage
{

    public string Content { get; init; } = string.Empty;

    public string FormattedContent { get; init; } = string.Empty;





    public bool IsUser => Role == ChatMessageRole.User;





    public ChatMessageRole Role { get; init; }

    public DateTimeOffset TimestampUtc { get; init; }

    public int TokenCount { get; init; }
}