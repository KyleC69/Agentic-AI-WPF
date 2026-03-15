// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IChatHistorySummarizer.cs
// Author: Kyle L. Crowder
// Build Num: 090936



using DataIngestionLib.Models;




namespace DataIngestionLib.Contracts.Services;





public interface IChatHistorySummarizer
{
    ValueTask<AIChatMessage?> SummarizeAsync(string conversationId, AIChatHistory messages, CancellationToken cancellationToken = default);
}