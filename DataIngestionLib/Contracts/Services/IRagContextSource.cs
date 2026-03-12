// Build Date: 2026/03/11
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IRagContextSource.cs
// Author: Kyle L. Crowder
// Build Num: 105647



using DataIngestionLib.Models;

using Microsoft.Agents.AI;




namespace DataIngestionLib.Contracts.Services;





public interface IRagContextSource
{
    ValueTask<AIChatHistory> GetContextMessagesAsync(AIChatHistory requestMessages, AgentSession? session, CancellationToken cancellationToken = default);
}