using System.IO;
using System.Text.Json;

using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;
using DataIngestionLib.Options;

namespace DataIngestionLib.Services;

public sealed class ChatConversationService : IChatConversationService
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = false };
    private readonly ChatSessionOptions _options;
    private readonly string _localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    public ChatConversationService(ChatSessionOptions options)
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

        _options = options;
    }

    /// <summary>
    /// Loads the persisted chat session for the current local user profile.
    /// </summary>
    /// <returns>The loaded chat session or a new empty session when no persisted state is available.</returns>
    public ChatSessionState LoadSession()
    {
        var path = GetSessionFilePath();
        if (!File.Exists(path))
        {
            return new ChatSessionState();
        }

        var json = File.ReadAllText(path);
        var state = JsonSerializer.Deserialize<ChatSessionState>(json, SerializerOptions);
        var normalizedState = state ?? new ChatSessionState();

        return NormalizeState(normalizedState);
    }

    /// <summary>
    /// Persists the provided chat session state to local storage.
    /// </summary>
    /// <param name="sessionState">The session state to persist.</param>
    public void SaveSession(ChatSessionState sessionState)
    {
        ArgumentNullException.ThrowIfNull(sessionState);

        var normalizedState = NormalizeState(sessionState);
        var folder = GetStorageFolderPath();
        Directory.CreateDirectory(folder);

        var json = JsonSerializer.Serialize(normalizedState, SerializerOptions);
        File.WriteAllText(GetSessionFilePath(), json);
    }

    /// <summary>
    /// Creates a user chat message with normalized formatting and token metadata.
    /// </summary>
    /// <param name="content">The raw user input content.</param>
    /// <returns>A normalized user chat message.</returns>
    public ChatMessage CreateUserMessage(string content)
        => CreateMessage(ChatMessageRole.User, content);

    /// <summary>
    /// Creates an assistant chat message with normalized formatting and token metadata.
    /// </summary>
    /// <param name="content">The assistant response content.</param>
    /// <returns>A normalized assistant chat message.</returns>
    public ChatMessage CreateAssistantMessage(string content)
        => CreateMessage(ChatMessageRole.Assistant, content);

    /// <summary>
    /// Generates an assistant response message asynchronously for the supplied user message.
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

        await Task.Delay(TimeSpan.FromMilliseconds(800), cancellationToken);

        var response = $"I received your message and this chat is ready for model integration.\n\nUser input:\n- {userMessage}\n\nContext tokens: {contextTokenCount}";
        return CreateAssistantMessage(response);
    }

    /// <summary>
    /// Appends a chat message into history and context while enforcing the configured sliding token window.
    /// </summary>
    /// <param name="sessionState">The current session state.</param>
    /// <param name="message">The message to append.</param>
    /// <returns>The updated session state after enforcing sliding context rules.</returns>
    public ChatSessionState AppendMessage(ChatSessionState sessionState, ChatMessage message)
    {
        ArgumentNullException.ThrowIfNull(sessionState);
        ArgumentNullException.ThrowIfNull(message);

        var normalizedMessage = NormalizeMessage(message);
        var history = sessionState.History.ToList();
        history.Add(normalizedMessage);

        var contextWindow = sessionState.ContextWindow.ToList();
        contextWindow.Add(normalizedMessage);

        var contextTokenCount = contextWindow.Sum(item => item.TokenCount);
        while (contextTokenCount > _options.MaxContextTokens && contextWindow.Count > 0)
        {
            contextTokenCount -= contextWindow[0].TokenCount;
            contextWindow.RemoveAt(0);
        }

        return new ChatSessionState
        {
            History = history,
            ContextWindow = contextWindow,
            ContextTokenCount = contextTokenCount
        };
    }

    private ChatSessionState NormalizeState(ChatSessionState sessionState)
    {
        var normalizedHistory = (sessionState.History ?? []).Select(NormalizeMessage).ToList();
        var normalizedContext = (sessionState.ContextWindow ?? []).Select(NormalizeMessage).ToList();

        if (normalizedContext.Count == 0)
        {
            var rebuiltState = new ChatSessionState();
            foreach (var message in normalizedHistory)
            {
                rebuiltState = AppendMessage(rebuiltState, message);
            }

            return rebuiltState with { History = normalizedHistory };
        }

        return AppendContextWindowLimit(new ChatSessionState
        {
            History = normalizedHistory,
            ContextWindow = normalizedContext,
            ContextTokenCount = normalizedContext.Sum(message => message.TokenCount)
        });
    }

    private ChatSessionState AppendContextWindowLimit(ChatSessionState sessionState)
    {
        var contextWindow = sessionState.ContextWindow.ToList();
        var contextTokenCount = contextWindow.Sum(item => item.TokenCount);

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

        var normalizedContent = content.Trim();
        return new ChatMessage
        {
            Role = role,
            Content = normalizedContent,
            FormattedContent = FormatMarkdownLite(normalizedContent),
            TokenCount = EstimateTokenCount(normalizedContent),
            TimestampUtc = DateTimeOffset.UtcNow
        };
    }

    private static ChatMessage NormalizeMessage(ChatMessage message)
    {
        var normalizedContent = message.Content?.Trim() ?? string.Empty;
        var tokenCount = message.TokenCount <= 0 ? EstimateTokenCount(normalizedContent) : message.TokenCount;
        var formattedContent = string.IsNullOrWhiteSpace(message.FormattedContent)
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

    private static int EstimateTokenCount(string content)
        => string.IsNullOrWhiteSpace(content) ? 0 : Math.Max(1, content.Length / 4);

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

    private string GetStorageFolderPath()
        => Path.Combine(_localAppDataPath, _options.ConfigurationsFolder);

    private string GetSessionFilePath()
        => Path.Combine(GetStorageFolderPath(), _options.ChatSessionFileName);
}
