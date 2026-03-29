// Build Date: 2026/03/29
// Solution: File
// Project:   DataIngestionLib
// File:         TokenUsageSnapshot.cs
// Author: GitHub Copilot



namespace DataIngestionLib.Models;





public sealed record TokenUsageSnapshot(
    int TotalTokens,
    int SessionTokens,
    int RagTokens,
    int ToolTokens,
    int SystemTokens,
    int InputTokens,
    int OutputTokens,
    int CachedInputTokens,
    int ReasoningTokens,
    string Source,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyDictionary<string, long> AdditionalCounts);