// Build Date: 2026/03/11
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ChatSessionState.cs
// Author: Kyle L. Crowder
// Build Num: 105642



namespace DataIngestionLib.Models;





public sealed record ChatSessionState
{

    public int ContextTokenCount { get; init; }

    public AIChatHistory ContextWindow { get; init; } = [];
    public AIChatHistory History { get; init; } = [];
}