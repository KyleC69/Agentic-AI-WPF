// Build Date: 2026/03/30
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IHistoryIdentityService.cs
// Author: Kyle L. Crowder
// Build Num: 233123



using DataIngestionLib.Services.Contracts;

using Microsoft.Agents.AI;




namespace DataIngestionLib.Contracts.Services;





public interface IHistoryIdentityService
{
    HistoryIdentity Current { get; }


    void ApplyToSession(AgentSession session);


    void Initialize(string applicationId, string agentId, string userId);


    void SetConversationId(string conversationId);
}