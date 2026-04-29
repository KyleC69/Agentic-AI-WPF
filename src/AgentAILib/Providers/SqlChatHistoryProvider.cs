// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         SqlChatHistoryProvider.cs
// Author: Kyle L. Crowder
// Build Num: 194500



using System.Text.Json;
using System.Text.Json.Serialization;

using AgentAILib.Contracts;
using AgentAILib.EFModels;
using AgentAILib.HistoryModels;
using AgentAILib.Services;

using CommunityToolkit.Diagnostics;

using Microsoft.Agents.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;




namespace AgentAILib.Providers;





public sealed class SqlChatHistoryProvider : ChatHistoryProvider
{
    private readonly int _charsPerToken = 4;
    private readonly IDbContextFactory<AIChatHistoryDb> _dbcontextFactory;
    private readonly IHistoryIdentityService _historyIdentityService;
    private readonly ILogger<SqlChatHistoryProvider> _logger;
    private readonly ProviderSessionState<HistoryIdentity> _sessionState;

    private const int CHARS_PER_TOKEN = 4;
    private const int MAX_JSON_DEPTH = 32;

    private static readonly JsonSerializerOptions StrictJsonOptions = new() { MaxDepth = MAX_JSON_DEPTH, WriteIndented = true, IndentSize = 2 };
    private static readonly JsonSerializerOptions CycleSafeJsonOptions = new() { MaxDepth = MAX_JSON_DEPTH, WriteIndented = true, IndentSize = 2, ReferenceHandler = ReferenceHandler.IgnoreCycles };








    public SqlChatHistoryProvider(ILogger<SqlChatHistoryProvider> logger, IHistoryIdentityService historyIdentityService, IDbContextFactory<AIChatHistoryDb> dbcontextFactory, Func<AgentSession, HistoryIdentity>? stateInitializer = null, string? stateKey = null)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(historyIdentityService);
        ArgumentNullException.ThrowIfNull(dbcontextFactory);

        _logger = logger;
        _historyIdentityService = historyIdentityService;
        _dbcontextFactory = dbcontextFactory;

