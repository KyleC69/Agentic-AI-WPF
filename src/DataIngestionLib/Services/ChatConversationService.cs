// Build Date: 2026/03/29
// Solution: File
// Project:   DataIngestionLib
// File:         ChatConversationService.cs
// Author: Kyle L. Crowder
// Build Num: 051937



using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;
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
    private readonly HistoryIdentity _identity;
    private readonly SemaphoreSlim _initializeGate = new(1, 1);
    private readonly ILogger<ChatConversationService> _logger;
    private readonly ISQLChatHistoryProvider? _sqlChatHistoryProvider;
    private AIAgent? _agent;
    private AgentSession? _agentSession;
    private const string DefaultAgentId = "Agentic-Max";








    public ChatConversationService(ILoggerFactory factory, IAgentFactory agentFactory, IAppSettings settings, ISQLChatHistoryProvider? sqlChatHistoryProvider = null)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(agentFactory);
        _appSettings = settings;
        ConversationTokenBudget = settings.GetTokenBudget();
        _agentFactory = agentFactory;
        _sqlChatHistoryProvider = sqlChatHistoryProvider;
        _logger = factory.CreateLogger<ChatConversationService>();
        _identity = new HistoryIdentity();


    }








    /// <summary>
    ///     This is to provide an identifier in enterprise scenarios running multiple applications.
    /// </summary>
    public string ApplicationId
    {
        get { return _appSettings.ApplicationId; }
    }

    /// <summary>
    ///     A collection of settings to provide the token budget allocated for the model
    /// </summary>
    private TokenBudget ConversationTokenBudget { get; }

    public bool Initialized { get; set; }

    /// <summary>
    ///     Onlly used for history persistence and retrieval filter.
    /// </summary>
    public static string UserId
    {
        get { return Environment.UserName; }
    }

    /// <inheritdoc />
    public string ConversationId { get; private set; } = string.Empty;

    /// <summary>
    ///     Internal tracking history of the conversation with the LLM, used for calculating token usage and providing context
    ///     to the LLM. Not intended to be a full record of the conversation, but rather a window into the recent history that
    ///     is relevant for generating responses.
    ///     This allows for more efficient token usage while still maintaining enough context for coherent conversations.
    ///     This is actually managed by the sqlChatHistoryProvider and the Context Injectors.
    /// </summary>
    public List<ChatMessage> AIHistory { get; } = new List<ChatMessage>();

    /// <summary>
    ///     An estimate of the token count in the current conversation. TODO: will be moved to TokenBudget class for source of
    ///     truth
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
        await InitializeAsync();
        BusyStateChanged?.Invoke(this, true);
        UsageDetails? usageDetails = null;
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

            _identity.AgentId = _agentSession.StateBag.GetValue<string>("AgentId") ?? string.Empty;


            //Add user message to ChatHistory
            AIHistory.Add(new ChatMessage(ChatRole.User, content));


            AgentResponse response = await _agent.RunAsync(content, _agentSession, null, token);

            usageDetails = response.Usage;
            if (usageDetails is not null)
            {
                //TODO: Need to create LoggingMessage
                // _logger.LogUsages(usageDetails.InputTokenCount, usageDetails.CachedInputTokenCount, usageDetails.OutputTokenCount, usageDetails.ReasoningTokenCount, usageDetails.AdditionalCounts, usageDetails.TotalTokenCount);
            }




            //TODO: Need to test that context additions are being removed before getting here.
            var assistantText = response.Text?.Trim() ?? string.Empty;

            ChatMessage msg = new(ChatRole.Assistant, assistantText);
            AIHistory.Add(msg);


            return msg;
        }
        finally
        {
            UpdateTokenCounts(usageDetails);
            PublishTokenCounts();
            BusyStateChanged?.Invoke(this, false);
        }
    }








    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<ChatMessage>> LoadConversationHistoryAsync(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        await InitializeAsync().ConfigureAwait(false);

        if (_sqlChatHistoryProvider is null || _agentSession is null)
        {
            AIHistory.Clear();
            UpdateTokenCounts(null);
            return [];
        }

        var conversationId = _agentSession.StateBag.GetValue<string>("ConversationId") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            AIHistory.Clear();
            UpdateTokenCounts(null);
            return [];
        }

        ConversationId = conversationId;

        var persistedMessages = await _sqlChatHistoryProvider.GetMessagesAsync(conversationId, null, token).ConfigureAwait(false);

        List<ChatMessage> historyMessages = [];
        foreach (PersistedChatMessage persistedMessage in persistedMessages)
        {
            if (string.IsNullOrWhiteSpace(persistedMessage.Content))
            {
                continue;
            }

            var roleValue = persistedMessage.Role?.Trim() ?? string.Empty;
            ChatRole role = roleValue.Length == 0 ? ChatRole.User : new ChatRole(roleValue);

            historyMessages.Add(new ChatMessage(role, persistedMessage.Content) { CreatedAt = persistedMessage.TimestampUtc, MessageId = persistedMessage.MessageId.ToString("D") });
        }

        AIHistory.Clear();
        AIHistory.AddRange(historyMessages);
        UpdateTokenCounts(null);

        return historyMessages;
    }








    /// <inheritdoc />
    public event EventHandler<bool>? BusyStateChanged;








    private TokenBuckets CalculateContextTokenBuckets()
    {
        var sessionTokens = 0;
        var ragTokens = 0;
        var toolTokens = 0;
        var systemTokens = 0;
        var totalTokens = 0;

        for (var index = AIHistory.Count - 1; index >= 0; index--)
        {
            var content = AIHistory[index].Text;
            var messageTokenCount = EstimateTokenCount(content);
            if (totalTokens + messageTokenCount > ConversationTokenBudget.SessionBudget)
            {
                break;
            }

            var role = AIHistory[index].Role.Value;
            if (string.Equals(role, AIChatRole.System.Value, StringComparison.OrdinalIgnoreCase))
            {
                systemTokens += messageTokenCount;
            }
            else if (string.Equals(role, AIChatRole.Tool.Value, StringComparison.OrdinalIgnoreCase))
            {
                toolTokens += messageTokenCount;
            }
            else if (string.Equals(role, AIChatRole.RAGContext.Value, StringComparison.OrdinalIgnoreCase) || string.Equals(role, AIChatRole.AIContext.Value, StringComparison.OrdinalIgnoreCase) || string.Equals(role, "rag", StringComparison.OrdinalIgnoreCase))
            {
                ragTokens += messageTokenCount;
            }
            else
            {
                sessionTokens += messageTokenCount;
            }

            totalTokens += messageTokenCount;
        }

        return new TokenBuckets(totalTokens, sessionTokens, ragTokens, toolTokens, systemTokens);
    }








    private static int ClampToInt(long value, int fallback)
    {
        return value <= 0 ? fallback : value >= int.MaxValue ? int.MaxValue : (int)value;
    }








    private static int EstimateTokenCount(string content)
    {
        return string.IsNullOrWhiteSpace(content) ? 0 : Math.Max(1, content.Length / 4);
    }








    private static long GetAdditionalCount(UsageDetails usageDetails, params string[] keys)
    {
        if (usageDetails.AdditionalCounts is null || usageDetails.AdditionalCounts.Count == 0)
        {
            return 0;
        }

        foreach (var key in keys)
        {
            foreach (var (countKey, countValue) in usageDetails.AdditionalCounts)
                if (string.Equals(countKey, key, StringComparison.OrdinalIgnoreCase))
                {
                    return countValue;
                }
        }

        return 0;
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
            if (Initialized)
            {
                return;
            }

            _agent = _agentFactory.GetCodingAssistantAgent(DefaultAgentId, AIModels.GPTOSS, "Agentic-Max Description");

            _agentSession = await _agent.CreateSessionAsync().ConfigureAwait(false);
            _agentSession.StateBag.SetValue("ApplicationId", ApplicationId);
            _agentSession.StateBag.SetValue("UserId", UserId);
            _agentSession.StateBag.SetValue("AgentId", DefaultAgentId);

            ConversationId = await ResolveStartupConversationIdAsync(CancellationToken.None).ConfigureAwait(false);
            _agentSession.StateBag.SetValue("ConversationId", ConversationId);

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
        if (_sqlChatHistoryProvider is not null)
        {
            var applicationId = string.IsNullOrWhiteSpace(ApplicationId) ? "unknown-application" : ApplicationId;
            var userId = string.IsNullOrWhiteSpace(UserId) ? "unknown-user" : UserId;
            var latestConversationId = await _sqlChatHistoryProvider.GetLatestConversationIdAsync(DefaultAgentId, userId, applicationId, cancellationToken).ConfigureAwait(false);

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








    private void UpdateTokenCounts(UsageDetails? usageDetails)
    {
        TokenBuckets buckets = CalculateContextTokenBuckets();

        ContextTokenCount = buckets.Total;
        SessionTokenCount = buckets.Session;
        RagTokenCount = buckets.Rag;
        ToolTokenCount = buckets.Tool;
        SystemTokenCount = buckets.System;

        if (usageDetails?.AdditionalCounts is null)
        {
            return;
        }

        var ragUsageTokens = GetAdditionalCount(usageDetails, "rag", "rag_tokens", "rag_token_count", "rag_context", "retrieval", "retrieval_tokens", "context", "context_tokens");
        var toolUsageTokens = GetAdditionalCount(usageDetails, "tool", "tool_tokens", "tool_token_count", "function", "function_tokens");
        var systemUsageTokens = GetAdditionalCount(usageDetails, "system", "system_tokens", "system_token_count", "instruction", "instruction_tokens");

        RagTokenCount = ClampToInt(ragUsageTokens, RagTokenCount);
        ToolTokenCount = ClampToInt(toolUsageTokens, ToolTokenCount);
        SystemTokenCount = ClampToInt(systemUsageTokens, SystemTokenCount);

        var reserved = RagTokenCount + ToolTokenCount + SystemTokenCount;
        SessionTokenCount = Math.Max(0, ContextTokenCount - reserved);
    }








    private readonly record struct TokenBuckets(int Total, int Session, int Rag, int Tool, int System);
}