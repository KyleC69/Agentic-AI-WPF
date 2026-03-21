// Build Date: 2026/03/19
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IAppSettings.cs
// Author: Kyle L. Crowder
// Build Num: 044231



using DataIngestionLib.Services.Contracts;




namespace DataIngestionLib.Contracts;





public interface IAppSettings
{
    string AgentId { get; set; }

    string ApplicationId { get; set; }

    string ChatHistoryConnectionString { get; set; }

    string ChatModel { get; set; }

    string EmbeddingModel { get; set; }
    string LastConversationId { get; set; }

    string LearnBaseUrl { get; set; }

    string LogDirectory { get; set; }
    string LogName { get; set; }
    int MaximumContext { get; set; }
    int MetaBudget { get; set; }

    string OllamaHost { get; set; }

    int OllamaPort { get; set; }
    int RAGBudget { get; set; }

    string RemoteRAGConnectionString { get; set; }
    int SessionBudget { get; set; }
    int SystemBudget { get; set; }
    int ToolBudget { get; set; }
    string UserId { get; set; }


    TokenBudget GetTokenBudget();
}