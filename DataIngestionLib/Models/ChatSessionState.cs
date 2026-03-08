// 2026/03/07
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         ChatSessionState.cs
//   Author: Kyle L. Crowder



namespace DataIngestionLib.Models;





public sealed record ChatSessionState
{

    public int ContextTokenCount { get; init; }

    public ChatHistory ContextWindow { get; init; } = [];
    public ChatHistory History { get; init; } = [];
}