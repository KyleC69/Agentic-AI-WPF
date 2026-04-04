// Build Date: 2026/04/04
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ChatConversationService.cs
// Author: GitHub Copilot


using CommunityToolkit.Diagnostics;

using DataIngestionLib.Contracts;
using DataIngestionLib.Models;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;


namespace DataIngestionLib.Services;



/// <summary>
///     Class is responsible for managing the chat conversation round with LLM, self-contained and keeps viewmodel clean.
///     Encapsulates the management of the agent operations.
/// </summary>
public sealed class ChatConversationService : ChatConversationBase, IChatConversationService
{
    private readonly IAgentFactory _agentFactory;
    private readonly IHistoryIdentityService _historyIdentityService;
    private readonly SemaphoreSlim _initializeGate = new(1, 1);
    private readonly string _initialUserId;
    private readonly ILogger<ChatConversationService> _logger;
    private readonly ISqlChatHistoryProvider _sqlChatHistoryProvider;

    private AIAgent? _agent;
    private AgentSession? _agentSession;
    private ProviderSessionState<HistoryIdentity>? _sessionStateHelper;





    public ChatConversationService(ILoggerFactory factory, IAgentFactory agentFactory, IHistoryIdentityService historyIdentityService, ISqlChatHistoryProvider sqlChatHistoryProvider)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(agentFactory);
        ArgumentNullException.ThrowIfNull(historyIdentityService);
        ArgumentNullException.ThrowIfNull(sqlChatHistoryProvider);

        _agentFactory = agentFactory;
        _historyIdentityService = historyIdentityService;
        _sqlChatHistoryProvider = sqlChatHistoryProvider;
        _initialUserId = historyIdentityService.Current.UserId;
        _logger = factory.CreateLogger<ChatConversationService>();
    }





    public bool Initialized { get; set; }

    /// <summary>
    ///     Internal tracking history of the conversation with the LLM.
    /// </summary>
    public List<ChatMessage> AIHistory { get; } = [];

    /// <inheritdoc />
    public event EventHandler<bool>? BusyStateChanged;

    /// <summary>
    ///     Current token context derived from middleware snapshots.
    /// </summary>
    public int ContextTokenCount { get; private set; }

    /// <inheritdoc />
    public string ConversationId { get; private set; } = string.Empty;





    async ValueTask<IEnumerable<ChatMessage>> IChatConversationService.LoadConversationHistoryAsync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        await this.InitializeAsync(token).ConfigureAwait(false);

        AgentSession session = GetRequiredSession();

        ConversationId = HistoryIdentityService.GetConversationId();

        IReadOnlyList<ChatMessage> historyMessages = [.. await _sqlChatHistoryProvider.GetMessagesAsync(ConversationId, token).ConfigureAwait(false) ?? []];

        //Load the state and attach the history messages to it.
        ProviderSessionState<HistoryIdentity> sessionState = GetRequiredSessionState();
        HistoryIdentity state = sessionState.GetOrInitializeState(session);
        state.Messages.AddRange(historyMessages);
        // Update the session to reflect the loaded conversation history.
        sessionState.SaveState(session, state);

        return historyMessages;
    }





    /// <inheritdoc />
    public int RagTokenCount { get; private set; }





    /// <summary>
    ///     Sends request to LLM and waits for a response.
    /// </summary>
    /// <param name="content">The user message content to answer.</param>
    /// <param name="token">The cancellation token for interrupting generation.</param>
    /// <returns>The generated assistant chat message.</returns>
    /// <exception cref="ArgumentException"></exception>
    public async ValueTask<ChatMessage> SendRequestToModelAsync(string content, CancellationToken token)
    {
        await this.InitializeAsync(token).ConfigureAwait(false);
        BusyStateChanged?.Invoke(this, true);
        try
        {
            Guard.IsNotNullOrEmpty(content);

            if (_agent is null || _agentSession is null)
            {
                throw new InvalidOperationException("Agent session is not initialized.");
            }

            AIHistory.Add(new ChatMessage(ChatRole.User, content));

            AgentResponse response = await _agent.RunAsync(content, _agentSession, null, token).ConfigureAwait(false);

            var assistantText = response.Text?.Trim() ?? string.Empty;
            ChatMessage assistantMessage = new(ChatRole.Assistant, assistantText);
            AIHistory.Add(assistantMessage);

            return assistantMessage;
        }
        finally
        {
            BusyStateChanged?.Invoke(this, false);
        }
    }





    /// <inheritdoc />
    public int SessionTokenCount { get; private set; }





    public async Task StartNewConversationAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await this.InitializeAsync(cancellationToken).ConfigureAwait(false);

        AgentSession session = GetRequiredSession();
        ProviderSessionState<HistoryIdentity> sessionState = GetRequiredSessionState();

        //Initiates a new conversation by clearing the current history and generating a new conversation ID.
        HistoryIdentityService.SaveConversationId(null, true);
        ConversationId = HistoryIdentityService.GetConversationId();

        _logger.LogWarning("A new conversation has been started at users request.");
        HistoryIdentity state = sessionState.GetOrInitializeState(session);
        state.ApplicationId = _historyIdentityService.Current.ApplicationId;
        state.AgentId = _historyIdentityService.Current.AgentId;
        state.ConversationId = ConversationId;
        state.Messages.Clear();
        AIHistory.Clear();

        session.StateBag.SetValue("ConversationId", ConversationId);
        _historyIdentityService.ApplyToSession(session);
        sessionState.SaveState(session, state);
    }





    /// <inheritdoc />
    public int SystemTokenCount { get; private set; }

    /// <inheritdoc />
    public int ToolTokenCount { get; private set; }





    /// <summary>
    ///     Initializes the chat conversation service, setting up necessary components such as the agent session
    ///     and conversation identifiers. Ensures the service is ready for use.
    /// </summary>
    /// <remarks>
    ///     This method is designed to be idempotent, meaning it can be safely called multiple times without
    ///     adverse effects. It uses a semaphore to prevent concurrent initialization attempts.
    /// </remarks>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the initialization process encounters an unexpected state or configuration issue.
    /// </exception>
    internal async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (Initialized)
        {
            return;
        }

        await _initializeGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (Initialized)
            {
                return;
            }

            ConversationId = HistoryIdentityService.GetConversationId();

            _agent = _agentFactory.GetCodingAssistantAgent(DefaultAgentId, AIModels.GLM5, "Agentic-Max Description");
            _agentSession = await _agent.CreateSessionAsync().ConfigureAwait(false);

            _historyIdentityService.Initialize(Settings.ApplicationId.ToString("D"), DefaultAgentId, _initialUserId);
            _historyIdentityService.ApplyToSession(_agentSession);

            _sessionStateHelper = new ProviderSessionState<HistoryIdentity>(currentSession => _historyIdentityService.Current, this.GetType().Name);
            _sessionStateHelper.SaveState(_agentSession, _historyIdentityService.Current);

            Initialized = true;
        }
        finally
        {
            _ = _initializeGate.Release();
        }
    }





    private AgentSession GetRequiredSession()
    {
        return _agentSession ?? throw new InvalidOperationException("Agent session is not initialized.");
    }





    private ProviderSessionState<HistoryIdentity> GetRequiredSessionState()
    {
        return _sessionStateHelper ?? throw new InvalidOperationException("Session state helper is not initialized.");
    }
}
