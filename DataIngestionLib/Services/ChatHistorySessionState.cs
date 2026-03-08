// 2026/03/07
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         ChatHistorySessionState.cs
//   Author: Kyle L. Crowder



using Microsoft.Agents.AI;




namespace DataIngestionLib.Services;





internal static class ChatHistorySessionState
{
    private const string AgentIdStateKey = "ChatHistoryAgentId";
    private const string ApplicationIdStateKey = "ChatHistoryApplicationId";
    private const string ConversationIdStateKey = "ChatHistoryConversationId";
    private const string SessionIdStateKey = "ChatHistorySessionId";
    private const string UserIdStateKey = "ChatHistoryUserId";








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
        return GetOrCreateValue(session, ConversationIdStateKey, static () => Guid.NewGuid().ToString("N"));
    }








    public static string GetOrCreateSessionId(AgentSession? session)
    {
        return GetOrCreateValue(session, SessionIdStateKey, static () => Guid.NewGuid().ToString("N"));
    }








    public static string GetOrCreateUserId(AgentSession? session)
    {
        return GetOrCreateValue(session, UserIdStateKey, static () => Environment.UserName);
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
}