// Copyright (c) Your Organization. All rights reserved.



using System.Collections.ObjectModel;
using System.Text.Json;

using DataIngestionLib.Contracts;

using Microsoft.Agents.AI;
using Microsoft.Agents.Builder;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using KernelChatHistory = Microsoft.SemanticKernel.ChatCompletion.ChatHistory;
// ChatHistoryProvider, ProviderSessionState, AgentSession
// ITurnContext
// ChatMessage, ChatRole
// ChatMessageContent
// ChatHistory, AuthorRole




namespace DataIngestionLib.Providers.ChatHistory;

/// <summary>
/// SQL-backed implementation of <see cref="IChatHistoryProvider"/>.
///
/// All database operations are isolated into <c>protected virtual</c> methods named
/// <c>Sql*Async</c>.  Override those methods in a derived class to supply the actual
/// data-access logic (Dapper, EF Core, ADO.NET, etc.) for your target database engine.
///
/// The public API is fully implemented and production-ready; only the <c>Sql*Async</c>
/// stubs need to be filled in.
/// </summary>
public class SqlChatHistoryProvider : ChatHistoryProvider, IChatHistoryProvider
{
    private readonly IAppSettings _appSettings;
    private readonly SqlChatHistoryOptions _options;
    protected ILogger<SqlChatHistoryProvider> Logger;
    private readonly ProviderSessionState<ProviderState> _sessionState;
    private IReadOnlyList<string>? _stateKeys;

