// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         AppSettings.cs
// Author: Kyle L. Crowder
// Build Num: 212926



namespace DataIngestionLib;





public class AppSettings : IAppSettings
{

    public string AgentId { get; set; } = "Agentic-Max";

    public string ApplicationId { get; set; } = "15A53D0F-041D-44DD-A150-DFB8D0F133FF";

    public string ChatHistoryConnectionString { get; set; } = "CHAT_HISTORY";

    public string ChatModel { get; set; } = "gpt-oss:cloud";

    public string EmbeddingModel { get; set; } = "mxbai-embed-large:latest";

    public string LearnBaseUrl { get; set; } = "https://learn.microsoft.com/en-us/agent-framework";

    public string LogDirectory { get; set; } = "\\logs";

    public string LogName { get; set; } = "AgenticLogs.log";

    public int MaximumContext { get; set; } = 130000;
    public OrchestrationMode Orchestration { get; set; }

    public string RemoteRAGConnectionString { get; set; } = "REMOTE_RAG";

    public string RestEndpoint { get; set; } = "http://127.0.0.1:11434";

    public bool ResumeLast { get; set; } = true;

    public string UserName { get; set; } = "TommyCat";
}





//Agentic orchestration modes
public enum OrchestrationMode
{
    None, Concurrent, Sequential, RoundRobin
}