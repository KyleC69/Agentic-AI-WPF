// 2026/03/07
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         IChatHistorySummarizer.cs
//   Author: Kyle L. Crowder



using DataIngestionLib.Models;





namespace DataIngestionLib.Contracts.Services;





public interface IChatHistorySummarizer
{
    ValueTask<AIChatMessage?> SummarizeAsync(string conversationId, ChatHistory messages, CancellationToken cancellationToken = default);
}