    // Reusable options instance for JSON serialisation of SK metadata.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    public SqlChatHistoryProvider(
        IOptions<SqlChatHistoryOptions> options,
        ILogger<SqlChatHistoryProvider> logger,
        IAppSettings appSettings)
        : base(
            options?.Value.ProvideOutputMessageFilter,
            options?.Value.StoreInputRequestMessageFilter,
            options?.Value.StoreInputResponseMessageFilter)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));

        _sessionState = new ProviderSessionState<ProviderState>(
            _options.StateInitializer ?? CreateProviderState,
            _options.StateKey ?? this.GetType().Name);
    }

    /// <inheritdoc />
    public override IReadOnlyList<string> StateKeys =>
        _stateKeys ??= [_sessionState.StateKey];

    /// <summary>
    /// Session-scoped key payload stored in <see cref="AgentSession.StateBag"/>.
    /// </summary>
    public sealed class ProviderState
    {
        public ProviderState(
            Guid applicationId,
            string userId,
            string agentId,
            string conversationId,
            string channelId,
            string? tenantId = null)
        {
            ApplicationId = applicationId;
            UserId = userId;
            AgentId = agentId;
            ConversationId = conversationId;
            ChannelId = channelId;
            TenantId = tenantId;
        }

        public Guid ApplicationId { get; }
        public string UserId { get; }
        public string AgentId { get; }
        public string ConversationId { get; }
        public string ChannelId { get; }
        public string? TenantId { get; }

        public ChatHistoryKey ToKey() => new(
            ApplicationId,
            UserId ?? string.Empty,
            AgentId ?? string.Empty,
            ConversationId ?? string.Empty,
            ChannelId ?? string.Empty,
            TenantId);

        public static ProviderState CreateDefault() => new(
            Guid.Empty,
            "unknown-user",
            "unknown-agent",
            Guid.NewGuid().ToString("N"),
            "unknown-channel");
    }

    private ProviderState CreateProviderState(AgentSession? session)
    {
        Guid applicationId = Guid.TryParse(_appSettings.ApplicationId, out Guid parsedApplicationId)
            ? parsedApplicationId
            : Guid.Empty;

        string userId = Environment.UserName;
        string agentId = session?.StateBag.GetValue<string>("AgentId") ?? "unknown-agent";
        string conversationId = session?.StateBag.GetValue<string>("ConversationId")
            ?? session?.StateBag.GetValue<string>("SessionId")
            ?? Guid.NewGuid().ToString("N");
        string channelId = session?.StateBag.GetValue<string>("ChannelId") ?? "desktop";
        string? tenantId = session?.StateBag.GetValue<string>("TenantId");

        return new ProviderState(
            applicationId,
            userId,
            agentId,
            conversationId,
            channelId,
            tenantId);
    }

    // ── IChatHistoryProvider: Key building ────────────────────────────────────

    /// <inheritdoc/>
    public ChatHistoryKey BuildKey(ITurnContext turnContext, Guid applicationId, string agentId)
    {
        ArgumentNullException.ThrowIfNull(turnContext);
        ArgumentException.ThrowIfNullOrWhiteSpace(agentId);

        var activity = turnContext.Activity;

        if (string.IsNullOrWhiteSpace(activity.From?.Id))
            throw new InvalidOperationException(
                "Cannot build a ChatHistoryKey: Activity.From.Id is null or empty. " +
                "Ensure the channel populates the sender identity before calling this method.");

        if (string.IsNullOrWhiteSpace(activity.Conversation?.Id))
            throw new InvalidOperationException(
                "Cannot build a ChatHistoryKey: Activity.Conversation.Id is null or empty.");

        string userId = activity.From?.Id?.ToString() ?? string.Empty;
        string conversationId = activity.Conversation?.Id?.ToString() ?? string.Empty;
        string channelId = activity.ChannelId?.ToString() ?? string.Empty;
        string? tenantId = activity.Conversation?.TenantId;

        return new ChatHistoryKey(
            ApplicationId: applicationId,
            UserId:         userId,
            AgentId:        agentId,
            ConversationId: conversationId,
            ChannelId:      channelId,
                TenantId:       tenantId);
    }

    // ── IChatHistoryProvider: Load ────────────────────────────────────────────

    /// <inheritdoc/>
    public Task<KernelChatHistory> LoadHistoryAsync(
        ITurnContext turnContext,
        Guid applicationId,
        string agentId,
        CancellationToken cancellationToken = default)
    {
        var key = BuildKey(turnContext, applicationId, agentId);
        return LoadHistoryAsync(key, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<KernelChatHistory> LoadHistoryAsync(
        ChatHistoryKey key,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);

        Logger.LogDebug("Loading chat history for {Key}", key);

        IReadOnlyList<ChatHistoryRecord> records =
            await SqlLoadMessagesAsync(key, cancellationToken).ConfigureAwait(false);

        var history = new KernelChatHistory();

        foreach (ChatHistoryRecord record in records)
        {
            AuthorRole role = ParseRole(record.Role);
            var message = new ChatMessageContent(role, record.Content)
            {
                AuthorName = record.AuthorName,
                ModelId    = record.ModelId,
            };

            // Restore SK-level metadata when available (token counts, finish reason, etc.)
            if (!string.IsNullOrWhiteSpace(record.MetadataJson))
            {
                try
                {
                    var meta = JsonSerializer.Deserialize<Dictionary<string, object?>>(
                        record.MetadataJson, JsonOptions);

                    if (meta is { Count: > 0 })
                    {
                        foreach (var (k, v) in meta)
                            message.Metadata ??= new Dictionary<string, object?>();
                        // ChatMessageContent.Metadata is read-only after construction;
                        // re-create the message with metadata supplied via the constructor.
                        message = new ChatMessageContent(
                            role,
                            record.Content,
                            modelId:    record.ModelId,
                            metadata:   new ReadOnlyDictionary<string, object?>(meta));
                        message.AuthorName = record.AuthorName;
                    }
                }
                catch (JsonException ex)
                {
                    Logger.LogWarning(ex,
                        "Failed to deserialise MetadataJson for RowId={RowId}", record.RowId);
                }
            }

            history.Add(message);
        }

        Logger.LogDebug("Loaded {Count} messages for {Key}", history.Count, key);
        return history;
    }

    // ── IChatHistoryProvider: Append ──────────────────────────────────────────

    /// <inheritdoc/>
    public Task AppendMessageAsync(
        ITurnContext turnContext,
        Guid applicationId,
        string agentId,
        ChatMessageContent message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(turnContext);
        ArgumentNullException.ThrowIfNull(message);

        var key      = BuildKey(turnContext, applicationId, agentId);
        var metadata = ActivityMetadata.FromTurnContext(turnContext);
        return AppendMessageAsync(key, message, metadata, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task AppendMessageAsync(
        ChatHistoryKey key,
        ChatMessageContent message,
        ActivityMetadata? activityMetadata = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(message);

        int nextSeq = await SqlGetNextSequenceNumberAsync(key, cancellationToken)
            .ConfigureAwait(false);

        ChatHistoryRecord record = MapToRecord(key, message, activityMetadata, nextSeq);
        await SqlInsertMessageAsync(record, cancellationToken).ConfigureAwait(false);

        Logger.LogDebug(
            "Appended {Role} message (seq {Seq}) for {Key}",
            message.Role.Label, nextSeq, key);
    }

    // ── IChatHistoryProvider: Save (full replace) ──────────────────────────────

    /// <inheritdoc/>
    public async Task SaveHistoryAsync(
        ChatHistoryKey key,
        KernelChatHistory history,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(history);

        Logger.LogDebug(
            "Saving {Count} messages for {Key} (full replace)", history.Count, key);

        await SqlDeleteHistoryAsync(key, cancellationToken).ConfigureAwait(false);

        int seq = 0;
        foreach (ChatMessageContent message in history)
        {
            ChatHistoryRecord record = MapToRecord(key, message, meta: null, seq++);
            await SqlInsertMessageAsync(record, cancellationToken).ConfigureAwait(false);
        }
    }

    // ── IChatHistoryProvider: Delete ──────────────────────────────────────────

    /// <inheritdoc/>
    public Task DeleteHistoryAsync(ChatHistoryKey key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        Logger.LogInformation("Deleting chat history for {Key}", key);
        return SqlDeleteHistoryAsync(key, cancellationToken);
    }

    /// <inheritdoc/>
    public Task DeleteUserHistoryAsync(
        Guid applicationId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        Logger.LogInformation(
            "Deleting all history for user {UserId} in app {ApplicationId}",
            userId, applicationId);
        return SqlDeleteUserHistoryAsync(applicationId, userId, cancellationToken);
    }

    // ── IChatHistoryProvider: Query ───────────────────────────────────────────

    /// <inheritdoc/>
    public Task<IReadOnlyList<string>> ListConversationIdsAsync(
        Guid applicationId,
        string userId,
        string agentId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(agentId);
        return SqlListConversationIdsAsync(applicationId, userId, agentId, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<ChatHistoryKey>> ListUserConversationsAsync(
        Guid applicationId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        return SqlListUserConversationsAsync(applicationId, userId, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<ChatHistoryKey>> ListAgentConversationsAsync(
        Guid applicationId,
        string agentId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agentId);
        return SqlListAgentConversationsAsync(applicationId, agentId, cancellationToken);
    }

    // ── Agent Framework ChatHistoryProvider overrides ────────────────────────

    /// <inheritdoc />
    protected override async ValueTask<IEnumerable<ChatMessage>> ProvideChatHistoryAsync(
        InvokingContext context,
        CancellationToken cancellationToken = default)
    {
        var state = _sessionState.GetOrInitializeState(context.Session);
        ChatHistoryKey key = state.ToKey();

        IReadOnlyList<ChatHistoryRecord> records =
            await SqlLoadMessagesAsync(key, cancellationToken).ConfigureAwait(false);

        return records
            .OrderBy(r => r.SequenceNumber)
            .Select(MapToAgentFrameworkMessage)
            .ToList();
    }

    /// <inheritdoc />
    protected override async ValueTask StoreChatHistoryAsync(
        InvokedContext context,
        CancellationToken cancellationToken = default)
    {
        var state = _sessionState.GetOrInitializeState(context.Session);
        ChatHistoryKey key = state.ToKey();

        int seq = await SqlGetNextSequenceNumberAsync(key, cancellationToken)
            .ConfigureAwait(false);

        IEnumerable<ChatMessage> allNewMessages =
            context.RequestMessages.Concat(context.ResponseMessages ?? []);

        foreach (ChatMessage message in allNewMessages)
        {
            ChatHistoryRecord record = MapToRecord(key, message, seq++);
            await SqlInsertMessageAsync(record, cancellationToken).ConfigureAwait(false);
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // SQL stub methods — override in a derived class with your data-access library
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Loads all <see cref="ChatHistoryRecord"/> rows for <paramref name="key"/>,
    /// ordered ascending by <c>SequenceNumber</c>.
    ///
    /// Suggested query (SQL Server / Azure SQL):
    /// <code>
    /// SELECT *
    /// FROM   ChatHistoryMessages
    /// WHERE  ApplicationId  = @ApplicationId
    ///   AND  UserId         = @UserId
    ///   AND  AgentId        = @AgentId
    ///   AND  ConversationId = @ConversationId
    /// ORDER BY SequenceNumber ASC;
    /// </code>
    ///
    /// Dapper example:
    /// <code>
    /// await using var conn = new SqlConnection(_options.ConnectionString);
    /// var rows = await conn.QueryAsync&lt;ChatHistoryRecord&gt;(sql, new {
    ///     key.ApplicationId, key.UserId, key.AgentId, key.ConversationId
    /// }, commandTimeout: _options.CommandTimeoutSeconds);
    /// return rows.ToList();
    /// </code>
    /// </summary>
    protected virtual Task<IReadOnlyList<ChatHistoryRecord>> SqlLoadMessagesAsync(
        ChatHistoryKey key,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException(
            "Override SqlLoadMessagesAsync in a derived class with your database logic.");
    }

    /// <summary>
    /// Inserts a single <see cref="ChatHistoryRecord"/> row.
    ///
    /// Suggested query (SQL Server / Azure SQL):
    /// <code>
    /// INSERT INTO ChatHistoryMessages (
    ///     ApplicationId, UserId, AgentId, ConversationId, ChannelId, TenantId,
    ///     Role, Content, AuthorName, ModelId, SequenceNumber, CreatedAt,
    ///     ActivityId, ActivityType,
    ///     FromId, FromName, FromRole, FromAadObjectId,
    ///     RecipientId, RecipientName, RecipientRole,
    ///     ReplyToId, Locale, TextFormat, InputHint, ServiceUrl, ActivityTimestamp,
    ///     ConversationName, ConversationType, IsGroupConversation,
    ///     EntitiesJson, MetadataJson)
    /// VALUES (
    ///     @ApplicationId, @UserId, @AgentId, @ConversationId, @ChannelId, @TenantId,
    ///     @Role, @Content, @AuthorName, @ModelId, @SequenceNumber, @CreatedAt,
    ///     @ActivityId, @ActivityType,
    ///     @FromId, @FromName, @FromRole, @FromAadObjectId,
    ///     @RecipientId, @RecipientName, @RecipientRole,
    ///     @ReplyToId, @Locale, @TextFormat, @InputHint, @ServiceUrl, @ActivityTimestamp,
    ///     @ConversationName, @ConversationType, @IsGroupConversation,
    ///     @EntitiesJson, @MetadataJson);
    /// </code>
    /// </summary>
    protected virtual Task SqlInsertMessageAsync(
        ChatHistoryRecord record,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException(
            "Override SqlInsertMessageAsync in a derived class with your database logic.");
    }

    /// <summary>
    /// Returns the next sequence number to use when appending a message to <paramref name="key"/>.
    ///
    /// Suggested query (SQL Server / Azure SQL):
    /// <code>
    /// SELECT COALESCE(MAX(SequenceNumber) + 1, 0)
    /// FROM   ChatHistoryMessages
    /// WHERE  ApplicationId  = @ApplicationId
    ///   AND  UserId         = @UserId
    ///   AND  AgentId        = @AgentId
    ///   AND  ConversationId = @ConversationId;
    /// </code>
    ///
    /// In high-concurrency scenarios consider using a database sequence or
    /// wrapping the MAX + INSERT in a serialisable transaction.
    /// </summary>
    protected virtual Task<int> SqlGetNextSequenceNumberAsync(
        ChatHistoryKey key,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException(
            "Override SqlGetNextSequenceNumberAsync in a derived class with your database logic.");
    }

    /// <summary>
    /// Deletes all messages for a single conversation identified by <paramref name="key"/>.
    ///
    /// Suggested query (SQL Server / Azure SQL):
    /// <code>
    /// DELETE FROM ChatHistoryMessages
    /// WHERE  ApplicationId  = @ApplicationId
    ///   AND  UserId         = @UserId
    ///   AND  AgentId        = @AgentId
    ///   AND  ConversationId = @ConversationId;
    /// </code>
    /// </summary>
    protected virtual Task SqlDeleteHistoryAsync(
        ChatHistoryKey key,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException(
            "Override SqlDeleteHistoryAsync in a derived class with your database logic.");
    }

    /// <summary>
    /// Deletes all conversation messages for a user across all agents within an application.
    /// Used for GDPR / right-to-erasure requests.
    ///
    /// Suggested query (SQL Server / Azure SQL):
    /// <code>
    /// DELETE FROM ChatHistoryMessages
    /// WHERE  ApplicationId = @ApplicationId
    ///   AND  UserId        = @UserId;
    /// </code>
    /// </summary>
    protected virtual Task SqlDeleteUserHistoryAsync(
        Guid applicationId,
        string userId,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException(
            "Override SqlDeleteUserHistoryAsync in a derived class with your database logic.");
    }

    /// <summary>
    /// Returns distinct <c>ConversationId</c> values for a user/agent pair.
    ///
    /// Suggested query (SQL Server / Azure SQL):
    /// <code>
    /// SELECT DISTINCT ConversationId
    /// FROM   ChatHistoryMessages
    /// WHERE  ApplicationId = @ApplicationId
    ///   AND  UserId        = @UserId
    ///   AND  AgentId       = @AgentId
    /// ORDER BY ConversationId;
    /// </code>
    /// </summary>
    protected virtual Task<IReadOnlyList<string>> SqlListConversationIdsAsync(
        Guid applicationId,
        string userId,
        string agentId,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException(
            "Override SqlListConversationIdsAsync in a derived class with your database logic.");
    }

    /// <summary>
    /// Returns fully-qualified <see cref="ChatHistoryKey"/> objects for every distinct
    /// conversation a user has participated in, ordered by the most recently active.
    ///
    /// Suggested query (SQL Server / Azure SQL):
    /// <code>
    /// SELECT   ApplicationId, UserId, AgentId, ConversationId, ChannelId,
    ///          TenantId, MAX(CreatedAt) AS LastActivity
    /// FROM     ChatHistoryMessages
    /// WHERE    ApplicationId = @ApplicationId
    ///   AND    UserId        = @UserId
    /// GROUP BY ApplicationId, UserId, AgentId, ConversationId, ChannelId, TenantId
    /// ORDER BY MAX(CreatedAt) DESC;
    /// </code>
    /// </summary>
    protected virtual Task<IReadOnlyList<ChatHistoryKey>> SqlListUserConversationsAsync(
        Guid applicationId,
        string userId,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException(
            "Override SqlListUserConversationsAsync in a derived class with your database logic.");
    }

    /// <summary>
    /// Returns fully-qualified <see cref="ChatHistoryKey"/> objects for every distinct
    /// conversation an agent has handled, ordered by the most recently active.
    ///
    /// Suggested query (SQL Server / Azure SQL):
    /// <code>
    /// SELECT   ApplicationId, UserId, AgentId, ConversationId, ChannelId,
    ///          TenantId, MAX(CreatedAt) AS LastActivity
    /// FROM     ChatHistoryMessages
    /// WHERE    ApplicationId = @ApplicationId
    ///   AND    AgentId       = @AgentId
    /// GROUP BY ApplicationId, UserId, AgentId, ConversationId, ChannelId, TenantId
    /// ORDER BY MAX(CreatedAt) DESC;
    /// </code>
    /// </summary>
    protected virtual Task<IReadOnlyList<ChatHistoryKey>> SqlListAgentConversationsAsync(
        Guid applicationId,
        string agentId,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException(
            "Override SqlListAgentConversationsAsync in a derived class with your database logic.");
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static ChatHistoryRecord MapToRecord(
        ChatHistoryKey key,
        ChatMessageContent message,
        ActivityMetadata? meta,
        int sequenceNumber)
    {
        string? metadataJson = null;
        if (message.Metadata is { Count: > 0 })
        {
            try { metadataJson = JsonSerializer.Serialize(message.Metadata, JsonOptions); }
            catch { /* Non-serialisable metadata is silently dropped. */ }
        }

        return new ChatHistoryRecord
        {
            // Composite key
            ApplicationId   = key.ApplicationId,
            UserId          = key.UserId,
            AgentId         = key.AgentId,
            ConversationId  = key.ConversationId,
            ChannelId       = key.ChannelId,
            TenantId        = key.TenantId,

            // Message body
            Role            = message.Role.Label,
            Content         = message.Content ?? string.Empty,
            AuthorName      = message.AuthorName,
            ModelId         = message.ModelId,
            SequenceNumber  = sequenceNumber,
            CreatedAt       = DateTimeOffset.UtcNow,

            // Activity metadata (built-in properties)
            ActivityId        = meta?.ActivityId,
            ActivityType      = meta?.ActivityType,
            ActivityTimestamp = meta?.Timestamp,
            ServiceUrl        = meta?.ServiceUrl,

            FromId            = meta?.FromId,
            FromName          = meta?.FromName,
            FromRole          = meta?.FromRole,
            FromAadObjectId   = meta?.FromAadObjectId,

            RecipientId       = meta?.RecipientId,
            RecipientName     = meta?.RecipientName,
            RecipientRole     = meta?.RecipientRole,

            ReplyToId         = meta?.ReplyToId,
            Locale            = meta?.Locale,
            TextFormat        = meta?.TextFormat,
            InputHint         = meta?.InputHint,

            ConversationName  = meta?.ConversationName,
            ConversationType  = meta?.ConversationType,
            IsGroupConversation = meta?.IsGroupConversation,

            EntitiesJson    = meta?.EntitiesJson,
            MetadataJson    = metadataJson,
        };
    }

    private static ChatHistoryRecord MapToRecord(
        ChatHistoryKey key,
        ChatMessage message,
        int sequenceNumber)
    {
        string? metadataJson = null;
        if (message.AdditionalProperties is { Count: > 0 })
        {
            try { metadataJson = JsonSerializer.Serialize(message.AdditionalProperties, JsonOptions); }
            catch { /* Non-serialisable metadata is silently dropped. */ }
        }

        return new ChatHistoryRecord
        {
            ApplicationId = key.ApplicationId,
            UserId = key.UserId,
            AgentId = key.AgentId,
            ConversationId = key.ConversationId,
            ChannelId = key.ChannelId,
            TenantId = key.TenantId,

            Role = message.Role.Value,
            Content = message.Text ?? string.Empty,
            AuthorName = message.AuthorName,
            SequenceNumber = sequenceNumber,
            CreatedAt = DateTimeOffset.UtcNow,

            ActivityId = message.MessageId,
            ActivityTimestamp = message.CreatedAt,
            MetadataJson = metadataJson,
        };
    }

    private static ChatMessage MapToAgentFrameworkMessage(ChatHistoryRecord record)
    {
        var message = new ChatMessage(ParseChatRole(record.Role), record.Content)
        {
            AuthorName = record.AuthorName,
            MessageId = record.ActivityId,
            CreatedAt = record.ActivityTimestamp ?? record.CreatedAt,
            RawRepresentation = record.MetadataJson,
        };

        return message;
    }

    private static AuthorRole ParseRole(string roleLabel) =>
        roleLabel.ToLowerInvariant() switch
        {
            "user"      => AuthorRole.User,
            "assistant" => AuthorRole.Assistant,
            "system"    => AuthorRole.System,
            "tool"      => AuthorRole.Tool,
            _           => AuthorRole.User,
        };

    private static ChatRole ParseChatRole(string roleLabel) =>
        roleLabel.ToLowerInvariant() switch
        {
            "user" => ChatRole.User,
            "assistant" => ChatRole.Assistant,
            "system" => ChatRole.System,
            "tool" => ChatRole.Tool,
            _ => ChatRole.User,
        };
}
