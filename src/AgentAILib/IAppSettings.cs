// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         IAppSettings.cs
// Author: Kyle L. Crowder
// Build Num: 194525



namespace AgentAILib;





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