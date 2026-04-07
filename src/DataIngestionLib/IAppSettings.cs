// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IAppSettings.cs
// Author: Kyle L. Crowder
// Build Num: 211356



namespace DataIngestionLib;





public interface IAppSettings
{
    string OllamaHost { get; set; }
    int OllamaPort { get; set; }
    string EmbeddingModel { get; set; }
    string LogDirectory { get; set; }
    string LogName { get; set; }
    int MaximumContext { get; set; }
    string AgentId { get; set; }
    string ChatHistoryConnectionString { get; set; }
    string RemoteRAGConnectionString { get; set; }
    bool ResumeLast { get; set; }
    string ChatModel { get; set; }
    string LearnBaseUrl { get; set; }
    string ApplicationId { get; set; }
    string UserName { get; set; }
    OrchestrationMode Orchestration { get; set; }
}