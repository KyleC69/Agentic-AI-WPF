// Build Date: 2026/04/03
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IHistoryIdentityService.cs
// Author: Kyle L. Crowder
// Build Num: 095140



using DataIngestionLib.Services.Contracts;

using Microsoft.Agents.AI;




namespace DataIngestionLib.Contracts.Services;





public interface IHistoryIdentityService
{

    HistoryIdentity Current { get; }


    void ApplyToSession(AgentSession session);


    string GetAgentId();


    void Initialize(string applicationId, string agentId, string userId);
}