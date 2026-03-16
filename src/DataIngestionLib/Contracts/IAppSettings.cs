// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IAppSettings.cs
// Author: Kyle L. Crowder
// Build Num: 155936



using DataIngestionLib.Services.Contracts;




namespace DataIngestionLib.Contracts;





public interface IAppSettings
{
    string ApplicationId { get; set; }


    string ChatHistoryConnectionString { get; }


    string ChatModel { get; }


    string EmbeddingModel { get; }


    string LearnBaseUrl { get; }


    string LogDirectory { get; }
    int MaximumContext { get; }
    int MetaBudget { get; }

    string OllamaHost { get; }


    int OllamaPort { get; }
    int RAGBudget { get; }

    string RemoteRAGConnectionString { get; }
    int SessionBudget { get; }
    int SystemBudget { get; }
    int ToolBudget { get; }


    TokenBudget GetTokenBudget();
}