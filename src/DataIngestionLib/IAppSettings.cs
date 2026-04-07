// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IAppSettings.cs
// Author: Kyle L. Crowder
// Build Num: 212926



namespace DataIngestionLib;





public interface IAppSettings
{
    string AgentId { get; set; }
    string ApplicationId { get; set; }
    string ChatHistoryConnectionString { get; set; }
    string ChatModel { get; set; }
    string EmbeddingModel { get; set; }
    string LearnBaseUrl { get; set; }
    string LogDirectory { get; set; }
    string LogName { get; set; }
    int MaximumContext { get; set; }
    OrchestrationMode Orchestration { get; set; }
    string RemoteRAGConnectionString { get; set; }
    string RestEndpoint { get; set; }
    bool ResumeLast { get; set; }
    string UserName { get; set; }
}