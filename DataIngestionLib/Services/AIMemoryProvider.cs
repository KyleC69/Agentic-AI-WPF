using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;

using Microsoft.Agents.AI;

using BaseMessageAIContextProvider = Microsoft.Agents.AI.MessageAIContextProvider;




namespace DataIngestionLib.Services;










public sealed class AIHistoryProvider : BaseMessageAIContextProvider
{
    //We need get the actual Agent id. Where can we get it from???
    private string DefaultAgentId = "default-agent";

    private readonly IChatHistoryMemoryProvider _chatHistoryMemoryProvider;
    private readonly string _applicationId;
    private readonly IRuntimeContextAccessor _runtimeAccessor;








    /// <summary>
    /// 
    /// </summary>
    /// <param name="chatHistoryMemoryProvider"></param>
    /// <param name="applicationId"></param>
    public AIHistoryProvider(IChatHistoryMemoryProvider chatHistoryMemoryProvider, IRuntimeContextAccessor accessor)
    {
        ArgumentNullException.ThrowIfNull(chatHistoryMemoryProvider);

        _chatHistoryMemoryProvider = chatHistoryMemoryProvider;
        _runtimeAccessor = accessor;
        _applicationId = accessor.GetCurrent().ApplicationId.ToString();
    }



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
    protected override async ValueTask<IEnumerable<Microsoft.Extensions.AI.ChatMessage>> ProvideMessagesAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(context);

        string conversationId = ChatHistorySessionState.GetOrCreateConversationId(context.Session);
        ChatHistory requestMessages = [];
        foreach (Microsoft.Extensions.AI.ChatMessage m in context.RequestMessages)
        {
            requestMessages.Add(m.Role, m.Text);

        }

        return (IEnumerable<Microsoft.Extensions.AI.ChatMessage>)await _chatHistoryMemoryProvider.BuildContextMessagesAsync(conversationId, requestMessages, cancellationToken).ConfigureAwait(false);
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

        string conversationId = ChatHistorySessionState.GetOrCreateConversationId(context.Session);
        string sessionId = ChatHistorySessionState.GetOrCreateSessionId(context.Session);
        string agentId = ChatHistorySessionState.GetOrCreateAgentId(context.Session, DefaultAgentId);
        string userId = ChatHistorySessionState.GetOrCreateUserId(context.Session);
        string applicationId = ChatHistorySessionState.GetOrCreateApplicationId(context.Session, _applicationId);

        ChatHistory requestMessages = [.. context.RequestMessages.Cast<AIChatMessage>()];
        ChatHistory responseMessages = [.. context.ResponseMessages.Cast<AIChatMessage>()];

        return _chatHistoryMemoryProvider.StoreMessagesAsync(
            conversationId,
            sessionId,
            agentId,
            userId,
            applicationId,
            requestMessages,
            responseMessages,
            cancellationToken);
    }
}
