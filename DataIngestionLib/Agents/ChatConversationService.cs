// 2026/03/10
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         ChatConversationService.cs
//   Author: Kyle L. Crowder



using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;
using DataIngestionLib.Options;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;




namespace DataIngestionLib.Agents;





/// <summary>
///     Class is responsible for managing the chat conversation round with LLM, self-contained and keeps viewmodel clean.
/// </summary>
public sealed class ChatConversationService : IChatConversationService
{
    private readonly AIAgent _agent;
    private readonly AgentSession _agentSession;
    private readonly IRuntimeContextAccessor _contextAccessor;
    private readonly ChatSessionOptions _options;








    public ChatConversationService(ChatSessionOptions options, IChatClient client, ILoggerFactory factory, IAgentFactory agentFactory, IRuntimeContextAccessor runtimeContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(agentFactory);
        ArgumentNullException.ThrowIfNull(runtimeContextAccessor);

        if (options.MaxContextTokens <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Maximum context tokens must be a positive value.");
        }

        _options = options;
        _agent = agentFactory.GetCodingAssistantAgent();

        _agentSession = _agent.CreateSessionAsync().Result;
        _contextAccessor = runtimeContextAccessor;




    }








    public string ApplicationId
    {
        get { return _contextAccessor.GetCurrent().ApplicationId.ToString(); }
    }





    public string UserId
    {
        get { return _contextAccessor.GetCurrent().UserPrincipalName; }
    }





    //Duplicate history objects?  We should not need to track sepearately the session holds the context and our sql backed chat history should be handling all the history objects.
    public ChatHistory ChatHistory { get; } = [];





    public int ContextTokenCount
    {
        get { return CalculateContextTokenCount(); }
    }








    /// <summary>
    ///     Sends request to LLM and waits for a responsel chat history.
    /// </summary>
    /// <param name="userMessage">The user message content to answer.</param>
    /// <param name="cancellationToken">The cancellation token for interrupting generation.</param>
    /// <returns>The generated assistant chat message.</returns>
    public async ValueTask<AIChatMessage> SendRequestToModelAsync(string userMessage, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
        {
            throw new ArgumentException("User message cannot be empty.", nameof(userMessage));
        }

        //Add user message to ChatHistory
        ChatHistory.AddUserMessage(userMessage);
        AgentResponse response = await _agent.RunAsync(userMessage, _agentSession, null, cancellationToken);
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

        AIChatMessage assistantMessage = new AIChatMessage(ChatRole.Assistant, assistantText);
        if (!string.IsNullOrWhiteSpace(assistantMessage.Text))
        {
            ChatHistory.Add(assistantMessage);
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
            if (tokenCount + messageTokenCount > _options.MaxContextTokens)
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








    private static string FormatMarkdownLite(string content)
    {
        var normalized = content.Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace("**", string.Empty, StringComparison.Ordinal)
                .Replace("__", string.Empty, StringComparison.Ordinal)
                .Replace("`", string.Empty, StringComparison.Ordinal);

        var lines = normalized.Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].TrimEnd();
            if (line.StartsWith("### ", StringComparison.Ordinal))
            {
                lines[i] = line[4..];
                continue;
            }

            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                lines[i] = line[3..];
                continue;
            }

            if (line.StartsWith("# ", StringComparison.Ordinal))
            {
                lines[i] = line[2..].ToUpperInvariant();
                continue;
            }

            if (line.StartsWith("- ", StringComparison.Ordinal) || line.StartsWith("* ", StringComparison.Ordinal))
            {
                lines[i] = $"• {line[2..]}";
            }
        }

        return string.Join(Environment.NewLine, lines);
    }
}