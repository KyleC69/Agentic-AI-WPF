// Build Date: 2026/03/29
// Solution: File
// Project:   DataIngestionLib
// File:         ChatHistorySessionState.cs
// Author: Kyle L. Crowder
// Build Num: 051938



using Microsoft.Agents.AI;




namespace DataIngestionLib.Services;





internal static class ChatHistorySessionState
{

    private const string AgentIdStateKey = "ChatHistoryAgentId";
    private const string ApplicationIdStateKey = "ChatHistoryApplicationId";
    private const string ConversationIdStateKey = "ChatHistoryConversationId";
    private const string UserIdStateKey = "ChatHistoryUserId";

    private static string? _startupConversationId;
    private static readonly Lock SyncRoot = new();








    private static void ApplyStartupConversationIfAvailable(AgentSession? session)
    {
        if (session is null)
        {
            return;
        }

        if (session.StateBag.TryGetValue(ConversationIdStateKey, out string? existingConversationId) && !string.IsNullOrWhiteSpace(existingConversationId))
        {
            return;
        }

        var startupConversationId = TryTakeStartupConversationId();
        if (string.IsNullOrWhiteSpace(startupConversationId))
        {
            return;
        }

        session.StateBag.SetValue(ConversationIdStateKey, startupConversationId);
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
        ApplyStartupConversationIfAvailable(session);
        return GetOrCreateValue(session, ConversationIdStateKey, static () => Guid.NewGuid().ToString("N"));
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








    public static void SetStartupConversation(string conversationId)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            return;
        }

        lock (SyncRoot)
        {
            _startupConversationId = conversationId.Trim();
        }
    }








    private static string? TryTakeStartupConversationId()
    {
        lock (SyncRoot)
        {
            if (string.IsNullOrWhiteSpace(_startupConversationId))
            {
                return null;
            }

            var startupConversationId = _startupConversationId;
            _startupConversationId = null;
            return startupConversationId;
        }
    }
}