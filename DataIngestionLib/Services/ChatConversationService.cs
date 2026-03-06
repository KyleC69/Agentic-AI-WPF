// 2026/03/05
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         ChatConversationService.cs
//   Author: Kyle L. Crowder



using System.IO;
using System.Text.Json;

using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;
using DataIngestionLib.Options;
using DataIngestionLib.ToolFunctions;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using ChatMessage = DataIngestionLib.Models.ChatMessage;




namespace DataIngestionLib.Services;





public sealed class ChatConversationService : IChatConversationService
{
    private readonly string _localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private readonly ChatSessionOptions _options;
    private readonly ILoggerFactory _factory;
    private readonly IChatClient _client;
    private readonly IChatClient _outer;
    private readonly ChatOptions _clioptions;
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = false };








    public ChatConversationService(ChatSessionOptions options, IChatClient client, ILoggerFactory factory)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.ConfigurationsFolder))
        {
            throw new ArgumentException("Configurations folder must be configured.", nameof(options));
        }

        if (string.IsNullOrWhiteSpace(options.ChatSessionFileName))
        {
            throw new ArgumentException("Chat session file name must be configured.", nameof(options));
        }

        if (options.MaxContextTokens <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Maximum context tokens must be a positive value.");
        }

        _client = client;
        _factory = factory;
        _options = options;



        IChatClient outer = new ChatClientBuilder(_client)
                .UseLogging(_factory)
                .UseFunctionInvocation()
                .Build();


        ChatOptions clioptions = new()
        {
            Tools = ToolBuilder.GetAiTools(),
            Instructions = """

                               """,
            Temperature = 0.7f,
            MaxOutputTokens = 8000,
            AllowMultipleToolCalls = true,
            ToolMode = ChatToolMode.Auto
        };

        _clioptions = clioptions;
        _outer = outer;




    }








    /// <summary>
    ///     Loads the persisted chat session for the current local user profile.
    /// </summary>
    /// <returns>The loaded chat session or a new empty session when no persisted state is available.</returns>
    public ChatSessionState LoadSession()
    {
        string path = GetSessionFilePath();
        if (!File.Exists(path))
        {
            return new();
        }

        string json = File.ReadAllText(path);
        ChatSessionState? state = JsonSerializer.Deserialize<ChatSessionState>(json, SerializerOptions);
        ChatSessionState normalizedState = state ?? new ChatSessionState();

        return NormalizeState(normalizedState);
    }








    /// <summary>
    ///     Persists the provided chat session state to local storage.
    /// </summary>
    /// <param name="sessionState">The session state to persist.</param>
    public void SaveSession(ChatSessionState sessionState)
    {
        ArgumentNullException.ThrowIfNull(sessionState);

        ChatSessionState normalizedState = NormalizeState(sessionState);
        string folder = GetStorageFolderPath();
        Directory.CreateDirectory(folder);

        string json = JsonSerializer.Serialize(normalizedState, SerializerOptions);
        File.WriteAllText(GetSessionFilePath(), json);
    }








    /// <summary>
    ///     Creates a user chat message with normalized formatting and token metadata.
    /// </summary>
    /// <param name="content">The raw user input content.</param>
    /// <returns>A normalized user chat message.</returns>
    public ChatMessage CreateUserMessage(string content)
    {
        return CreateMessage(ChatMessageRole.User, content);
    }








    /// <summary>
    ///     Creates an assistant chat message with normalized formatting and token metadata.
    /// </summary>
    /// <param name="content">The assistant response content.</param>
    /// <returns>A normalized assistant chat message.</returns>
    public ChatMessage CreateAssistantMessage(string content)
    {
        return CreateMessage(ChatMessageRole.Assistant, content);
    }








    /// <summary>
    ///     Generates an assistant response message asynchronously for the supplied user message.
    /// </summary>
    /// <param name="userMessage">The user message content to answer.</param>
    /// <param name="contextTokenCount">The active context token count at generation time.</param>
    /// <param name="cancellationToken">The cancellation token for interrupting generation.</param>
    /// <returns>The generated assistant chat message.</returns>
    public async Task<ChatMessage> GenerateAssistantMessageAsync(string userMessage, int contextTokenCount, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
        {
            throw new ArgumentException("User message cannot be empty.", nameof(userMessage));
        }



        ChatResponse response = await _outer.GetResponseAsync(userMessage, _clioptions, cancellationToken);

        return CreateAssistantMessage(response.Text);
    }








    /// <summary>
    ///     Appends a chat message into history and context while enforcing the configured sliding token window.
    /// </summary>
    /// <param name="sessionState">The current session state.</param>
    /// <param name="message">The message to append.</param>
    /// <returns>The updated session state after enforcing sliding context rules.</returns>
    public ChatSessionState AppendMessage(ChatSessionState sessionState, ChatMessage message)
    {
        ArgumentNullException.ThrowIfNull(sessionState);
        ArgumentNullException.ThrowIfNull(message);

        ChatMessage normalizedMessage = NormalizeMessage(message);
        List<ChatMessage> history = sessionState.History.ToList();
        history.Add(normalizedMessage);

        List<ChatMessage> contextWindow = sessionState.ContextWindow.ToList();
        contextWindow.Add(normalizedMessage);

        int contextTokenCount = contextWindow.Sum(item => item.TokenCount);
        while (contextTokenCount > _options.MaxContextTokens && contextWindow.Count > 0)
        {
            contextTokenCount -= contextWindow[0].TokenCount;
            contextWindow.RemoveAt(0);
        }

        return new()
        {
            History = history,
            ContextWindow = contextWindow,
            ContextTokenCount = contextTokenCount
        };
    }








    private ChatSessionState AppendContextWindowLimit(ChatSessionState sessionState)
    {
        List<ChatMessage> contextWindow = sessionState.ContextWindow.ToList();
        int contextTokenCount = contextWindow.Sum(item => item.TokenCount);

        while (contextTokenCount > _options.MaxContextTokens && contextWindow.Count > 0)
        {
            contextTokenCount -= contextWindow[0].TokenCount;
            contextWindow.RemoveAt(0);
        }

        return sessionState with
        {
            ContextWindow = contextWindow,
            ContextTokenCount = contextTokenCount
        };
    }








    private ChatMessage CreateMessage(ChatMessageRole role, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Chat message content cannot be empty.", nameof(content));
        }

        string normalizedContent = content.Trim();
        return new()
        {
            Role = role,
            Content = normalizedContent,
            FormattedContent = FormatMarkdownLite(normalizedContent),
            TokenCount = EstimateTokenCount(normalizedContent),
            TimestampUtc = DateTimeOffset.UtcNow
        };
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








    private string GetSessionFilePath()
    {
        return Path.Combine(GetStorageFolderPath(), _options.ChatSessionFileName);
    }

    private string GetStorageFolderPath()
    {
        return Path.Combine(_localAppDataPath, _options.ConfigurationsFolder);
    }

    private static ChatMessage NormalizeMessage(ChatMessage message)
    {
        string normalizedContent = message.Content.Trim() ?? string.Empty;
        int tokenCount = message.TokenCount <= 0 ? EstimateTokenCount(normalizedContent) : message.TokenCount;
        string formattedContent = string.IsNullOrWhiteSpace(message.FormattedContent)
                ? FormatMarkdownLite(normalizedContent)
                : message.FormattedContent;

        return message with
        {
            Content = normalizedContent,
            FormattedContent = formattedContent,
            TokenCount = tokenCount,
            TimestampUtc = message.TimestampUtc == default ? DateTimeOffset.UtcNow : message.TimestampUtc
        };
    }








    private ChatSessionState NormalizeState(ChatSessionState sessionState)
    {
        List<ChatMessage> normalizedHistory = (sessionState.History ?? []).Select(NormalizeMessage).ToList();
        List<ChatMessage> normalizedContext = (sessionState.ContextWindow ?? []).Select(NormalizeMessage).ToList();

        if (normalizedContext.Count == 0)
        {
            ChatSessionState rebuiltState = new();
            foreach (ChatMessage message in normalizedHistory)
            {
                rebuiltState = AppendMessage(rebuiltState, message);
            }

            return rebuiltState with { History = normalizedHistory };
        }

        return AppendContextWindowLimit(new()
        {
            History = normalizedHistory,
            ContextWindow = normalizedContext,
            ContextTokenCount = normalizedContext.Sum(message => message.TokenCount)
        });
    }
}