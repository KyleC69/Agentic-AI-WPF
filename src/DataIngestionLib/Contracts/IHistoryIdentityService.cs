// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IHistoryIdentityService.cs
// Author: Kyle L. Crowder
// Build Num: 212853



using DataIngestionLib.Services;

using Microsoft.Agents.AI;




namespace DataIngestionLib.Contracts;





public interface IHistoryIdentityService
{

    HistoryIdentity Current { get; }


    void ApplyToSession(AgentSession session);


    string GetAgentId();


    void Initialize(string applicationId, string agentId, string userId);
}