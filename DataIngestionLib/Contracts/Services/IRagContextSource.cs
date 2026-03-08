// 2026/03/07
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         IRagContextSource.cs
//   Author: Kyle L. Crowder



using DataIngestionLib.Models;

using Microsoft.Agents.AI;




namespace DataIngestionLib.Contracts.Services;





public interface IRagContextSource
{
    ValueTask<ChatHistory> GetContextMessagesAsync(ChatHistory requestMessages, AgentSession? session, CancellationToken cancellationToken = default);
}