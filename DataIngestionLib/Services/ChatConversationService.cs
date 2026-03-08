// 2026/03/07
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




namespace DataIngestionLib.Services;





public sealed class ChatConversationService : IChatConversationService
{
    private readonly AIAgent _agent;
    private readonly AgentSession _agentSession;
    private readonly ChatSessionOptions _options;
    private readonly IRuntimeContextAccessor _contextAccessor;








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
        _agentSession = _agent.CreateSessionAsync().GetAwaiter().GetResult();
        _contextAccessor = runtimeContextAccessor;




    }


    public string ApplicationId => _contextAccessor.GetCurrent().ApplicationId.ToString();
    public string UserId => _contextAccessor.GetCurrent().UserPrincipalName.ToString();





    public ChatHistory ChatHistory { get; } = [];





    public int ContextTokenCount => CalculateContextTokenCount();








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
        string assistantText = response.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(assistantText))
        {
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
            ChatHistory.Add(assistantMessage);
        }

        return assistantMessage;
    }








    private int CalculateContextTokenCount()
    {
        int tokenCount = 0;

        for (int index = ChatHistory.Count - 1; index >= 0; index--)
        {
            string content = ChatHistory[index].Text ?? string.Empty;
            int messageTokenCount = EstimateTokenCount(content);
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
        string normalized = content.Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace("**", string.Empty, StringComparison.Ordinal)
                .Replace("__", string.Empty, StringComparison.Ordinal)
                .Replace("`", string.Empty, StringComparison.Ordinal);

        string[] lines = normalized.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].TrimEnd();
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