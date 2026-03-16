// Build Date: 2026/03/16
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ChatConversationService.cs
// Author: Kyle L. Crowder
// Build Num: 051926



using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Logging;
using DataIngestionLib.Models;
using DataIngestionLib.Services.Contracts;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;




namespace DataIngestionLib.Services;





/// <summary>
///     Class is responsible for managing the chat conversation round with LLM, self-contained and keeps viewmodel clean.
/// </summary>
public sealed class ChatConversationService : IChatConversationService
{

    private readonly IAgentFactory _agentFactory;
    private readonly IAppSettings _appSettings;
    private readonly ILogger<ChatConversationService> _logger;
    private AIAgent? _agent;
    private AgentSession? _agentSession;








    public ChatConversationService(ILoggerFactory factory, IAgentFactory agentFactory, IAppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(agentFactory);
        _appSettings = settings;
        ConversationTokenBudget = settings.GetTokenBudget();
        _agentFactory = agentFactory;
        _logger = factory.CreateLogger<ChatConversationService>();

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
    public List<ChatMessage> AIHistory { get; } = new();

    /// <summary>
    ///     An estimate of the token count in the current conversation. TODO: will be moved to TokenBudget class for source of
    ///     truth
    /// </summary>
    public int ContextTokenCount
    {
        get { return CalculateContextTokenCount(); }
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
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("User message cannot be empty.", nameof(content));
        }

        //Add user message to ChatHistory
        AIHistory.Add(new ChatMessage(ChatRole.User, content));


        AgentResponse response = await _agent.RunAsync(content, _agentSession, null, token);

        UsageDetails deets = response.Usage;
        _logger.LogUsages(deets.InputTokenCount, deets.CachedInputTokenCount, deets.OutputTokenCount, deets.ReasoningTokenCount, deets.AdditionalCounts, deets.TotalTokenCount);




        //TODO: Need to test that context additions are being removed before getting here.
        var assistantText = response.Text.Trim();

        ChatMessage msg = new ChatMessage(ChatRole.Assistant, assistantText);
        AIHistory.Add(msg);

        return msg;
    }








    private int CalculateContextTokenCount()
    {
        //TODO: this needs to be adapted to include all budgets


        var tokenCount = 0;

        for (var index = AIHistory.Count - 1; index >= 0; index--)
        {
            var content = AIHistory[index].Text;
            var messageTokenCount = EstimateTokenCount(content);
            if (tokenCount + messageTokenCount > ConversationTokenBudget.SessionBudget)
            {
                break;
            }

            tokenCount += messageTokenCount;
        }

        return tokenCount;
    }








    private static int EstimateTokenCount(string content)
    {
        return string.IsNullOrWhiteSpace(content) ? 0 : Math.Max(1, content.Length / 4);
    }








    private async Task InitializeAsync()
    {
        if (Initialized)
        {
            return;
        }


        // Create the system default agent with the specified Model.
        // A different agent could be created with different instructions and tools if desired.
        // AgentId must be unique for each agent created, it is used in history persistence to associate messages with the agent that generated them.
        // This allows for long term behavior analysis on the performance of agent presets and tools.
        _agent = _agentFactory.GetCodingAssistantAgent("Agentic-Max", AIModels.GPTOSS, "Agentic-Max Description");

        _agentSession = await _agent.CreateSessionAsync();
        _agentSession.StateBag.SetValue("ApplicationId", ApplicationId);
        _agentSession.StateBag.SetValue("UserId", UserId);
        _agentSession.StateBag.SetValue("SessionId", Guid.NewGuid().ToString());






        Initialized = true;

    }








    //Thrown when the session token budget is near exhausted, allowing for immediate window trimming.
    //This is intended to be a proactive measure to prevent hitting hard limits on the model and allow
    // for some trimming on the backend before throwning. Ideal scenario is if a tool or context injector
    //suddenly bloats the session history.
    public static event EventHandler? SessionBugetExceeded;
    public static event EventHandler? TokenBudgetExceeded;
}