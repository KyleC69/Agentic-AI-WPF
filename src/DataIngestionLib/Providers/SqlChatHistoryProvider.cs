// Build Date: 2026/03/19
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         SqlChatHistoryProvider.cs
// Author: Kyle L. Crowder
// Build Num: 044250



using System.Reflection;
using System.Text.Json;

using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Data;
using DataIngestionLib.History.HistoryModels;
using DataIngestionLib.Models;
using DataIngestionLib.Services;

using Microsoft.Agents.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;




namespace DataIngestionLib.Providers;





public sealed class SqlChatHistoryProvider : ChatHistoryProvider, ISQLChatHistoryProvider, IDisposable
{
    private readonly IAppSettings _appSettings;

    private readonly IDbContextFactory<AIChatHistoryDb>? _dbContextFactory;
    private readonly SemaphoreSlim _initializationGate = new(1, 1);
    private readonly ILogger<SqlChatHistoryProvider> _logger;
    private int _isInitialized;
    private const int ConversationWindowMessageLimit = 80;
    private const int DefaultSqlCommandTimeoutSeconds = 300;
    private const string FallbackAgentId = "unknown-agent";
    private const string FallbackApplicationId = "unknown-application";
    private const string FallbackConnectionString = "Server=.;Database=AIChatHistory;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;";
    private const int MaxIdentityLength = 128;
    private const int MaxRoleLength = 32;

    private static readonly HashSet<AgentRequestMessageSourceType> IgnoredRequestSourceTypes =
    [
            AgentRequestMessageSourceType.ChatHistory
    ];








    public SqlChatHistoryProvider(
            ILogger<SqlChatHistoryProvider> logger,
            IAppSettings appSettings,
            IDbContextFactory<AIChatHistoryDb>? dbContextFactory = null)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(appSettings);

