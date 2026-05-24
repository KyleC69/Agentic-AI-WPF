// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         TokenUsageSnapshot.cs
// Author: Kyle L. Crowder
// Build Date: 2026/05/24



namespace AgentAILib.Models;





public sealed record TokenUsageSnapshot(int TotalTokens, int SessionTokens, int RagTokens, int ToolTokens, int SystemTokens, int InputTokens, int OutputTokens, int CachedInputTokens, int ReasoningTokens, string Source, DateTimeOffset UpdatedAtUtc, IReadOnlyDictionary<string, long> AdditionalCounts);