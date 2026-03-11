// Build Date: 2026/03/11
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IChatHistorySummarizer.cs
// Author: Kyle L. Crowder
// Build Num: 105647



using DataIngestionLib.Models;




namespace DataIngestionLib.Contracts.Services;





public interface IChatHistorySummarizer
{
    ValueTask<AIChatMessage?> SummarizeAsync(string conversationId, ChatHistory messages, CancellationToken cancellationToken = default);
}