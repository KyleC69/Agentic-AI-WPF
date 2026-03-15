// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IAgentFactory.cs
// Author: Kyle L. Crowder
// Build Num: 090937



using Microsoft.Agents.AI;




namespace DataIngestionLib.Contracts;





public interface IAgentFactory
{
    AIAgent GetCodingAssistantAgent(string agentId, string model, string agentDescription = "", string? instructions = null);
}