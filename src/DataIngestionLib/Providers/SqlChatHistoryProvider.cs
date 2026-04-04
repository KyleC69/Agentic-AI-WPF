// Build Date: 2026/04/04
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         SqlChatHistoryProvider.cs
// Author: GitHub Copilot


using System.Text.Json;

using CommunityToolkit.Diagnostics;

using DataIngestionLib.Contracts;
using DataIngestionLib.EFModels;
using DataIngestionLib.HistoryModels;
using DataIngestionLib.Services;

using Microsoft.Agents.AI;
using Microsoft.Agents.Core.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;


namespace DataIngestionLib.Providers;



public sealed class SqlChatHistoryProvider : ChatHistoryProvider, ISqlChatHistoryProvider
{
    private readonly IDbContextFactory<AIChatHistoryDb> _dbcontextFactory;
    private readonly IHistoryIdentityService _historyIdentityService;
    private readonly ILogger<SqlChatHistoryProvider> _logger;
    private readonly ProviderSessionState<HistoryIdentity> _sessionState;
    private readonly int _charsPerToken = 4;

    private static readonly JsonSerializerOptions JsonOptions = new();





    public SqlChatHistoryProvider(ILogger<SqlChatHistoryProvider> logger, IHistoryIdentityService historyIdentityService, IDbContextFactory<AIChatHistoryDb> dbcontextFactory, Func<AgentSession, HistoryIdentity>? stateInitializer = null, string? stateKey = null)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(historyIdentityService);
        ArgumentNullException.ThrowIfNull(dbcontextFactory);

        _logger = logger;
        _historyIdentityService = historyIdentityService;
        _dbcontextFactory = dbcontextFactory;

        _sessionState = new ProviderSessionState<HistoryIdentity>((stateInitializer ?? (_ => new HistoryIdentity(HistoryIdentityService.GetConversationId())))!, stateKey ?? this.GetType().Name);
    }





    protected override async ValueTask<IEnumerable<ChatMessage>> ProvideChatHistoryAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        if (context is null)
        {
            return [];
        }

        HistoryIdentity state = _sessionState.GetOrInitializeState(context.Session);

        try
        {
            IEnumerable<ChatMessage>? historyMessages = await this.GetMessagesAsync(state.ConversationId, cancellationToken).ConfigureAwait(false);
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
            List<ChatMessage> newMessages = context.RequestMessages.Concat(context.ResponseMessages ?? []).ToList();
            List<ChatMessage> persistableMessages = newMessages.Where(ShouldPersistMessage).ToList();

            _logger.LogTrace("Beginning to save chat messages for conversation {ConversationId}", state.ConversationId);
            if (persistableMessages.Count == 0)
            {
                return;
            }

            await this.PersistInteractionAsync(state, persistableMessages, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to persist chat history to SQL store.");
        }
    }





    public override IReadOnlyList<string> StateKeys => new[] { _sessionState.StateKey };





    public async ValueTask<string?> GetLatestConversationIdAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await using AIChatHistoryDb dbContext = await _dbcontextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var latest = await dbContext.ChatHistoryMessages.AsNoTracking()
            .OrderByDescending(message => message.CreatedAt)
            .ThenByDescending(message => message.MessageId)
            .Select(message => message.ConversationId)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return string.IsNullOrWhiteSpace(latest) ? null : latest;
    }





    public async ValueTask<IEnumerable<ChatMessage>?> GetMessagesAsync(string conversationId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await using AIChatHistoryDb db = await _dbcontextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogTrace("Fetching chat history messages for conversation {ConversationId}", conversationId);
        List<ChatHistoryMessage> ordered = await db.ChatHistoryMessages
            .Where(message => message.ConversationId == conversationId)
            .OrderBy(message => message.CreatedAt)
            .ThenBy(message => message.MessageId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (ordered.Count == 0)
        {
            return [];
        }

        IReadOnlyList<ChatMessage> chatMessages = ordered.ToChatMessages();
        IEnumerable<ChatMessage> tagged = chatMessages.Select(message => message.WithAgentRequestMessageSource(AgentRequestMessageSourceType.ChatHistory, nameof(SqlChatHistoryProvider)));

        return tagged;
    }





    private int EstimateTokenCount(ChatMessage message)
    {
        var serialized = JsonSerializer.Serialize(message, JsonOptions);
        return Math.Max(1, (int)Math.Ceiling(serialized.Length / (double)_charsPerToken));
    }





    private async ValueTask PersistInteractionAsync(HistoryIdentity identity, IEnumerable<ChatMessage> messages, CancellationToken cancellationToken)
    {
        await using AIChatHistoryDb dbContext = await _dbcontextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        List<ChatHistoryMessage> entities = this.ToEntities(messages, identity);
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
            return JsonSerializer.Serialize(additionalProperties);
        }
        catch (Exception exception)
        {
            _logger.LogDebug(exception, "Chat message metadata could not be serialized and will be skipped.");
            return null;
        }
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

            var createdAtLocal = DateTime.Now;
            entities.Add(new ChatHistoryMessage
            {
                MessageId = messageId,
                ConversationId = identity.ConversationId,
                AgentId = identity.AgentId,
                UserId = identity.UserId,
                ApplicationId = identity.ApplicationId,
                Role = message.Role.Value,
                Content = message.Text.Trim(),
                TimestampUtc = createdAtLocal,
                Metadata = this.SerializeMetadata(message.AdditionalProperties),
                CreatedAt = createdAtLocal,
                Enabled = true,
                TokenCnt = this.EstimateTokenCount(message)
            });
        }

        return entities;
    }





    private static Guid TryParseMessageId(string? messageId)
    {
        return Guid.TryParse(messageId, out Guid parsedMessageId) ? parsedMessageId : Guid.NewGuid();
    }





    private static bool HasExplicitSourceType(ChatMessage message, AgentRequestMessageSourceType sourceType)
    {
        if (message.AdditionalProperties is null)
        {
            return false;
        }

        if (!message.AdditionalProperties.TryGetValue(AgentRequestMessageSourceAttribution.AdditionalPropertiesKey, out object? value))
        {
            return false;
        }

        return value is AgentRequestMessageSourceAttribution attribution && attribution.SourceType == sourceType;
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
        if (HasExplicitSourceType(message, AgentRequestMessageSourceType.External))
        {
            return false;
        }

        if (HasExplicitSourceType(message, AgentRequestMessageSourceType.AIContextProvider))
        {
            return false;
        }

        return true;
    }
}
