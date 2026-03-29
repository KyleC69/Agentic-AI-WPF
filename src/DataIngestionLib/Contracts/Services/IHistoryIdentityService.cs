// Build Date: 2026/03/29
// Solution: File
// Project:   DataIngestionLib
// File:         IHistoryIdentityService.cs
// Author: GitHub Copilot



using DataIngestionLib.Services.Contracts;

using Microsoft.Agents.AI;



namespace DataIngestionLib.Contracts.Services;





public interface IHistoryIdentityService
{
    HistoryIdentity Current { get; }

    void Initialize(string applicationId, string agentId, string userId);

    void SetConversationId(string conversationId);

    void ApplyToSession(AgentSession session);
}