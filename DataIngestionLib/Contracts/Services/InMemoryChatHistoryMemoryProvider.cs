using System.Text.Json.Serialization;

using Microsoft.Agents.AI;

using Microsoft.Extensions.AI;

namespace DataIngestionLib.Contracts.Services;





/// <summary>
/// Provides an in-memory implementation of <see cref="ChatHistoryProvider"/> with support for message reduction.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="InMemoryChatHistoryProvider"/> stores chat messages in the <see cref="AgentSession.StateBag"/>,
/// providing fast access and manipulation capabilities integrated with session state management.
/// </para>
/// <para>
/// This <see cref="ChatHistoryProvider"/> maintains all messages in memory. For long-running conversations or high-volume scenarios, consider using
/// message reduction strategies or alternative storage implementations.
/// </para>
/// </remarks>
public sealed class InMemoryChatHistoryProvider : ChatHistoryProvider
{
    private readonly ProviderSessionState<State> _sessionState;
    private IReadOnlyList<string>? _stateKeys;







    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryChatHistoryProvider"/> class.
    /// </summary>
    /// <param name="options">
    /// Optional configuration options that control the provider's behavior, including state initialization,
    /// message reduction, and serialization settings. If <see langword="null"/>, default settings will be used.
    /// </param>
    public InMemoryChatHistoryProvider(InMemoryChatHistoryProviderOptions? options = null)
    : base(
        options?.ProvideOutputMessageFilter,
        options?.StorageInputRequestMessageFilter,
        options?.StorageInputResponseMessageFilter)
    {
        Func<AgentSession?, State> stateInitializer = options?.StateInitializer is null
            ? static _ => new State()
            : session =>
            {
                global::Microsoft.Agents.AI.InMemoryChatHistoryProvider.State state = options.StateInitializer(session);
                return new State
                {
                    Messages = state.Messages
                };
            };

        _sessionState = new ProviderSessionState<State>(
        stateInitializer,
        options?.StateKey ?? GetType().Name,
        options?.JsonSerializerOptions);

        ChatReducer = options?.ChatReducer;
        ReducerTriggerEvent = options?.ReducerTriggerEvent
        ?? InMemoryChatHistoryProviderOptions.ChatReducerTriggerEvent.BeforeMessagesRetrieval;
    }








    /// <inheritdoc />
    public override IReadOnlyList<string> StateKeys => _stateKeys ??= [_sessionState.StateKey];

    /// <summary>
    /// Gets the chat reducer used to process or reduce chat messages. If null, no reduction logic will be applied.
    /// </summary>
    public IChatReducer? ChatReducer { get; }

    /// <summary>
    /// Gets the event that triggers the reducer invocation in this provider.
    /// </summary>
    public InMemoryChatHistoryProviderOptions.ChatReducerTriggerEvent ReducerTriggerEvent { get; }








    /// <summary>
    /// Gets the chat messages stored for the specified session.
    /// </summary>
    /// <param name="session">The agent session containing the state.</param>
    /// <returns>A list of chat messages, or an empty list if no state is found.</returns>
    public List<ChatMessage> GetMessages(AgentSession? session)
    {
        return _sessionState.GetOrInitializeState(session).Messages;
    }








    /// <summary>
    /// Sets the chat messages for the specified session.
    /// </summary>
    /// <param name="session">The agent session containing the state.</param>
    /// <param name="messages">The messages to store.</param>
    /// <exception cref="ArgumentNullException"><paramref name="messages"/> is <see langword="null"/>.</exception>
    public void SetMessages(AgentSession? session, List<ChatMessage> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);

        State state = _sessionState.GetOrInitializeState(session);
        state.Messages = messages;
    }








    /// <inheritdoc />
    protected override async ValueTask<IEnumerable<ChatMessage>> ProvideChatHistoryAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        State state = _sessionState.GetOrInitializeState(context.Session);

        if (ReducerTriggerEvent is InMemoryChatHistoryProviderOptions.ChatReducerTriggerEvent.BeforeMessagesRetrieval && ChatReducer is not null)
        {
            state.Messages = (await ChatReducer.ReduceAsync(state.Messages, cancellationToken).ConfigureAwait(false)).ToList();
        }

        return state.Messages;
    }








    /// <inheritdoc />
    protected override async ValueTask StoreChatHistoryAsync(InvokedContext context, CancellationToken cancellationToken = default)
    {
        State state = _sessionState.GetOrInitializeState(context.Session);

        // Add request and response messages to the provider
        IEnumerable<ChatMessage> allNewMessages = context.RequestMessages.Concat(context.ResponseMessages ?? []);
        state.Messages.AddRange(allNewMessages);

        if (ReducerTriggerEvent is InMemoryChatHistoryProviderOptions.ChatReducerTriggerEvent.AfterMessageAdded && ChatReducer is not null)
        {
            state.Messages = (await ChatReducer.ReduceAsync(state.Messages, cancellationToken).ConfigureAwait(false)).ToList();
        }
    }








    /// <summary>
    /// Represents the state of a <see cref="InMemoryChatHistoryProvider"/> stored in the <see cref="AgentSession.StateBag"/>.
    /// </summary>
    public sealed class State
    {
        /// <summary>
        /// Gets or sets the list of chat messages.
        /// </summary>
        [JsonPropertyName("messages")]
        public List<ChatMessage> Messages { get; set; } = [];
    }
}