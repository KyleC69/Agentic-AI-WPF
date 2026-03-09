// 2026/03/08
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         AIContextHistoryInjector2.cs
//   Author: Kyle L. Crowder



using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;

using Microsoft.Agents.AI;




namespace DataIngestionLib.Services.ContextInjectors;




/// <summary>
///     An <see cref="Microsoft.Agents.AI.MessageAIContextProvider" /> that injects and persists chat
///     history into the agent's invocation pipeline using an <see cref="IChatHistoryMemoryProvider" />.
/// </summary>
/// <remarks>
///     <para>
///         On each <em>invoking</em> turn this provider retrieves the relevant historical messages for
///         the current conversation and prepends them to the request so the agent has access to prior
///         context.
///     </para>
///     <para>
///         On each <em>invoked</em> turn (after the agent has responded) the request and response
///         messages are persisted back to the history store, scoped by the identifiers resolved from
///         the active <see cref="AgentSession" /> and the injected dependencies.
///     </para>
/// </remarks>
public sealed class AIContextHistoryInjector2 : MessageAIContextProvider
{
    private readonly string _applicationId;
    private readonly IAgentIdentityProvider _agentIdentityProvider;
    private readonly IChatHistoryMemoryProvider _chatHistoryMemoryProvider;




    /// <summary>
    ///     Initializes a new instance of <see cref="AIContextHistoryInjector2" />.
    /// </summary>
    /// <param name="chatHistoryMemoryProvider">
    ///     The provider responsible for reading and writing chat history.
    /// </param>
    /// <param name="accessor">
    ///     Supplies the application-level runtime context (e.g., application ID and user identity)
    ///     used to scope persisted messages.
    /// </param>
    /// <param name="agentIdentityProvider">
    ///     Resolves the identifier of the active agent so that stored messages are correctly attributed.
    /// </param>
    public AIContextHistoryInjector2(
            IChatHistoryMemoryProvider chatHistoryMemoryProvider,
            IRuntimeContextAccessor accessor,
            IAgentIdentityProvider agentIdentityProvider)
    {
        ArgumentNullException.ThrowIfNull(chatHistoryMemoryProvider);
        ArgumentNullException.ThrowIfNull(accessor);
        ArgumentNullException.ThrowIfNull(agentIdentityProvider);

        _chatHistoryMemoryProvider = chatHistoryMemoryProvider;
        _agentIdentityProvider = agentIdentityProvider;
        _applicationId = accessor.GetCurrent().ApplicationId.ToString();
    }




    /// <summary>
    ///     Provides a collection of chat messages for the specified invoking context.
    /// </summary>
    /// <param name="context">The invoking context containing session and other relevant information.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an enumerable
    ///     collection of <see cref="Microsoft.Extensions.AI.ChatMessage" /> objects representing the
    ///     historical chat messages associated with the current session.
    /// </returns>
    /// <remarks>
    ///     This method retrieves the most recent chat messages from the session's history, limited to a
    ///     maximum number of context messages. If no messages are available for the session, an empty
    ///     collection is returned.
    /// </remarks>
    protected override async ValueTask<IEnumerable<Microsoft.Extensions.AI.ChatMessage>> ProvideMessagesAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(context);

        string conversationId = ChatHistorySessionState.GetOrCreateConversationId(context.Session);
        ChatHistory requestMessages = ToChatHistory(context.RequestMessages);
        IEnumerable<AIChatMessage> historyMessages = await _chatHistoryMemoryProvider
                .BuildContextMessagesAsync(conversationId, requestMessages, cancellationToken)
                .ConfigureAwait(false);

        return ToFrameworkChatMessages(historyMessages);
    }




    /// <summary>
    ///     Stores the AI context, including request and response messages, into the session's chat
    ///     history.
    /// </summary>
    /// <param name="context">
    ///     The invoked context containing session information, request messages, and response messages.
    /// </param>
    /// <param name="cancellationToken">
    ///     A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation.
    /// </returns>
    /// <remarks>
    ///     This method saves the provided request and response messages into the session's history,
    ///     ensuring that the total number of stored messages does not exceed the maximum allowed. If
    ///     the session's history exceeds the limit, the oldest messages are removed to maintain the
    ///     size constraint.
    /// </remarks>
    protected override ValueTask StoreAIContextAsync(InvokedContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(context);

        if (context.InvokeException is not null)
        {
            return ValueTask.CompletedTask;
        }

        string conversationId = ChatHistorySessionState.GetOrCreateConversationId(context.Session);
        string sessionId = ChatHistorySessionState.GetOrCreateSessionId(context.Session);
        string agentId = ChatHistorySessionState.GetOrCreateAgentId(context.Session, _agentIdentityProvider.GetAgentId());
        string userId = ChatHistorySessionState.GetOrCreateUserId(context.Session);
        string applicationId = ChatHistorySessionState.GetOrCreateApplicationId(context.Session, _applicationId);

        ChatHistory requestMessages = ToChatHistory(context.RequestMessages);
        ChatHistory responseMessages = ToChatHistory(context.ResponseMessages);

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




    private static ChatHistory ToChatHistory(IEnumerable<Microsoft.Extensions.AI.ChatMessage> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);

        ChatHistory chatHistory = [];
        foreach (Microsoft.Extensions.AI.ChatMessage message in messages)
        {
            chatHistory.Add(new AIChatMessage(message.Role, message.Text));
        }

        return chatHistory;
    }




    private static IEnumerable<Microsoft.Extensions.AI.ChatMessage> ToFrameworkChatMessages(IEnumerable<AIChatMessage> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);

        foreach (AIChatMessage message in messages)
        {
            yield return new Microsoft.Extensions.AI.ChatMessage(message.Role, message.Text);
        }
    }
}
