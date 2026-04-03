// Build Date: 2026/03/31
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ChatHistoryContextInjector.cs
// Author: Kyle L. Crowder
// Build Num: 232058



using System.Diagnostics;
using System.Text.Json;

using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Data;
using DataIngestionLib.Services;
using DataIngestionLib.Services.Contracts;

using Microsoft.Agents.AI;
using Microsoft.Agents.Core.Serialization;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;




namespace DataIngestionLib.Providers;





public sealed class ChatHistoryContextInjector : AIContextProvider
{
    private readonly AIChatHistoryDb _dbcontext;
    private readonly IHistoryIdentityService _historyIdentityService;
    private readonly ILogger<ChatHistoryContextInjector> _logger;
    private readonly ProviderSessionState<HistoryIdentity> _sessionStateHelper;

    private static readonly HashSet<AgentRequestMessageSourceType> IgnoredRequestSourceTypes =
    [
            AgentRequestMessageSourceType.AIContextProvider
    ];

    private static readonly Func<IEnumerable<ChatMessage>, IEnumerable<ChatMessage>> providerInputFilter = msgs => msgs.Where(msg => msg.Text != string.Empty);
    private static readonly Func<IEnumerable<ChatMessage>, IEnumerable<ChatMessage>> storeInputRequestFilter = msgs => msgs.Where(msg => msg.Text != string.Empty);
    private static readonly Func<IEnumerable<ChatMessage>, IEnumerable<ChatMessage>> storeInputResponseFilter = msgs => msgs.Where(msg => msg.Text != string.Empty);








    public ChatHistoryContextInjector(ILogger<ChatHistoryContextInjector> logger, HistoryIdentityService historyIdentityService) : base(providerInputFilter, storeInputRequestFilter, storeInputResponseFilter)
    {
        _logger = logger;
        _historyIdentityService = historyIdentityService;
        _dbcontext = new AIChatHistoryDb();

        // Database keys are stored in the state bag of the session for easy access by the providers and context injectors,
        _sessionStateHelper = new ProviderSessionState<HistoryIdentity>(currentSession => _historyIdentityService.Current, this.GetType().Name);



    }








    /// <summary>
    ///     Asynchronously handles the core logic for an AI operation invocation.
    /// </summary>
    /// <param name="context">
    ///     The <see cref="InvokedContext" /> containing details about the AI operation being invoked.
    /// </param>
    /// <param name="cancellationToken">
    ///     A <see cref="CancellationToken" /> that can be used to cancel the operation.
    /// </param>
    /// <returns>
    ///     A <see cref="ValueTask" /> representing the asynchronous operation.
    /// </returns>
    /// <remarks>
    ///     This method is responsible for processing the invocation context and performing any necessary actions
    ///     before delegating to the base implementation.
    /// </remarks>
    protected override ValueTask InvokedCoreAsync(InvokedContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        ChatMessage? lastRequest = context.RequestMessages?.LastOrDefault();
        var messageId = lastRequest is null ? string.Empty : lastRequest.GetAgentRequestMessageSourceId() ?? string.Empty;
        var conversationId = context.Session?.StateBag?.GetValue<string>("ConversationId") ?? string.Empty;

        _logger.LogTrace("Call from InvokedCore in ChatHistoryContextInjector: MessageID {MessageId} ConversationID {ConversationId}", messageId, conversationId);

        return base.InvokedCoreAsync(context, cancellationToken);
    }








    /// <summary>
    ///     Asynchronously prepares and provides the AI context for an AI operation before it is invoked.
    /// </summary>
    /// <param name="context">
    ///     The <see cref="InvokingContext" /> containing details about the AI operation being prepared.
    /// </param>
    /// <param name="cancellationToken">
    ///     A <see cref="CancellationToken" /> that can be used to cancel the operation.
    /// </param>
    /// <returns>
    ///     A <see cref="ValueTask{TResult}" /> representing the asynchronous operation, with the result being the prepared
    ///     <see cref="AIContext" />.
    /// </returns>
    /// <remarks>
    ///     This method is responsible for initializing and returning the AI context required for the AI operation.
    /// </remarks>
    protected override ValueTask<AIContext> InvokingCoreAsync(InvokingContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        var messageId = context.AIContext?.Messages?.LastOrDefault()?.MessageId ?? string.Empty;
        var conversationId = context.Session?.StateBag?.GetValue<string>("ConversationId") ?? string.Empty;

        _logger.LogTrace("Call from InvokingCoreAsync in ChatHistoryContextInjector: MessageID {MessageId} ConversationID {ConversationId}", messageId, conversationId);

        if (context.AIContext?.Messages != null)
        {
            //      context.AIContext.Messages.Append(new ChatMessage(ChatRole.User, "The following is the conversation history from the previous session, which may be relevant to the current conversation."));
        }

        return base.InvokingCoreAsync(context, cancellationToken);
    }








