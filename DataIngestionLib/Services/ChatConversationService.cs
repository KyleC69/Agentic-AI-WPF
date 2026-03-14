// Build Date: 2026/03/13
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ChatConversationService.cs
// Author: Kyle L. Crowder
// Build Num: 175056



using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using SystemConfigurationManager = System.Configuration.ConfigurationManager;




namespace DataIngestionLib.Services;





/// <summary>
///     Class is responsible for managing the chat conversation round with LLM, self-contained and keeps viewmodel clean.
/// </summary>
public sealed class ChatConversationService : IChatConversationService
{
    private readonly AIAgent _agent;
    private readonly AgentSession _agentSession;
    private readonly int _maxContextTokens;








    public ChatConversationService(ILoggerFactory factory, IAgentFactory agentFactory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(agentFactory);

        _maxContextTokens = int.TryParse(SystemConfigurationManager.AppSettings["MaxContextTokens"], out var maxContextTokens) && maxContextTokens > 0
                ? maxContextTokens
                : 120000;

        if (_maxContextTokens <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(_maxContextTokens), "Maximum context tokens must be a positive value.");
        }

        _agent = agentFactory.GetCodingAssistantAgent();

        _agentSession = _agent.CreateSessionAsync().IsCompleted
                ? _agent.CreateSessionAsync().GetAwaiter().GetResult()
                : throw new InvalidOperationException("Failed to create agent session.");





    }








    public string ApplicationId
    {
        get { return SystemConfigurationManager.AppSettings["ApplicationId"] ?? AppDomain.CurrentDomain.FriendlyName; }
    }





    public string UserId
    {
        get { return Environment.UserName; }
    }





    //Duplicate history objects?  We should not need to track sepearately the session holds the context and our sql backed chat history should be handling all the history objects.
    public AIChatHistory ChatHistory { get; } = [];





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
    public async ValueTask<AIChatMessage> SendRequestToModelAsync(string content, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("User message cannot be empty.", nameof(content));
        }

        //Add user message to ChatHistory
        ChatHistory.AddUserMessage(content);
        AgentResponse response = await _agent.RunAsync(content, _agentSession, null, token);
        var assistantText = response.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(assistantText))
        {

            //What is this extra item!!!
            //Too much duplication
            assistantText = string.Join(
                    Environment.NewLine,
                    response.Messages
                            .Where(static message => message.Role == ChatRole.Assistant)
                            .Select(static message => message.Text?.Trim())
                            .Where(static text => !string.IsNullOrWhiteSpace(text)));
        }

        AIChatMessage assistantMessage = new(ChatRole.Assistant, assistantText);
        if (!string.IsNullOrWhiteSpace(assistantMessage.Text))
        {
            ((ICollection<AIChatMessage>)ChatHistory).Add(assistantMessage);
        }

        return assistantMessage;
    }








    private int CalculateContextTokenCount()
    {
        var tokenCount = 0;

        for (var index = ChatHistory.Count - 1; index >= 0; index--)
        {
            var content = ChatHistory[index].Text ?? string.Empty;
            var messageTokenCount = EstimateTokenCount(content);
            if (tokenCount + messageTokenCount > _maxContextTokens)
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
}