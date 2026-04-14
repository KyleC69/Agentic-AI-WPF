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



using CommunityToolkit.Diagnostics;

using AgentAILib.Agents;
using AgentAILib.Contracts;
using AgentAILib.EFModels;
using AgentAILib.HistoryModels;
using AgentAILib.Models;
using AgentAILib.Providers;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;




namespace AgentAILib.Services;





/// <summary>
///     Class is responsible for managing the chat conversation round with LLM, self-contained and keeps viewmodel clean.
///     Encapsulates the management of the agent operations.
/// </summary>
public sealed class ChatConversationService : ChatConversationBase, IChatConversationService
{
    private readonly SqlChatHistoryProvider? _sqlChatHistoryProvider;
    private AIModelDescriptor _currentModel = AIModels.Default;








    public ChatConversationService(ILoggerFactory factory, IAgentFactory agentFactory, IHistoryIdentityService historyIdentityService, SqlChatHistoryProvider? sqlChatHistoryProvider, IAppSettings appSettings) : base(appSettings)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(agentFactory);
        ArgumentNullException.ThrowIfNull(historyIdentityService);
        ArgumentNullException.ThrowIfNull(appSettings);
        Guard.IsNotNull(sqlChatHistoryProvider);
        ApplicationId = historyIdentityService.Current.ApplicationId;
        _initialUserId = historyIdentityService.Current.UserId;
        _agentFactory = agentFactory;
        _historyIdentityService = historyIdentityService;
        _sqlChatHistoryProvider = sqlChatHistoryProvider;
        _logger = factory.CreateLogger<ChatConversationService>();



    }








    /// <summary>
    ///     Current token context derived from middleware snapshots.
    /// </summary>
    public int ContextTokenCount { get; private set; }

    public bool Initialized { get; set; }

    /// <summary>
    ///     Internal tracking history of the conversation with the LLM.
    /// </summary>
    public List<ChatMessage> AIHistory { get; } = [];

    /// <inheritdoc />
    public event EventHandler<bool>? BusyStateChanged;

    /// <inheritdoc />
    public string ConversationId { get; private set; } = HistoryIdentityService.GetConversationId();








    public async ValueTask<IEnumerable<ChatMessage>> LoadConversationHistoryAsync(CancellationToken token)
    {
        AIChatHistoryDb db = new();
        token.ThrowIfCancellationRequested();
        await this.InitializeAsync().ConfigureAwait(false);

        if (_sqlChatHistoryProvider is null || _agentSession is null)
        {
            AIHistory.Clear();

            return [];
        }

        ConversationId = HistoryIdentityService.GetConversationId();

        List<ChatHistoryMessage> historyMessages = db.ChatHistoryMessages.Where(m => m.ConversationId == ConversationId).OrderByDescending(m => m.CreatedAt).ToList();
        IReadOnlyList<ChatMessage> messages = historyMessages.ToChatMessages();
        //Tag messages to identify source as history load vs new conversation.
        IEnumerable<ChatMessage> tagged = messages.Select(m => m.WithAgentRequestMessageSource(AgentRequestMessageSourceType.ChatHistory, this.GetType().Name));

        return tagged;
    }








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
            Guard.IsNotNull(_agent);
            Guard.IsNotNull(_agentSession);
            ChatMessage cm = new(new ChatRole("User"), content);
            cm.CreatedAt = DateTime.Now;

            AIHistory.Add(cm);

            AgentResponse response = await _agent.RunAsync(cm, _agentSession, null, token).ConfigureAwait(false);

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
    public async Task ChangeModelAsync(AIModelDescriptor descriptor, CancellationToken token)
    {
        Guard.IsNotNull(descriptor);

        await _initializeGate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            _currentModel = descriptor;
            Initialized = false;
            _agent = null;
            _agentSession = null;
        }
        finally
        {
            _initializeGate.Release();
        }
    }


    public async Task StartNewConversationAsync(CancellationToken cancellationToken)    {
        //Initiates a new conversation by clearing the current history and generating a new conversation ID.
        HistoryIdentityService.SaveConversationId(null, true);
        ConversationId = HistoryIdentityService.GetConversationId();

        _logger?.LogWarning("A new conversation has been started at users request.");

        HistoryIdentity State = _sessionStateHelper.GetOrInitializeState(_agentSession);
        if (_historyIdentityService != null)
        {
            State.ApplicationId = _historyIdentityService.Current.ApplicationId;
            State.AgentId = _historyIdentityService.Current.AgentId;
            State.ConversationId = ConversationId;
            State.Messages.Clear();
            AIHistory.Clear();

            _agentSession?.StateBag.SetValue("ConversationId", ConversationId);
            if (_agentSession != null)
            {
                _historyIdentityService.ApplyToSession(_agentSession);
                _sessionStateHelper.SaveState(_agentSession, State);
            }
        }


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
            IAgentFactory agentFactory = _agentFactory ?? throw new InvalidOperationException("Agent factory has not been initialized.");
            IHistoryIdentityService historyIdentityService = _historyIdentityService ?? throw new InvalidOperationException("History identity service has not been initialized.");
            var initialUserId = string.IsNullOrWhiteSpace(_initialUserId) ? historyIdentityService.Current.UserId : _initialUserId;

            ConversationId = HistoryIdentityService.GetConversationId();
            IChatClient client = agentFactory.GetChatClient(_currentModel);
            _agent = agentFactory.BuildAssistantAgent(client, DefaultAgentId, "AgentName", "Agentic-Max Description");

            _agentSession = await _agent.CreateSessionAsync().ConfigureAwait(false);
            _agentSession.StateBag.SetValue("ConversationId", ConversationId);
            _agentSession.StateBag.SetValue("ApplicationId", historyIdentityService.Current.ApplicationId);
            _agentSession.StateBag.SetValue("AgentId", historyIdentityService.Current.AgentId);
            _agentSession.StateBag.SetValue("UserId", historyIdentityService.Current.UserId);


            historyIdentityService.Initialize(Settings.ApplicationId, DefaultAgentId, initialUserId);

            historyIdentityService.ApplyToSession(_agentSession);


            _sessionStateHelper = new ProviderSessionState<HistoryIdentity>(currentSession => historyIdentityService.Current, this.GetType().Name);

            _sessionStateHelper.SaveState(_agentSession, historyIdentityService.Current);

            Initialized = true;
        }
        finally
        {
            var unused = _initializeGate.Release();
        }
    }
}