        _logger = logger;
        _appSettings = appSettings;
        _dbContextFactory = dbContextFactory;
    }








    protected override async ValueTask InvokedCoreAsync(InvokedContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

            var conversationId = ChatHistorySessionState.GetOrCreateConversationId(context.Session);

            var fallbackAgentId = GetSessionStateValue(context.Session, "AgentId", FallbackAgentId);
            var fallbackApplicationId = GetSessionStateValue(context.Session, "ApplicationId", _appSettings.ApplicationId);
            var fallbackUserId = GetSessionStateValue(context.Session, "UserId", _appSettings.UserId);

            var agentId = ChatHistorySessionState.GetOrCreateAgentId(context.Session, fallbackAgentId);
            var applicationId = ChatHistorySessionState.GetOrCreateApplicationId(context.Session, fallbackApplicationId);
            var userId = ChatHistorySessionState.GetOrCreateUserId(context.Session, fallbackUserId);

            IReadOnlyList<ChatMessage> requestMessages = FilterRequestMessages(context.RequestMessages).ToArray();
            var responseMessages = GetContextMessages(context, "ResponseMessages");
            if (responseMessages.Count == 0)
            {
                responseMessages = GetContextMessages(context, "ResultMessages");
            }

            if (requestMessages.Count == 0 && responseMessages.Count == 0)
            {
                return;
            }

            await PersistInteractionAsync(
                            conversationId,
                            agentId,
                            userId,
                            applicationId,
                            requestMessages,
                            responseMessages,
                            cancellationToken)
                    .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to persist invoked chat interaction. Continuing without failing the active agent run.");
        }
    }








    protected override async ValueTask<IEnumerable<ChatMessage>> InvokingCoreAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

            var conversationId = ChatHistorySessionState.GetOrCreateConversationId(context.Session);
            var persistedMessages = await GetMessagesAsync(conversationId, ConversationWindowMessageLimit, cancellationToken).ConfigureAwait(false);
            if (persistedMessages.Count == 0)
            {
                return [];
            }

            HashSet<string> existingMessageSourceIds = new(StringComparer.OrdinalIgnoreCase);
            foreach (ChatMessage requestMessage in context.RequestMessages ?? [])
            {
                var sourceId = requestMessage.GetAgentRequestMessageSourceId();
                if (!string.IsNullOrWhiteSpace(sourceId))
                {
                    _ = existingMessageSourceIds.Add(sourceId);
                }
            }

            List<ChatMessage> historyContextMessages = [];
            foreach (PersistedChatMessage persistedMessage in persistedMessages)
            {
                if (string.IsNullOrWhiteSpace(persistedMessage.Content))
                {
                    continue;
                }

                var sourceId = persistedMessage.MessageId.ToString("D");
                if (existingMessageSourceIds.Contains(sourceId))
                {
                    continue;
                }

                ChatMessage contextMessage = new(ParseRole(persistedMessage.Role), persistedMessage.Content)
                {
                        CreatedAt = persistedMessage.TimestampUtc,
                        MessageId = sourceId
                };

                historyContextMessages.Add(contextMessage.WithAgentRequestMessageSource(AgentRequestMessageSourceType.ChatHistory, sourceId));
            }

            return historyContextMessages;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to provide SQL-backed chat history context. Continuing with an empty history window.");
            return [];
        }
    }








    public async ValueTask<PersistedChatMessage> CreateMessageAsync(PersistedChatMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        cancellationToken.ThrowIfCancellationRequested();

        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

        await using AIChatHistoryDb dbContext = await CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        ChatHistoryMessage entity = ToEntity(message);

        await dbContext.ChatHistoryMessages.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return ToPersisted(entity);
    }








    public async ValueTask<int> DeleteConversationAsync(string conversationId, CancellationToken cancellationToken = default)
    {
        var normalizedConversationId = NormalizeIdentity(conversationId, nameof(conversationId));
        cancellationToken.ThrowIfCancellationRequested();

        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

        await using AIChatHistoryDb dbContext = await CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        return await dbContext.ChatHistoryMessages
                .Where(message => message.ConversationId == normalizedConversationId)
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);
    }








    public async ValueTask<bool> DeleteMessageAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        if (messageId == Guid.Empty)
        {
            throw new ArgumentException("Message ID must be a non-empty GUID.", nameof(messageId));
        }

        cancellationToken.ThrowIfCancellationRequested();

        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

        await using AIChatHistoryDb dbContext = await CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var affectedRows = await dbContext.ChatHistoryMessages
                .Where(message => message.MessageId == messageId)
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);

        return affectedRows > 0;
    }








    public async ValueTask EnsureInitializedAsync(CancellationToken cancellationToken = default)
    {
        if (Volatile.Read(ref _isInitialized) == 1)
        {
            return;
        }

        await _initializationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (Volatile.Read(ref _isInitialized) == 1)
            {
                return;
            }

            await using AIChatHistoryDb dbContext = await CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

            Volatile.Write(ref _isInitialized, 1);
            _logger.LogDebug("SQL chat history provider initialized successfully.");
        }
        finally
        {
            _initializationGate.Release();
        }
    }








    public async ValueTask<string?> GetLatestConversationIdAsync(
            string agentId,
            string userId,
            string applicationId,
            CancellationToken cancellationToken = default)
    {
        string normalizedAgentId = NormalizeIdentity(agentId, nameof(agentId));
        string normalizedUserId = NormalizeIdentity(userId, nameof(userId));
        string normalizedApplicationId = NormalizeIdentity(applicationId, nameof(applicationId));
        cancellationToken.ThrowIfCancellationRequested();

        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

        await using AIChatHistoryDb dbContext = await CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var latest = await dbContext.ChatHistoryMessages
                .AsNoTracking()
            .Where(message => message.AgentId == normalizedAgentId)
            .Where(message => message.UserId == normalizedUserId)
            .Where(message => message.ApplicationId == normalizedApplicationId)
            .Where(message => message.ConversationId != string.Empty)
                .OrderByDescending(message => message.TimestampUtc)
                .ThenByDescending(message => message.CreatedAt)
            .Select(message => message.ConversationId)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

        return string.IsNullOrWhiteSpace(latest) ? null : latest;
    }








    public async ValueTask<PersistedChatMessage?> GetMessageAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        if (messageId == Guid.Empty)
        {
            throw new ArgumentException("Message ID must be a non-empty GUID.", nameof(messageId));
        }

        cancellationToken.ThrowIfCancellationRequested();

        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

        await using AIChatHistoryDb dbContext = await CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        ChatHistoryMessage? entity = await dbContext.ChatHistoryMessages
                .AsNoTracking()
                .FirstOrDefaultAsync(message => message.MessageId == messageId, cancellationToken)
                .ConfigureAwait(false);

        return entity is null ? null : ToPersisted(entity);
    }








    public async ValueTask<IReadOnlyList<PersistedChatMessage>> GetMessagesAsync(string conversationId, int? take, CancellationToken cancellationToken = default)
    {
        var normalizedConversationId = NormalizeIdentity(conversationId, nameof(conversationId));
        if (take is <= 0)
        {
            return [];
        }

        cancellationToken.ThrowIfCancellationRequested();
        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

        await using AIChatHistoryDb dbContext = await CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        IQueryable<ChatHistoryMessage> ordered = dbContext.ChatHistoryMessages
                .AsNoTracking()
                .Where(message => message.ConversationId == normalizedConversationId)
                .OrderByDescending(message => message.TimestampUtc)
                .ThenByDescending(message => message.CreatedAt);

        if (take.HasValue)
        {
            ordered = ordered.Take(take.Value);
        }

        List<ChatHistoryMessage> entities = await ordered
            .OrderBy(message => message.TimestampUtc)
            .ThenBy(message => message.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<PersistedChatMessage> messages = entities
            .Select(ToPersisted)
            .ToList();

        return messages;
    }








    public async ValueTask<PersistedChatMessage?> UpdateMessageAsync(Guid messageId, string content, DateTimeOffset timestampUtc, CancellationToken cancellationToken = default)
    {
        if (messageId == Guid.Empty)
        {
            throw new ArgumentException("Message ID must be a non-empty GUID.", nameof(messageId));
        }

        var normalizedContent = NormalizeContent(content, nameof(content));
        cancellationToken.ThrowIfCancellationRequested();
        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

        await using AIChatHistoryDb dbContext = await CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        ChatHistoryMessage? entity = await dbContext.ChatHistoryMessages
                .FirstOrDefaultAsync(message => message.MessageId == messageId, cancellationToken)
                .ConfigureAwait(false);
        if (entity is null)
        {
            return null;
        }

        entity.Content = normalizedContent;
        entity.TimestampUtc = timestampUtc;

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ToPersisted(entity);
    }















    private async ValueTask<AIChatHistoryDb> CreateDbContextAsync(CancellationToken cancellationToken)
    {
        if (_dbContextFactory is not null)
        {
            return await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        }

        DbContextOptionsBuilder<AIChatHistoryDb> optionsBuilder = new();
        _ = optionsBuilder.UseSqlServer(
                ResolveConnectionString(),
                sqlOptions => sqlOptions.CommandTimeout(DefaultSqlCommandTimeoutSeconds));

        return new AIChatHistoryDb(optionsBuilder.Options);
    }








    private static IReadOnlyList<ChatMessage> FilterRequestMessages(IEnumerable<ChatMessage>? requestMessages)
    {
        if (requestMessages is null)
        {
            return [];
        }

        return requestMessages
                .Where(message => !IgnoredRequestSourceTypes.Contains(message.GetAgentRequestMessageSourceType()))
                .Where(message => !string.IsNullOrWhiteSpace(message.Text))
                .ToArray();
    }








    private static IReadOnlyList<ChatMessage> GetContextMessages(object context, string propertyName)
    {
        PropertyInfo? property = context.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property?.GetValue(context) is IEnumerable<ChatMessage> messages)
        {
            return messages.Where(message => !string.IsNullOrWhiteSpace(message.Text)).ToArray();
        }

        return [];
    }








    private static string GetSessionStateValue(AgentSession? session, string key, string fallback)
    {
        if (session?.StateBag is not null
            && session.StateBag.TryGetValue(key, out string? stateValue)
            && !string.IsNullOrWhiteSpace(stateValue))
        {
            return stateValue.Trim();
        }

        if (!string.IsNullOrWhiteSpace(fallback))
        {
            return fallback.Trim();
        }

        return key.Equals("UserId", StringComparison.OrdinalIgnoreCase)
                ? Environment.UserName
                : "unknown";
    }








    private static string NormalizeContent(string content, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Message content cannot be empty.", parameterName);
        }

        return content.Trim();
    }








    private static string NormalizeIdentity(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} cannot be empty.", parameterName);
        }

        var normalized = value.Trim();
        if (normalized.Length > MaxIdentityLength)
        {
            throw new ArgumentOutOfRangeException(parameterName, $"{parameterName} exceeds the maximum length of {MaxIdentityLength} characters.");
        }

        return normalized;
    }








    private string NormalizeRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return ChatRole.User.Value;
        }

        var normalized = role.Trim();
        if (normalized.Length > MaxRoleLength)
        {
            throw new ArgumentOutOfRangeException(nameof(role), $"Role exceeds the maximum length of {MaxRoleLength} characters.");
        }

        return normalized;
    }








    private JsonDocument? ParseMetadata(string? metadataJson, Guid messageId)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
        {
            return null;
        }

        try
        {
            return JsonDocument.Parse(metadataJson);
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(exception, "Invalid metadata JSON was found for chat history message {MessageId}. Metadata will be ignored.", messageId);
            return null;
        }
    }








    private ChatRole ParseRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return ChatRole.User;
        }

        var normalized = role.Trim();
        if (normalized.Length > MaxRoleLength)
        {
            _logger.LogWarning("Role value '{Role}' exceeds max length {MaxLength}. Falling back to user role.", normalized, MaxRoleLength);
            return ChatRole.User;
        }

        return new ChatRole(normalized);
    }








    private async ValueTask PersistInteractionAsync(
            string conversationId,
            string agentId,
            string userId,
            string applicationId,
            IReadOnlyList<ChatMessage> requestMessages,
            IReadOnlyList<ChatMessage> responseMessages,
            CancellationToken cancellationToken)
    {
        await using AIChatHistoryDb dbContext = await CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        List<ChatHistoryMessage> entities = [];
        entities.AddRange(ToEntities(requestMessages, conversationId, agentId, userId, applicationId));
        entities.AddRange(ToEntities(responseMessages, conversationId, agentId, userId, applicationId));

        if (entities.Count == 0)
        {
            return;
        }

        await dbContext.ChatHistoryMessages.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }








    private string ResolveConnectionString()
    {
        var configured = _appSettings.ChatHistoryConnectionString?.Trim() ?? string.Empty;
        return string.IsNullOrWhiteSpace(configured) ? FallbackConnectionString : configured;
    }








    private static string RoleToString(ChatRole role)
    {
        var value = role.Value?.Trim() ?? string.Empty;
        if (value.Length == 0)
        {
            return ChatRole.User.Value;
        }

        return value.Length <= MaxRoleLength ? value : value[..MaxRoleLength];
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








    private List<ChatHistoryMessage> ToEntities(
            IEnumerable<ChatMessage> messages,
            string conversationId,
            string agentId,
            string userId,
            string applicationId)
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
            if (!seenMessageIds.Add(messageId))
            {
                continue;
            }

            entities.Add(new ChatHistoryMessage
            {
                    MessageId = messageId,
                    ConversationId = NormalizeIdentity(conversationId, nameof(conversationId)),
                    AgentId = NormalizeIdentity(agentId, nameof(agentId)),
                    UserId = NormalizeIdentity(userId, nameof(userId)),
                    ApplicationId = NormalizeIdentity(applicationId, nameof(applicationId)),
                    Role = RoleToString(message.Role),
                    Content = message.Text.Trim(),
                    TimestampUtc = message.CreatedAt ?? DateTimeOffset.UtcNow,
                    Metadata = SerializeMetadata(message.AdditionalProperties),
                    CreatedAt = DateTime.UtcNow,
                    Enabled = true
            });
        }

        return entities;
    }








    private ChatHistoryMessage ToEntity(PersistedChatMessage message)
    {
        return new ChatHistoryMessage
        {
                MessageId = message.MessageId == Guid.Empty ? Guid.NewGuid() : message.MessageId,
                ConversationId = NormalizeIdentity(message.ConversationId, nameof(message.ConversationId)),
                AgentId = NormalizeIdentity(message.AgentId, nameof(message.AgentId)),
                UserId = NormalizeIdentity(message.UserId, nameof(message.UserId)),
                ApplicationId = NormalizeIdentity(message.ApplicationId, nameof(message.ApplicationId)),
                Role = NormalizeRole(message.Role),
                Content = NormalizeContent(message.Content, nameof(message.Content)),
                TimestampUtc = message.TimestampUtc == default ? DateTimeOffset.UtcNow : message.TimestampUtc,
                Metadata = message.Metadata?.RootElement.GetRawText(),
                CreatedAt = DateTime.UtcNow,
                Enabled = true
        };
    }








    private PersistedChatMessage ToPersisted(ChatHistoryMessage message)
    {
        return new PersistedChatMessage
        {
                MessageId = message.MessageId,
                ConversationId = message.ConversationId,
                AgentId = message.AgentId,
                UserId = message.UserId,
                ApplicationId = message.ApplicationId,
                Role = message.Role,
                Content = message.Content,
                TimestampUtc = message.TimestampUtc,
                Metadata = ParseMetadata(message.Metadata, message.MessageId)
        };
    }








    private static Guid TryParseMessageId(string? messageId)
    {
        return Guid.TryParse(messageId, out Guid parsedMessageId)
                ? parsedMessageId
                : Guid.NewGuid();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}