    /// <summary>
    ///     Asynchronously provides the AI context for the current invocation of an AI operation.
    /// </summary>
    /// <param name="context">
    ///     The <see cref="InvokingContext" /> containing details about the AI operation being invoked.
    /// </param>
    /// <param name="cancellationToken">
    ///     A <see cref="CancellationToken" /> that can be used to cancel the operation.
    /// </param>
    /// <returns>
    ///     A <see cref="ValueTask{TResult}" /> representing the asynchronous operation, with the result being the provided
    ///     <see cref="AIContext" />.
    /// </returns>
    /// <remarks>
    ///     This method is responsible for preparing and returning the AI context required for the AI operation.
    /// </remarks>
    protected override ValueTask<AIContext> ProvideAIContextAsync(InvokingContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        var messageId = context.Session?.StateBag?.GetValue<string>("MessageId") ?? string.Empty;
        var conversationId = context.Session?.StateBag?.GetValue<string>("ConversationId") ?? string.Empty;

        _logger.LogTrace("Call from ProvideAIContextAsync in ChatHistoryContextInjector: MessageID {MessageId} ConversationID {ConversationId}", messageId, conversationId);

        return base.ProvideAIContextAsync(context, cancellationToken);
    }








    /// <summary>
    ///     Asynchronously stores the AI context after the invocation of an AI operation.
    /// </summary>
    /// <param name="context">
    ///     The <see cref="InvokedContext" /> containing details about the AI operation that was invoked.
    /// </param>
    /// <param name="cancellationToken">
    ///     A <see cref="CancellationToken" /> that can be used to cancel the operation.
    /// </param>
    /// <returns>
    ///     A <see cref="ValueTask" /> representing the asynchronous operation.
    /// </returns>
    /// <remarks>
    ///     This method is invoked to persist any relevant AI context after the operation has been processed.
    /// </remarks>
    protected override ValueTask StoreAIContextAsync(InvokedContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        _ = context.Session?.StateBag?.GetValue<string>("MessageId") ?? string.Empty;
        _ = context.Session?.StateBag?.GetValue<string>("ConversationId") ?? string.Empty;

        try
        {
            HistoryIdentity state = _sessionStateHelper.GetOrInitializeState(context.Session);
            var cm = GetContextMessages(context);

            //Need to filter out bad tool results and empty messages before saving to the session state and database,
            //but we want to keep the full set of messages in the session state for any providers that run after this one in the pipeline and may need access to the unfiltered messages.
            var filtered = FilterMessages(cm);

            state.Messages.AddRange(filtered);
            _sessionStateHelper.SaveState(context.Session, state);
            _logger.LogTrace("Beginning to save chat messages for conversation {0}", state.ConversationId);
            //    await this.PersistInteractionAsync(state, filtered, cancellationToken);

        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unable to persist chat history to SQL store..");
        }

        return base.StoreAIContextAsync(context, cancellationToken);
    }








    private static IEnumerable<ChatMessage> FilterMessages(IEnumerable<ChatMessage> allMessages)
    {


        //REmoves messages that are tagged with ignored source types or that have empty/whitespace content,
        //as these are not useful to keep in the chat history and can cause issues with some LLM providers if included in the prompt.
        var clean = allMessages.Where(message => !IgnoredRequestSourceTypes.Contains(message.GetAgentRequestMessageSourceType())).Where(message => !string.IsNullOrWhiteSpace(message.Text)).ToArray();
        IEnumerable<ChatMessage> good = clean.Where(msg => !IsErroredToolResult(msg)).ToList();
        return good;
    }








    private List<ChatMessage> GetContextMessages(InvokedContext context)
    {
        List<ChatMessage> msgs = [];
        Debug.Assert(context.ResponseMessages != null);
        foreach (ChatMessage m in context.ResponseMessages) msgs.Add(m);

        foreach (ChatMessage m in context.RequestMessages) msgs.Add(m);

        return msgs;
    }








    /// <summary>
    ///     Determines whether the specified chat message represents an errored tool result.
    /// </summary>
    /// <param name="msg">The chat message to evaluate.</param>
    /// <returns>
    ///     <c>true</c> if the chat message is from a tool and contains an error; otherwise, <c>false</c>.
    /// </returns>
    private static bool IsErroredToolResult(ChatMessage msg)
    {
        if (msg.Role != ChatRole.Tool)
        {
            return false;
        }

        _ = msg.ToJsonElements();

        foreach (AIContent content in msg.Contents)
            if (content is FunctionResultContent rc)
            {
                if (rc.Result is not null && rc.Result is JsonElement resultElement && resultElement.TryGetProperty("error", out _))
                {
                    return true;
                }
            }

        return false;
    }
}