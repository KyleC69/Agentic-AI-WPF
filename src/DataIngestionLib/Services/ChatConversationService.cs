// Build Date: 2026/04/03
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ChatConversationService.cs
// Author: Kyle L. Crowder
// Build Num: 095155



using CommunityToolkit.Diagnostics;

using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;
using DataIngestionLib.Providers;
using DataIngestionLib.Services.Contracts;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;




namespace DataIngestionLib.Services;





/// <summary>
///     Class is responsible for managing the chat conversation round with LLM, self-contained and keeps viewmodel clean.
///     Encapsulates the management of the agent operations.
/// </summary>
public sealed class ChatConversationService : IChatConversationService
{
    private AIAgent? _agent;
    private readonly IAgentFactory _agentFactory;
    private AgentSession? _agentSession;
    private readonly IHistoryIdentityService _historyIdentityService;
    private readonly SemaphoreSlim _initializeGate = new(1, 1);
    private readonly string _initialUserId;
    private readonly ILogger<ChatConversationService> _logger;
    private ProviderSessionState<HistoryIdentity> _sessionStateHelper = null!;
    private readonly SqlChatHistoryProvider? _sqlChatHistoryProvider;
    private const string DefaultAgentId = "Agentic-Max";








    public ChatConversationService(ILoggerFactory factory, IAgentFactory agentFactory, IHistoryIdentityService historyIdentityService, SqlChatHistoryProvider? sqlChatHistoryProvider)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(agentFactory);
        ArgumentNullException.ThrowIfNull(historyIdentityService);
        Guard.IsNotNull(sqlChatHistoryProvider);
        ApplicationId = historyIdentityService.Current.ApplicationId;
        _initialUserId = historyIdentityService.Current.UserId;
        _agentFactory = agentFactory;
        _historyIdentityService = historyIdentityService;
        _sqlChatHistoryProvider = sqlChatHistoryProvider;
        _logger = factory.CreateLogger<ChatConversationService>();



    }








    /// <summary>
    ///     This is to provide an identifier in enterprise scenarios running multiple applications.
    /// </summary>
    public string ApplicationId { get; }

    /// <summary>
    ///     A collection of settings to provide the token budget allocated for the model.
    /// </summary>
    private TokenBudget ConversationTokenBudget { get; }

    public bool Initialized { get; set; }

    /// <summary>
    ///     The last conversation ID that was set. The WPF layer should read this property
    ///     and persist it to Settings. Default.LastConversationId.
    /// </summary>
    public string LastConversationIdValue { get; private set; } = string.Empty;

    private AppSettings Settings { get; } = new AppSettings();

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
        await InitializeAsync().ConfigureAwait(false);

        if (_sqlChatHistoryProvider is null || _agentSession is null)
        {
            AIHistory.Clear();

            return [];
        }

        ConversationId = HistoryIdentityService.GetConversationId();

        var historyMessages = await RagDataService.GetChatHistoryByConversationId(Guid.Parse(ConversationId));

        //Tag messages to identify source as history load vs new conversation.
        var tagged = historyMessages.Select(m => m.WithAgentRequestMessageSource(AgentRequestMessageSourceType.ChatHistory, this.GetType().Name));

        //Load the state and attach the history messages to it.
        HistoryIdentity state = _sessionStateHelper.GetOrInitializeState(_agentSession);
        state.Messages.AddRange(tagged);
        // Update the session to reflect the loaded conversation history.
        _sessionStateHelper.SaveState(_agentSession, state);


        return tagged;
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
        await InitializeAsync().ConfigureAwait(false);
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
        //Initiates a new conversation by clearing the current history and generating a new conversation ID.
        HistoryIdentityService.SaveConversationId(null, true);
        ConversationId = HistoryIdentityService.GetConversationId();

        _logger.LogWarning("A new conversation has been started at users request.");
        HistoryIdentity State = _sessionStateHelper.GetOrInitializeState(_agentSession);
        State.ApplicationId = _historyIdentityService.Current.ApplicationId;
        State.AgentId = _historyIdentityService.Current.AgentId;
        State.ConversationId = ConversationId;
        State.Messages.Clear();
        AIHistory.Clear();

        _agentSession?.StateBag.SetValue("ConversationId", ConversationId);
        _historyIdentityService.ApplyToSession(_agentSession);
        _sessionStateHelper.SaveState(_agentSession, State);


    }








    /// <inheritdoc />
    public int SystemTokenCount { get; private set; }

    /// <inheritdoc />
    public int ToolTokenCount { get; private set; }








    private static int ClampToInt(long value)
    {
        return value <= 0 ? 0 : value >= int.MaxValue ? int.MaxValue : (int)value;
    }








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
    internal async Task InitializeAsync()
    {

        if (Initialized)
        {
            return;
        }

        await _initializeGate.WaitAsync().ConfigureAwait(false);
        try
        {
            ConversationId = HistoryIdentityService.GetConversationId();

            _agent = _agentFactory.GetCodingAssistantAgent(DefaultAgentId, AIModels.GPTOSS, "Agentic-Max Description");

            _agentSession = await _agent.CreateSessionAsync().ConfigureAwait(false);


            _historyIdentityService.Initialize(Settings.ApplicationId.ToString("D"), DefaultAgentId, _initialUserId);

            _historyIdentityService.ApplyToSession(_agentSession);


            _sessionStateHelper = new ProviderSessionState<HistoryIdentity>(currentSession => _historyIdentityService.Current, this.GetType().Name);

            _sessionStateHelper.SaveState(_agentSession, _historyIdentityService.Current);

            Initialized = true;
        }
        finally
        {
            var unused = _initializeGate.Release();
        }
    }








    /// <inheritdoc />
    public event EventHandler<int>? MaximumContextWarning;








    private static int ReadInt(IReadOnlyDictionary<string, long> values, string key, int fallback)
    {
        return values.TryGetValue(key, out var value) ? ClampToInt(value) : fallback;
    }
}