// 2026/03/05
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         ChatSessionOptions.cs
//   Author: Kyle L. Crowder



namespace DataIngestionLib.Options;





public sealed class ChatSessionOptions
{

    public string ChatSessionFileName { get; set; } = "ChatSession.json";
    public string ConfigurationsFolder { get; set; } = string.Empty;

    public int MaxContextTokens { get; set; } = 120000;
}