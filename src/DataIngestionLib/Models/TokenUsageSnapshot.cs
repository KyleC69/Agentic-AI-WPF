// Build Date: 2026/03/30
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         TokenUsageSnapshot.cs
// Author: Kyle L. Crowder
// Build Num: 233132



namespace DataIngestionLib.Models;





public sealed record TokenUsageSnapshot(int TotalTokens, int SessionTokens, int RagTokens, int ToolTokens, int SystemTokens, int InputTokens, int OutputTokens, int CachedInputTokens, int ReasoningTokens, string Source, DateTimeOffset UpdatedAtUtc, IReadOnlyDictionary<string, long> AdditionalCounts);