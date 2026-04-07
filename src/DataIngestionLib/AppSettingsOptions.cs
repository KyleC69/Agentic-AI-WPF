// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//
//
//
//

namespace DataIngestionLib;




public class AppSettings : IAppSettings
{

    public string OllamaHost { get; set; } = "http://127.0.0.1";

    public int OllamaPort { get; set; } = 11434;

    public string EmbeddingModel { get; set; } = "mxbai-embed-large:latest";


    public string LogDirectory { get; set; } = "\\logs";

    public string LogName { get; set; } = "AgenticLogs.log";

    public int MaximumContext { get; set; } = 130000;

    public string AgentId { get; set; } = "Agentic-Max";

    public string ChatHistoryConnectionString { get; set; } = "CHAT_HISTORY";

    public string RemoteRAGConnectionString { get; set; } = "REMOTE_RAG";

    public bool ResumeLast { get; set; } = true;

    public string ChatModel { get; set; } = "gpt-oss:cloud";

    public string LearnBaseUrl { get; set; } = "https://learn.microsoft.com/en-us/agent-framework";

    public string ApplicationId { get; set; } = "15A53D0F-041D-44DD-A150-DFB8D0F133FF";

    public string UserName { get; set; } = "TommyCat";
    public OrchestrationMode Orchestration { get; set; }

}




//Agentic orchestration modes
public enum OrchestrationMode
{
    None,
    Concurrent,
    Sequential,
    RoundRobin
}

