// Build Date: 2026/03/29
// Solution: File
// Project:   DataIngestionLib
// File:         IAgentFactory.cs
// Author: Kyle L. Crowder
// Build Num: 051919



using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

using DataIngestionLib.Models;




namespace DataIngestionLib.Contracts;





public interface IAgentFactory
{
    AIAgent GetCodingAssistantAgent(string agentId, string model, string agentDescription = "", string? instructions = null, Action<TokenUsageSnapshot>? tokenSnapshotSink = null);
}