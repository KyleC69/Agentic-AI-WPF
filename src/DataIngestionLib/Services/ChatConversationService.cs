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

using System.Text.Json;

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
    private readonly IAppSettings _appSettings;
    private readonly IHistoryIdentityService _historyIdentityService;
    private readonly SemaphoreSlim _initializeGate = new(1, 1);
    private readonly ILogger<ChatConversationService> _logger;
    private readonly SqlChatHistoryProvider? _sqlChatHistoryProvider;
    private AIAgent? _agent;
    private AgentSession? _agentSession;
    private ProviderSessionState<HistoryIdentity> _sessionStateHelper;
    private const string DefaultAgentId = "Agentic-Max";

    public ChatConversationService(ILoggerFactory factory, IAgentFactory agentFactory, IAppSettings settings, IHistoryIdentityService historyIdentityService, SqlChatHistoryProvider? sqlChatHistoryProvider = null)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(agentFactory);
        ArgumentNullException.ThrowIfNull(historyIdentityService);
        _appSettings = settings;
        ConversationTokenBudget = settings.GetTokenBudget();
        _agentFactory = agentFactory;
        _historyIdentityService = historyIdentityService;
        _sqlChatHistoryProvider = sqlChatHistoryProvider;
        _logger = factory.CreateLogger<ChatConversationService>();
    }

    /// <summary>
    ///     This is to provide an identifier in enterprise scenarios running multiple applications.
    /// </summary>
    public string ApplicationId => _appSettings.ApplicationId;

    /// <summary>
    ///     A collection of settings to provide the token budget allocated for the model.
    /// </summary>
    private TokenBudget ConversationTokenBudget { get; }

    public bool Initialized { get; set; }

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
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("User message cannot be empty.", nameof(content));
            }

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

    public async ValueTask<IReadOnlyList<ChatMessage>> LoadConversationHistoryAsync(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        await this.InitializeAsync().ConfigureAwait(false);

        if (_sqlChatHistoryProvider is null || _agentSession is null)
        {
            AIHistory.Clear();
            return [];
        }

        var conversationId = _agentSession.StateBag.GetValue<string>("ConversationId") ?? _historyIdentityService.Current.ConversationId;
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            AIHistory.Clear();
            return [];
        }

        _historyIdentityService.SetConversationId(conversationId);
        _historyIdentityService.ApplyToSession(_agentSession);
        ConversationId = _historyIdentityService.Current.ConversationId;

        IReadOnlyList<PersistedChatMessage> persistedMessages = await _sqlChatHistoryProvider.GetMessagesAsync(conversationId, token).ConfigureAwait(false);

        List<ChatMessage> historyMessages = [];
        foreach (PersistedChatMessage persistedMessage in persistedMessages)
        {
            if (string.IsNullOrWhiteSpace(persistedMessage.Content))
            {
                continue;
            }

            var roleValue = persistedMessage.Role?.Trim() ?? string.Empty;
            ChatRole role = roleValue.Length == 0 ? ChatRole.User : new ChatRole(roleValue);

            historyMessages.Add(new ChatMessage(role, persistedMessage.Content)
            {
                CreatedAt = persistedMessage.TimestampUtc,
                MessageId = persistedMessage.MessageId.ToString("D")
            });
        }

        _sessionStateHelper.SaveState(_agentSession, new HistoryIdentity { Messages = historyMessages });

        AIHistory.Clear();
        AIHistory.AddRange(historyMessages);

        return historyMessages;
    }

    /// <inheritdoc />
    public event EventHandler<bool>? BusyStateChanged;

    /// <inheritdoc />
    public event EventHandler<TokenUsageSnapshot>? TokenUsageUpdated;

    private async Task InitializeAsync()
    {
        if (Initialized)
        {
            return;
        }

        await _initializeGate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (Initialized)
            {
                return;
            }

            _agent = _agentFactory.GetCodingAssistantAgent(DefaultAgentId, AIModels.GPTOSS, "Agentic-Max Description", tokenSnapshotSink: this.OnTokenSnapshotObserved);

            _agentSession = await _agent.CreateSessionAsync().ConfigureAwait(false);

            _historyIdentityService.Initialize(ApplicationId, DefaultAgentId, _appSettings.UserId);

            var startupConversationId = await this.ResolveStartupConversationIdAsync(CancellationToken.None).ConfigureAwait(false);
            _historyIdentityService.SetConversationId(startupConversationId);
            _historyIdentityService.ApplyToSession(_agentSession);

            ConversationId = _historyIdentityService.Current.ConversationId;

            _sessionStateHelper = new ProviderSessionState<HistoryIdentity>(
                stateInitializer: currentSession => _historyIdentityService.Current,
                stateKey: this.GetType().Name);

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

    private void PublishTokenCounts()
    {
        var sessionTokens = ContextTokenCount;

        if (sessionTokens >= ConversationTokenBudget.SessionBudget)
        {
            SessionBugetExceeded?.Invoke(this, EventArgs.Empty);
            TokenBudgetExceeded?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (sessionTokens >= ConversationTokenBudget.MaximumContext)
        {
            MaximumContextWarning?.Invoke(this, sessionTokens);
        }
    }

    private async ValueTask<string> ResolveStartupConversationIdAsync(CancellationToken cancellationToken)
    {
        HistoryIdentity identity = _historyIdentityService.Current;

        if (_sqlChatHistoryProvider is not null)
        {
            var applicationId = string.IsNullOrWhiteSpace(identity.ApplicationId) ? "unknown-application" : identity.ApplicationId;
            var userId = string.IsNullOrWhiteSpace(identity.UserId) ? "unknown-user" : identity.UserId;
            var agentId = string.IsNullOrWhiteSpace(identity.AgentId) ? DefaultAgentId : identity.AgentId;
            var latestConversationId = await _sqlChatHistoryProvider.GetLatestConversationIdAsync(agentId, userId, applicationId, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(latestConversationId))
            {
                return latestConversationId.Trim();
            }
        }

        var configuredConversationId = _appSettings.LastConversationId?.Trim() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(configuredConversationId) ? configuredConversationId : Guid.NewGuid().ToString("N");
    }

    /// <inheritdoc />
    public event EventHandler? SessionBugetExceeded;

    /// <inheritdoc />
    public event EventHandler? TokenBudgetExceeded;

    private void OnTokenSnapshotObserved(TokenUsageSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        ContextTokenCount = ReadInt(snapshot.AdditionalCounts, "context_total_tokens", snapshot.TotalTokens);
        SessionTokenCount = ReadInt(snapshot.AdditionalCounts, "context_session_tokens", snapshot.SessionTokens);
        RagTokenCount = ReadInt(snapshot.AdditionalCounts, "context_rag_tokens", snapshot.RagTokens);
        ToolTokenCount = ReadInt(snapshot.AdditionalCounts, "context_tool_tokens", snapshot.ToolTokens);
        SystemTokenCount = ReadInt(snapshot.AdditionalCounts, "context_system_tokens", snapshot.SystemTokens);

        this.PublishTokenCounts();
        TokenUsageUpdated?.Invoke(this, snapshot);
    }

    private static int ReadInt(IReadOnlyDictionary<string, long> values, string key, int fallback)
    {
        return values.TryGetValue(key, out long value) ? ClampToInt(value) : fallback;
    }

    private static int ClampToInt(long value)
    {
        return value <= 0 ? 0 : value >= int.MaxValue ? int.MaxValue : (int)value;
    }
}