        _sessionState = new ProviderSessionState<HistoryIdentity>(stateInitializer: currentSession => new HistoryIdentity(HistoryIdentityService.GetConversationId()), stateKey: this.GetType().Name);
    }








    protected override async ValueTask<IEnumerable<ChatMessage>> ProvideChatHistoryAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        if (context is null)
        {
            return [];
        }


        try
        {
            HistoryIdentity state = _sessionState.GetOrInitializeState(context.Session);
            var historyMessages = await GetMessagesAsync(state.ConversationId, cancellationToken).ConfigureAwait(false);
            var tagged = historyMessages?.Select(message => message.WithAgentRequestMessageSource(AgentRequestMessageSourceType.ChatHistory, nameof(SqlChatHistoryProvider)));
            return tagged ?? [];
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to provide chat history.");
            return [];
        }
    }








    protected override async ValueTask StoreChatHistoryAsync(InvokedContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            HistoryIdentity state = _sessionState.GetOrInitializeState(context.Session);
            var newMessages = context.RequestMessages.Concat(context.ResponseMessages ?? []).ToList();
            var persistableMessages = newMessages.Where(ShouldPersistMessage).ToList();

            _logger.LogTrace("Beginning to save chat messages for conversation {ConversationId}", state.ConversationId);
            if (persistableMessages.Count == 0)
            {
                return;
            }

            await PersistInteractionAsync(state, persistableMessages, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to persist chat history to SQL store.");
        }
    }








    public override IReadOnlyList<string> StateKeys
    {
        get { return new[] { _sessionState.StateKey }; }
    }








    private int EstimateTokenCount(ChatMessage message)
    {
        try
        {
            var serialized = JsonSerializer.Serialize(message, StrictJsonOptions);
            return Math.Max(1, (int)Math.Ceiling(serialized.Length / (double)CHARS_PER_TOKEN));
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(exception, "Cycle or deep graph detected while estimating chat message token count. Falling back to cycle-safe serialization.");

            try
            {
                var serialized = JsonSerializer.Serialize(message, CycleSafeJsonOptions);
                return Math.Max(1, (int)Math.Ceiling(serialized.Length / (double)CHARS_PER_TOKEN));
            }
            catch (Exception fallbackException)
            {
                _logger.LogDebug(fallbackException, "Cycle-safe serialization failed during token estimation. Falling back to text-length estimate.");
                var textLength = message.Text?.Length ?? 0;
                return Math.Max(1, (int)Math.Ceiling(textLength / (double)CHARS_PER_TOKEN));
            }
        }
    }








    public async ValueTask<IEnumerable<ChatMessage>?> GetMessagesAsync(string conversationId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await using AIChatHistoryDb db = await _dbcontextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogTrace("Fetching chat history messages for conversation {ConversationId}", conversationId);
        try
        {
            var ordered = await db.ChatHistoryMessages.Where(message => message.ConversationId == conversationId).OrderBy(message => message.CreatedAt).ToListAsync(cancellationToken).ConfigureAwait(false);

            if (ordered.Count == 0)
            {
                return [];
            }

            var chatMessages = ordered.ToChatMessages();
            var tagged = chatMessages.Select(message => message.WithAgentRequestMessageSource(AgentRequestMessageSourceType.ChatHistory, nameof(SqlChatHistoryProvider)));

            return tagged;
        }
        catch (InvalidOperationException)
        {

            _logger.LogTrace("An error was caught during chat history message load");
        }
        catch
        {
            _logger.LogTrace("Unknown error occurred during chat history message load");
        }

        return new List<ChatMessage>();
    }








    private static bool HasExplicitSourceType(ChatMessage message, AgentRequestMessageSourceType sourceType)
    {
        return message.AdditionalProperties is not null && message.AdditionalProperties.TryGetValue(AgentRequestMessageSourceAttribution.AdditionalPropertiesKey, out var value) && value is AgentRequestMessageSourceAttribution attribution && attribution.SourceType == sourceType;
    }








    private async ValueTask PersistInteractionAsync(HistoryIdentity identity, IEnumerable<ChatMessage> messages, CancellationToken cancellationToken)
    {
        await using AIChatHistoryDb dbContext = await _dbcontextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var entities = ToEntities(messages, identity);
        if (entities.Count == 0)
        {
            return;
        }

        try
        {
            _logger.LogTrace("Persisting chat history messages for conversation {ConversationId}", identity.ConversationId);
            await dbContext.ChatHistoryMessages.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
            _ = await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error occurred during record save.");
        }
    }








    private string? SerializeMetadata(IReadOnlyDictionary<string, object?>? additionalProperties)
    {
        if (additionalProperties is null || additionalProperties.Count == 0)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Serialize(additionalProperties, StrictJsonOptions);
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(exception, "Detected a metadata object graph cycle or depth overflow. Persisting sanitized metadata payload.");
            return SerializeMetadataFallback(additionalProperties);
        }
        catch (Exception exception)
        {
            _logger.LogDebug(exception, "Chat message metadata could not be serialized and will be skipped.");
            return null;
        }
    }






    private string? SerializeMetadataFallback(IReadOnlyDictionary<string, object?> additionalProperties)
    {
        try
        {
            Dictionary<string, object?> safeMetadata = new(StringComparer.OrdinalIgnoreCase)
            {
                    ["_serializationWarning"] = "CycleOrDepthDetected",
                    ["_propertyCount"] = additionalProperties.Count
            };

            foreach (KeyValuePair<string, object?> property in additionalProperties)
            {
                safeMetadata[property.Key] = property.Value?.ToString();
            }

            return JsonSerializer.Serialize(safeMetadata, CycleSafeJsonOptions);
        }
        catch (Exception exception)
        {
            _logger.LogDebug(exception, "Metadata fallback serialization failed and metadata will be skipped.");
            return null;
        }
    }








    private static bool ShouldPersistMessage(ChatMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.Text))
        {
            return false;
        }

        // Tool results are retained for the active turn only and are not persisted.
        if (message.Role == ChatRole.Tool)
        {
            return false;
        }

        // Do not persist historical replay messages.
        if (HasExplicitSourceType(message, AgentRequestMessageSourceType.ChatHistory))
        {
            return false;
        }

        // Do not persist explicitly tagged external/provider-context messages.
        return !HasExplicitSourceType(message, AgentRequestMessageSourceType.External) && !HasExplicitSourceType(message, AgentRequestMessageSourceType.AIContextProvider);
    }








    internal List<ChatHistoryMessage> ToEntities(IEnumerable<ChatMessage> messages, HistoryIdentity identity)
    {
        HashSet<Guid> seenMessageIds = [];
        List<ChatHistoryMessage> entities = [];

        foreach (ChatMessage message in messages)
        {
            if (string.IsNullOrWhiteSpace(message.Text))
            {
                continue;
            }

            Guid messageId = TryParseMessageId(message.MessageId);
            Guard.IsNotDefault(messageId);
            if (!seenMessageIds.Add(messageId))
            {
                continue;
            }

            entities.Add(new ChatHistoryMessage
            {
                    MessageId = messageId,
                    ConversationId = identity.ConversationId,
                    AgentId = identity.AgentId,
                    UserId = identity.UserId,
                    ApplicationId = identity.ApplicationId,
                    Role = message.Role.Value,
                    Content = message.Text.Trim(),
                    Metadata = SerializeMetadata(message.AdditionalProperties),
                    CreatedAt = DateTime.Now,
                    Enabled = true,
                    TokenCnt = EstimateTokenCount(message)
            });
        }

        return entities;
    }








    private static Guid TryParseMessageId(string? messageId)
    {
        return Guid.TryParse(messageId, out Guid parsedMessageId) ? parsedMessageId : Guid.NewGuid();
    }
}