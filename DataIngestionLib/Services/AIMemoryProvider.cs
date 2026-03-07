using System.Text.Json;

using DataIngestionLib.Contracts.Services;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

using AIChatMessage = Microsoft.Extensions.AI.ChatMessage;



namespace DataIngestionLib.Services;










public sealed class AIMemoryProvider : MessageAIContextProvider
{
    private const string SessionStateKey = nameof(ChatHistoryMemoryProvider);
    private const int MaxContextMessages = 8;

    private readonly ISqlVectorStore _sqlVectorStore;

    public AIMemoryProvider(ISqlVectorStore sqlVectorStore)
    {
        ArgumentNullException.ThrowIfNull(sqlVectorStore);
        _sqlVectorStore = sqlVectorStore;
    }

    private readonly Dictionary<string, List<AIChatMessage>> _historyBySession = [];



    /// <summary>
    /// Provides a collection of chat messages for the specified invoking context.
    /// </summary>
    /// <param name="context">The invoking context containing session and other relevant information.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an enumerable collection
    /// of <see cref="Microsoft.Extensions.AI.ChatMessage"/> objects, representing the chat messages
    /// associated with the current session.
    /// </returns>
    /// <remarks>
    /// This method retrieves the most recent chat messages from the session's history, limited to a maximum
    /// number of context messages. If no messages are available for the session, an empty collection is returned.
    /// </remarks>
    protected override ValueTask<IEnumerable<AIChatMessage>> ProvideMessagesAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(context);

        string sessionKey = GetOrCreateSessionKey(context.Session);

        IEnumerable<(string Message, DateTime Timestamp)> history = _sqlVectorStore.GetChatHistory(sessionKey);
        AIChatMessage[] contextMessages = history
                .TakeLast(MaxContextMessages)
                .Select(static item => DeserializeMessage(item.Message))
                .ToArray();

        return ValueTask.FromResult<IEnumerable<AIChatMessage>>(contextMessages);
    }



    /// <summary>
    /// Stores the AI context, including request and response messages, into the session's chat history.
    /// </summary>
    /// <param name="context">
    /// The invoked context containing session information, request messages, and response messages.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    /// <remarks>
    /// This method saves the provided request and response messages into the session's history, ensuring
    /// that the total number of stored messages does not exceed the maximum allowed. If the session's
    /// history exceeds the limit, the oldest messages are removed to maintain the size constraint.
    /// </remarks>
    protected override ValueTask StoreAIContextAsync(AIContextProvider.InvokedContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(context);

        if (context.InvokeException is not null)
        {
            return ValueTask.CompletedTask;
        }

        string sessionKey = GetOrCreateSessionKey(context.Session);
        List<AIChatMessage> messagesToStore = context.RequestMessages
                .Concat(context.ResponseMessages ?? [])
                .ToList();

        if (messagesToStore.Count == 0)
        {
            return ValueTask.CompletedTask;
        }

        DateTime timestamp = DateTime.UtcNow;
        for (int index = 0; index < messagesToStore.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            AIChatMessage message = messagesToStore[index];
            _sqlVectorStore.SaveChatHistory(sessionKey, SerializeMessage(message), timestamp.AddTicks(index));
        }

        return ValueTask.CompletedTask;
    }

    private static string SerializeMessage(AIChatMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        StoredChatMessage storedMessage = new(message.Role.ToString(), message.Text ?? string.Empty);
        return JsonSerializer.Serialize(storedMessage);
    }

    private static AIChatMessage DeserializeMessage(string persistedValue)
    {
        if (string.IsNullOrWhiteSpace(persistedValue))
        {
            return new AIChatMessage(ChatRole.Assistant, string.Empty);
        }

        try
        {
            StoredChatMessage? storedMessage = JsonSerializer.Deserialize<StoredChatMessage>(persistedValue);
            return storedMessage is null
                ? new AIChatMessage(ChatRole.Assistant, persistedValue)
                : new AIChatMessage(ParseRole(storedMessage.Role), storedMessage.Text ?? string.Empty);
        }
        catch (JsonException)
        {
            return new AIChatMessage(ChatRole.Assistant, persistedValue);
        }
    }

    private static ChatRole ParseRole(string? value)
    {
        return value?.Trim().ToLowerInvariant() switch
        {
            "user" => ChatRole.User,
            "assistant" => ChatRole.Assistant,
            "system" => ChatRole.System,
            "tool" => ChatRole.Tool,
            _ => ChatRole.Assistant,
        };
    }

    private static string GetOrCreateSessionKey(AgentSession session)
    {
        if (session.StateBag.TryGetValue<string>(SessionStateKey, out string? existingSessionKey) &&
            !string.IsNullOrWhiteSpace(existingSessionKey))
        {
            return existingSessionKey;
        }

        string sessionKey = Guid.NewGuid().ToString("N");
        session.StateBag.SetValue(SessionStateKey, sessionKey);
        return sessionKey;
    }

    private sealed record StoredChatMessage(string Role, string Text);
}
