// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ChatConversationService.cs
// Author: Kyle L. Crowder
// Build Num: 090953



using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;
using DataIngestionLib.Services.Contracts;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;




namespace DataIngestionLib.Services;





/// <summary>
///     Class is responsible for managing the chat conversation round with LLM, self-contained and keeps viewmodel clean.
/// </summary>
public sealed class ChatConversationService : IChatConversationService
{
    private readonly IAgentFactory _agentFactory;
    private readonly IAppSettings _appSettings;
    private AIAgent? _agent;
    private AgentSession? _agentSession;








    public ChatConversationService(ILoggerFactory factory, IAgentFactory agentFactory, IAppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(agentFactory);
        _appSettings = settings;
        ConversationTokenBudget = settings.GetTokenBudget();
        _agentFactory = agentFactory;


    }








    public string ApplicationId
    {
        get { return _appSettings.ApplicationId ?? Guid.NewGuid().ToString(); }
    }





    private TokenBudget ConversationTokenBudget { get; }





    public bool Initialized { get; set; }





    public static string UserId
    {
        get { return Environment.UserName; }
    }





    /// <summary>
    ///     Duplicate history objects?  We should not need to track sepearately the session holds the context and our sql
    ///     backed chat history should be handling all the history objects.
    /// </summary>
    public List<ChatMessage> ChatHistory { get; } = new();





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
        ChatHistory.Add(new ChatMessage(ChatRole.User, content));
        AgentResponse response = await _agent.RunAsync(content, _agentSession, null, token);
        var assistantText = response.Text.Trim();
        if (string.IsNullOrWhiteSpace(assistantText))
        {

            //What is this extra item!!!
            //Too much duplication
            assistantText = string.Join(
                    Environment.NewLine,
                    response.Messages
                            .Where(static message => message.Role == ChatRole.Assistant)
                            .Select(static message => message.Text.Trim())
                            .Where(static text => !string.IsNullOrWhiteSpace(text)));
        }

        ChatMessage assistantMessage = new ChatMessage(ChatRole.Assistant, assistantText);
        if (!string.IsNullOrWhiteSpace(assistantMessage.Text))
        {
            ChatHistory.Add(assistantMessage);
        }

        return assistantMessage;
    }








    private int CalculateContextTokenCount()
    {
        //TODO: this needs to be adapted to include all budgets


        var tokenCount = 0;

        for (var index = ChatHistory.Count - 1; index >= 0; index--)
        {
            var content = ChatHistory[index].Text;
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


        Initialized = true;

    }
}