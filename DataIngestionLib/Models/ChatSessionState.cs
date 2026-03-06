// 2026/03/05
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         ChatSessionState.cs
//   Author: Kyle L. Crowder



namespace DataIngestionLib.Models;





public sealed record ChatSessionState
{

    public int ContextTokenCount { get; init; }

    public IReadOnlyList<ChatMessage> ContextWindow { get; init; } = [];
    public IReadOnlyList<ChatMessage> History { get; init; } = [];
}