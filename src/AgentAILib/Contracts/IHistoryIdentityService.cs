// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         IHistoryIdentityService.cs
// Author: Kyle L. Crowder
// Build Num: 194446



using AgentAILib.Services;

using Microsoft.Agents.AI;




namespace AgentAILib.Contracts;





public interface IHistoryIdentityService
{

    HistoryIdentity Current { get; }


    void ApplyToSession(AgentSession session);


    string GetAgentId();


    void Initialize(string applicationId, string agentId, string userId);
}