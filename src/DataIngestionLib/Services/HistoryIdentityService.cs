// Build Date: 2026/03/30
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         HistoryIdentityService.cs
// Author: Kyle L. Crowder
// Build Num: 233139



using CommunityToolkit.Diagnostics;

using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Services.Contracts;

using Microsoft.Agents.AI;




namespace DataIngestionLib.Services;





/// <summary>
///     For the centralized management of the ConversationIdentity across the conversation.
/// </summary>
public sealed class HistoryIdentityService : IHistoryIdentityService, IAgentIdentityProvider
{
    private readonly HistoryIdentity _current = new();
    private readonly object _syncLock = new();








    public string GetAgentId()
    {
        HistoryIdentity snapshot = Current;
        return string.IsNullOrWhiteSpace(snapshot.AgentId) ? "unknown-agent" : snapshot.AgentId;
    }








    public HistoryIdentity Current
    {
        get
        {
            lock (_syncLock)
            {
                return new HistoryIdentity { AgentId = _current.AgentId, ApplicationId = _current.ApplicationId, ConversationId = _current.ConversationId, UserId = _current.UserId };
            }
        }
    }








    public void Initialize(string applicationId, string agentId, string userId)
    {
        Guard.IsNotNullOrEmpty(applicationId);
        Guard.IsNotNullOrEmpty(agentId);

        var normalizedUserId = string.IsNullOrWhiteSpace(userId) ? Environment.UserName : userId.Trim();
        Guard.IsNotNullOrEmpty(normalizedUserId, nameof(userId));
        lock (_syncLock)
        {
            _current.ApplicationId = applicationId.Trim();
            _current.AgentId = agentId.Trim();
            _current.UserId = normalizedUserId;
        }
    }








    public void SetConversationId(string conversationId)
    {
        Guard.IsNotNullOrEmpty(conversationId);

        lock (_syncLock)
        {
            _current.ConversationId = conversationId.Trim();
        }
    }








    public void ApplyToSession(AgentSession session)
    {
        Guard.IsNotNull(session);

        HistoryIdentity snapshot = Current;

        session.StateBag.SetValue("ApplicationId", snapshot.ApplicationId);
        session.StateBag.SetValue("AgentId", snapshot.AgentId);
        session.StateBag.SetValue("UserId", snapshot.UserId);
        session.StateBag.SetValue("UserName", snapshot.UserId);

        if (!string.IsNullOrWhiteSpace(snapshot.ConversationId))
        {
            session.StateBag.SetValue("ConversationId", snapshot.ConversationId);
        }
    }
}