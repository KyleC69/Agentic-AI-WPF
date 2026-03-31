// Build Date: 2026/03/30
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IAgentFactory.cs
// Author: Kyle L. Crowder
// Build Num: 233121



using DataIngestionLib.Models;

using Microsoft.Agents.AI;




namespace DataIngestionLib.Contracts;





public interface IAgentFactory
{
    AIAgent GetCodingAssistantAgent(string agentId, string model, string agentDescription = "", string? instructions = null, Action<TokenUsageSnapshot>? tokenSnapshotSink = null);
}