// Build Date: 2026/03/31
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IHistoryIdentityService.cs
// Author: Kyle L. Crowder
// Build Num: 232048



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