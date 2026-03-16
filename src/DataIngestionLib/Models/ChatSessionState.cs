// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ChatSessionState.cs
// Author: Kyle L. Crowder
// Build Num: 090951



using Microsoft.SemanticKernel.ChatCompletion;

namespace DataIngestionLib.Models;





public sealed record ChatSessionState
{

    public int ContextTokenCount { get; init; }

    public ChatHistory ContextWindow { get; init; } = [];
    public ChatHistory History { get; init; } = [];
}