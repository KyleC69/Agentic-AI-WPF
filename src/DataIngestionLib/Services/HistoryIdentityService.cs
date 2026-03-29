// Build Date: 2026/03/29
// Solution: File
// Project:   DataIngestionLib
// File:         HistoryIdentityService.cs
// Author: Kyle L. Crowder
// Build Num: 052959



using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Services.Contracts;

using Microsoft.Agents.AI;

namespace DataIngestionLib.Services;




/// <summary>
/// For the centralized management of the ConversationIdentity across the conversation.
/// </summary>
public sealed class HistoryIdentityService : IHistoryIdentityService, IAgentIdentityProvider
{
    private readonly object _syncLock = new();
    private readonly HistoryIdentity _current = new();






    public HistoryIdentity Current
    {
        get
        {
            lock (_syncLock)
            {
                return new HistoryIdentity
                {
                        AgentId = _current.AgentId,
                        ApplicationId = _current.ApplicationId,
                        ConversationId = _current.ConversationId,
                        MessageId = _current.MessageId,
                        UserId = _current.UserId
                };
            }
        }
    }






    public void Initialize(string applicationId, string agentId, string userId)
    {
        if (string.IsNullOrWhiteSpace(applicationId))
        {
            throw new ArgumentException("Application identity cannot be empty.", nameof(applicationId));
        }

        if (string.IsNullOrWhiteSpace(agentId))
        {
            throw new ArgumentException("Agent identity cannot be empty.", nameof(agentId));
        }

        var normalizedUserId = string.IsNullOrWhiteSpace(userId) ? Environment.UserName : userId.Trim();
        if (string.IsNullOrWhiteSpace(normalizedUserId))
        {
            throw new ArgumentException("User identity cannot be empty.", nameof(userId));
        }

        lock (_syncLock)
        {
            _current.ApplicationId = applicationId.Trim();
            _current.AgentId = agentId.Trim();
            _current.UserId = normalizedUserId;
        }
    }






    public void SetConversationId(string conversationId)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            throw new ArgumentException("Conversation identity cannot be empty.", nameof(conversationId));
        }

        lock (_syncLock)
        {
            _current.ConversationId = conversationId.Trim();
        }
    }






    public void ApplyToSession(AgentSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

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






    public string GetAgentId()
    {
        HistoryIdentity snapshot = Current;
        return string.IsNullOrWhiteSpace(snapshot.AgentId) ? "unknown-agent" : snapshot.AgentId;
    }





}

