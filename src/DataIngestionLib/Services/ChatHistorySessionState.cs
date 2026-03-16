// Build Date: 2026/03/16
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ChatHistorySessionState.cs
// Author: Kyle L. Crowder
// Build Num: 051927



using Microsoft.Agents.AI;




namespace DataIngestionLib.Services;





internal static class ChatHistorySessionState
{

    private const string AgentIdStateKey = "ChatHistoryAgentId";
    private const string ApplicationIdStateKey = "ChatHistoryApplicationId";
    private const string ConversationIdStateKey = "ChatHistoryConversationId";
    private const string SessionIdStateKey = "ChatHistorySessionId";
    private const string UserIdStateKey = "ChatHistoryUserId";

    private static string? _startupConversationId;
    private static string? _startupSessionId;
    private static readonly Lock SyncRoot = new();








    private static void ApplyStartupSessionIfAvailable(AgentSession? session)
    {
        if (session is null)
        {
            return;
        }

        if (session.StateBag.TryGetValue(SessionIdStateKey, out string? existingSessionId)
            && !string.IsNullOrWhiteSpace(existingSessionId))
        {
            return;
        }

        if (session.StateBag.TryGetValue(ConversationIdStateKey, out string? existingConversationId)
            && !string.IsNullOrWhiteSpace(existingConversationId))
        {
            return;
        }

        var startupSession = TryTakeStartupSession();
        if (startupSession is null)
        {
            return;
        }

        session.StateBag.SetValue(SessionIdStateKey, startupSession.Value.SessionId);
        session.StateBag.SetValue(ConversationIdStateKey, startupSession.Value.ConversationId);
    }








    public static string GetOrCreateAgentId(AgentSession? session, string fallbackAgentId)
    {
        return GetOrCreateValue(session, AgentIdStateKey, () => fallbackAgentId);
    }








    public static string GetOrCreateApplicationId(AgentSession? session, string fallbackApplicationId)
    {
        return GetOrCreateValue(session, ApplicationIdStateKey, () => fallbackApplicationId);
    }








    public static string GetOrCreateConversationId(AgentSession? session)
    {
        ApplyStartupSessionIfAvailable(session);
        return GetOrCreateValue(session, ConversationIdStateKey, static () => Guid.NewGuid().ToString("N"));
    }








    public static string GetOrCreateSessionId(AgentSession? session)
    {
        ApplyStartupSessionIfAvailable(session);
        return GetOrCreateValue(session, SessionIdStateKey, static () => Guid.NewGuid().ToString("N"));
    }








    public static string GetOrCreateUserId(AgentSession? session)
    {
        return GetOrCreateUserId(session, Environment.UserName);
    }








    public static string GetOrCreateUserId(AgentSession? session, string fallbackUserId)
    {
        return GetOrCreateValue(session, UserIdStateKey, () => fallbackUserId);
    }








    private static string GetOrCreateValue(AgentSession? session, string key, Func<string> factory)
    {
        if (session is null)
        {
            var value = factory();
            return string.IsNullOrWhiteSpace(value) ? "unknown" : value;
        }

        if (session.StateBag.TryGetValue(key, out string? existingValue) && !string.IsNullOrWhiteSpace(existingValue))
        {
            return existingValue;
        }

        var newValue = factory();
        if (string.IsNullOrWhiteSpace(newValue))
        {
            newValue = "unknown";
        }

        session.StateBag.SetValue(key, newValue);
        return newValue;
    }








    public static void SetStartupSession(string sessionId, string conversationId)
    {
        if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(conversationId))
        {
            return;
        }

        lock (SyncRoot)
        {
            _startupSessionId = sessionId.Trim();
            _startupConversationId = conversationId.Trim();
        }
    }








    private static (string SessionId, string ConversationId)? TryTakeStartupSession()
    {
        lock (SyncRoot)
        {
            if (string.IsNullOrWhiteSpace(_startupSessionId) || string.IsNullOrWhiteSpace(_startupConversationId))
            {
                return null;
            }

            (string SessionId, string ConversationId) startupSession = (_startupSessionId, _startupConversationId);
            _startupSessionId = null;
            _startupConversationId = null;
            return startupSession;
        }
    }
}