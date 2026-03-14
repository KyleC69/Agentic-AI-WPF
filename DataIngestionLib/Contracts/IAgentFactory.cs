// Build Date: 2026/03/13
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IAgentFactory.cs
// Author: Kyle L. Crowder
// Build Num: 175051



using Microsoft.Agents.AI;




namespace DataIngestionLib.Contracts;





public interface IAgentFactory
{
    AIAgent GetCodingAssistantAgent();
}