// 2026/03/07
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         IAgentFactory.cs
//   Author: Kyle L. Crowder



using Microsoft.Agents.AI;




namespace DataIngestionLib.Contracts;





public interface IAgentFactory
{
    AIAgent GetCodingAssistantAgent();
}