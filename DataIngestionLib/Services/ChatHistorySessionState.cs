using Microsoft.Agents.AI;

namespace DataIngestionLib.Services;

internal static class ChatHistorySessionState
{
    private const string ConversationIdStateKey = "ChatHistoryConversationId";
    private const string SessionIdStateKey = "ChatHistorySessionId";
    private const string AgentIdStateKey = "ChatHistoryAgentId";
    private const string UserIdStateKey = "ChatHistoryUserId";
    private const string ApplicationIdStateKey = "ChatHistoryApplicationId";

    public static string GetOrCreateConversationId(AgentSession? session)
    {
        return GetOrCreateValue(session, ConversationIdStateKey, static () => Guid.NewGuid().ToString("N"));
    }

    public static string GetOrCreateSessionId(AgentSession? session)
    {
        return GetOrCreateValue(session, SessionIdStateKey, static () => Guid.NewGuid().ToString("N"));
    }

    public static string GetOrCreateAgentId(AgentSession? session, string fallbackAgentId)
    {
        return GetOrCreateValue(session, AgentIdStateKey, () => fallbackAgentId);
    }

    public static string GetOrCreateUserId(AgentSession? session)
    {
        return GetOrCreateValue(session, UserIdStateKey, static () => Environment.UserName);
    }

    public static string GetOrCreateApplicationId(AgentSession? session, string fallbackApplicationId)
    {
        return GetOrCreateValue(session, ApplicationIdStateKey, () => fallbackApplicationId);
    }

    private static string GetOrCreateValue(AgentSession? session, string key, Func<string> factory)
    {
        if (session is null)
        {
            string value = factory();
            return string.IsNullOrWhiteSpace(value) ? "unknown" : value;
        }

        if (session.StateBag.TryGetValue<string>(key, out string? existingValue) && !string.IsNullOrWhiteSpace(existingValue))
        {
            return existingValue;
        }

        string newValue = factory();
        if (string.IsNullOrWhiteSpace(newValue))
        {
            newValue = "unknown";
        }

        session.StateBag.SetValue(key, newValue);
        return newValue;
    }
}
