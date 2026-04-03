// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//
//
//
//



using System.Diagnostics;

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
    private readonly IAgentFactory _agentFactory;
    private readonly IHistoryIdentityService _historyIdentityService;
    private readonly SemaphoreSlim _initializeGate = new(1, 1);
    private readonly string _initialLastConversationId;
    private readonly string _initialUserId;
    private readonly ILogger<ChatConversationService> _logger;
    private readonly bool _resumeLast;
    private readonly SqlChatHistoryProvider? _sqlChatHistoryProvider;
    private AIAgent? _agent;
    private AgentSession? _agentSession;
    private ProviderSessionState<HistoryIdentity> _sessionStateHelper = null!;
    private const string DefaultAgentId = "Agentic-Max";








    public ChatConversationService(ILoggerFactory factory, IAgentFactory agentFactory, string applicationId, bool resumeLast, string initialUserId, string initialLastConversationId, TokenBudget tokenBudget, IHistoryIdentityService historyIdentityService, SqlChatHistoryProvider? sqlChatHistoryProvider)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(agentFactory);
        ArgumentNullException.ThrowIfNull(historyIdentityService);
        ArgumentNullException.ThrowIfNullOrEmpty(applicationId);
        ArgumentNullException.ThrowIfNullOrEmpty(initialUserId);
        ArgumentNullException.ThrowIfNull(tokenBudget);
        ArgumentNullException.ThrowIfNull(initialLastConversationId);
        Guard.IsNotNull(sqlChatHistoryProvider);
        ApplicationId = applicationId;
        _resumeLast = resumeLast;
        _initialUserId = initialUserId;
        _initialLastConversationId = initialLastConversationId;
        ConversationTokenBudget = tokenBudget;
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

    /// <inheritdoc />
    public string ConversationId { get; private set; } = string.Empty;

    /// <summary>
    ///     Internal tracking history of the conversation with the LLM.
    /// </summary>
    public List<ChatMessage> AIHistory { get; } = [];

    /// <summary>
    ///     Current token context derived from middleware snapshots.
    /// </summary>
    public int ContextTokenCount { get; private set; }

    /// <inheritdoc />
    public int SessionTokenCount { get; private set; }

    /// <inheritdoc />
    public int ToolTokenCount { get; private set; }

    /// <inheritdoc />
    public int RagTokenCount { get; private set; }

    /// <inheritdoc />
    public int SystemTokenCount { get; private set; }








    /// <summary>
    ///     Sends request to LLM and waits for a response.
    /// </summary>
    /// <param name="content">The user message content to answer.</param>
    /// <param name="token">The cancellation token for interrupting generation.</param>
    /// <returns>The generated assistant chat message.</returns>
    /// <exception cref="ArgumentException"></exception>
    public async ValueTask<ChatMessage> SendRequestToModelAsync(string content, CancellationToken token)
    {
        await this.InitializeAsync().ConfigureAwait(false);
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








    public async Task StartNewConversationAsync(CancellationToken cancellationToken)
    {
        //Initiates a new conversation by clearing the current history and generating a new conversation ID.
        HistoryIdentityService.SaveConversationId(null, true);

        _logger.LogWarning("A new conversation has been started at users request.");

        //Making  sure our identity sessionState is populated.
        HistoryIdentity State = _sessionStateHelper.GetOrInitializeState(_agentSession);
        State.ApplicationId = _historyIdentityService.Current.ApplicationId;
        State.AgentId = _historyIdentityService.Current.AgentId;
        State.ConversationId = HistoryIdentityService.GetConversationId();
        State.Messages.Clear();
        AIHistory.Clear();

        _agentSession?.StateBag.SetValue("ConversationId", ConversationId);
        // This is crucial to ensure the _sessionStateHelper has the latest ConversationId from the _historyIdentityService.
        // It synchronizes the session state with the conversation state managed by _historyIdentityService.
        Debug.Assert(_agentSession != null, nameof(_agentSession) + " != null");
        _historyIdentityService.ApplyToSession(_agentSession);
        //_historyIdentityService.Current.ConversationId;
        _sessionStateHelper.SaveState(_agentSession, State);


    }








    async ValueTask<IEnumerable<ChatMessage>> IChatConversationService.LoadConversationHistoryAsync(CancellationToken token)
    {

        token.ThrowIfCancellationRequested();
        await this.InitializeAsync().ConfigureAwait(false);

        if (_sqlChatHistoryProvider is null || _agentSession is null)
        {
            AIHistory.Clear();

            return [];
        }

        ConversationId = HistoryIdentityService.GetConversationId();

        IReadOnlyList<ChatMessage>? historyMessages = await RagDataService.GetChatHistoryByConversationId(Guid.Parse(ConversationId));

        //Tag messages to idebtufy source as history load vs new conversation.
        IEnumerable<ChatMessage> tagged = historyMessages.Select(m => m.WithAgentRequestMessageSource(AgentRequestMessageSourceType.ChatHistory));

        //Load the state and attach the history messages to it.
        HistoryIdentity state = _sessionStateHelper.GetOrInitializeState(_agentSession);
        state.Messages.AddRange(tagged);
        // Update the session to reflect the loaded conversation history.
        _sessionStateHelper.SaveState(_agentSession, state);


        return tagged;
    }








    /// <inheritdoc />
    public event EventHandler<bool>? BusyStateChanged;








    private static int ClampToInt(long value)
    {
        return value <= 0 ? 0 : value >= int.MaxValue ? int.MaxValue : (int)value;
    }








    private async Task InitializeAsync()
    {
        if (Initialized)
        {
            return;
        }

        await _initializeGate.WaitAsync().ConfigureAwait(false);
        try
        {


            _agent = _agentFactory.GetCodingAssistantAgent(DefaultAgentId, AIModels.GPTOSS, "Agentic-Max Description");

            _agentSession = await _agent.CreateSessionAsync().ConfigureAwait(false);


            _historyIdentityService.Initialize(ApplicationId, DefaultAgentId, _initialUserId);



            ConversationId = HistoryIdentityService.GetConversationId();

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