// Build Date: 2026/03/19
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ChatConversationService.cs
// Author: Kyle L. Crowder
// Build Num: 044255



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
    private const string DefaultAgentId = "Agentic-Max";


    private readonly IAgentFactory _agentFactory;
    private readonly IAppSettings _appSettings;
    private readonly HistoryIdentity _identity;
    private readonly SemaphoreSlim _initializeGate = new(1, 1);
    private readonly ILogger<ChatConversationService> _logger;
    private AIAgent? _agent;
    private AgentSession? _agentSession;
    private int _contextTokenCount;
    private int _ragTokenCount;
    private int _sessionTokenCount;
    private int _systemTokenCount;
    private int _toolTokenCount;








    public ChatConversationService(ILoggerFactory factory, IAgentFactory agentFactory, IAppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(agentFactory);
        _appSettings = settings;
        ConversationTokenBudget = settings.GetTokenBudget();
        _agentFactory = agentFactory;
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
    ///     Need to be exposed to UI and allow users to reset their
    ///     their context session. We will use the SessionID as the context identifier and reload previous conversations based
    ///     on that.
    /// </summary>
    public string SessionId { get; set; }
        = string.Empty;

    /// <summary>
    ///     Onlly used for history persistence and retrieval filter.
    /// </summary>
    public static string UserId
    {
        get { return Environment.UserName; }
    }

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
    public int ContextTokenCount
    {
        get { return _contextTokenCount; }
    }

    /// <inheritdoc />
    public int SessionTokenCount
    {
        get { return _sessionTokenCount; }
    }

    /// <inheritdoc />
    public int ToolTokenCount
    {
        get { return _toolTokenCount; }
    }

    /// <inheritdoc />
    public int RagTokenCount
    {
        get { return _ragTokenCount; }
    }

    /// <inheritdoc />
    public int SystemTokenCount
    {
        get { return _systemTokenCount; }
    }








    /// <summary>
    ///     Sends request to LLM and waits for a responsel chat history.
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
                _logger.LogUsages(usageDetails.InputTokenCount, usageDetails.CachedInputTokenCount, usageDetails.OutputTokenCount, usageDetails.ReasoningTokenCount, usageDetails.AdditionalCounts, usageDetails.TotalTokenCount);
            }




            //TODO: Need to test that context additions are being removed before getting here.
            var assistantText = response.Text?.Trim() ?? string.Empty;

            ChatMessage msg = new ChatMessage(ChatRole.Assistant, assistantText);
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
            else if (string.Equals(role, AIChatRole.RAGContext.Value, StringComparison.OrdinalIgnoreCase)
                     || string.Equals(role, AIChatRole.AIContext.Value, StringComparison.OrdinalIgnoreCase)
                     || string.Equals(role, "rag", StringComparison.OrdinalIgnoreCase))
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








    private static int EstimateTokenCount(string content)
    {
        return string.IsNullOrWhiteSpace(content) ? 0 : Math.Max(1, content.Length / 4);
    }






    private void UpdateTokenCounts(UsageDetails? usageDetails)
    {
        TokenBuckets buckets = CalculateContextTokenBuckets();

        _contextTokenCount = buckets.Total;
        _sessionTokenCount = buckets.Session;
        _ragTokenCount = buckets.Rag;
        _toolTokenCount = buckets.Tool;
        _systemTokenCount = buckets.System;

        if (usageDetails?.AdditionalCounts is null)
        {
            return;
        }

        long ragUsageTokens = GetAdditionalCount(
                usageDetails,
                "rag",
                "rag_tokens",
                "rag_token_count",
                "rag_context",
                "retrieval",
                "retrieval_tokens",
                "context",
                "context_tokens");
        long toolUsageTokens = GetAdditionalCount(
                usageDetails,
                "tool",
                "tool_tokens",
                "tool_token_count",
                "function",
                "function_tokens");
        long systemUsageTokens = GetAdditionalCount(
                usageDetails,
                "system",
                "system_tokens",
                "system_token_count",
                "instruction",
                "instruction_tokens");

        _ragTokenCount = ClampToInt(ragUsageTokens, _ragTokenCount);
        _toolTokenCount = ClampToInt(toolUsageTokens, _toolTokenCount);
        _systemTokenCount = ClampToInt(systemUsageTokens, _systemTokenCount);

        int reserved = _ragTokenCount + _toolTokenCount + _systemTokenCount;
        _sessionTokenCount = Math.Max(0, _contextTokenCount - reserved);
    }






    private static int ClampToInt(long value, int fallback)
    {
        if (value <= 0)
        {
            return fallback;
        }

        return value >= int.MaxValue ? int.MaxValue : (int)value;
    }






    private static long GetAdditionalCount(UsageDetails usageDetails, params string[] keys)
    {
        if (usageDetails.AdditionalCounts is null || usageDetails.AdditionalCounts.Count == 0)
        {
            return 0;
        }

        foreach (string key in keys)
        {
            foreach ((string countKey, long countValue) in usageDetails.AdditionalCounts)
            {
                if (string.Equals(countKey, key, StringComparison.OrdinalIgnoreCase))
                {
                    return countValue;
                }
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


        // Create the system default agent with the specified Model.
        // A different agent could be created with different instructions and tools if desired.
        // AgentId must be unique for each agent created, it is used in history persistence to associate messages with the agent that generated them.
        // This allows for long term behavior analysis on the performance of agent presets and tools.
        _agent = _agentFactory.GetCodingAssistantAgent(DefaultAgentId, AIModels.GPTOSS, "Agentic-Max Description");

        _agentSession = await _agent.CreateSessionAsync();
        _agentSession.StateBag.SetValue("ApplicationId", ApplicationId);
        _agentSession.StateBag.SetValue("UserId", UserId);
        _agentSession.StateBag.SetValue("AgentId", DefaultAgentId);
        SessionId = Guid.NewGuid().ToString();
        _agentSession.StateBag.SetValue("SessionId", SessionId);
        _agentSession.StateBag.SetValue("ConversationId", SessionId);






        Initialized = true;

        }
        finally
        {
            _initializeGate.Release();
        }

    }








    /// <inheritdoc />
    public event EventHandler<int>? MaximumContextWarning;








    private void PublishTokenCounts()
    {
        int sessionTokens = ContextTokenCount;


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








    /// <inheritdoc />
    public event EventHandler? SessionBugetExceeded;

    /// <inheritdoc />
    public event EventHandler? TokenBudgetExceeded;






    private readonly record struct TokenBuckets(int Total, int Session, int Rag, int Tool, int System);
}