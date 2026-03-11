// 2026/03/10
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         SqlChatHistoryProvider.cs
//   Author: Kyle L. Crowder



using System.Text.Json;

using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;

using Microsoft.Agents.AI;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.AI;




namespace DataIngestionLib.Services;





/// <summary>
///     This class shows how to implement a SQL-backed chat history provider. Why store chat history in SQL?
///     It simplifies the life cycle hooks, all you need to do is get the last session id used by this agent in this
///     application.
///     Each agent has an Id each agent has a session id and an application can have an id. Why all the Id's?
///     Because you can have multiple agents in the same application and each agent can have multiple sessions.
///     This allows you to easily query the chat history for a specific agent and session, and also allows you to easily
///     clean up
///     old chat history by deleting or deactivating rows from the database that are older than a certain date or that
///     belong to a specific session or agent.
///     How about creating a semantic chat reducer that ensures you are using the most relevant information not only by
///     date but also relevance, that is power!
///     If this is in an application in an enterprise and company policy to save all history for security and safety
///     reasons, you have an onsite SQL server that stores it
///     for each application in the Enterprise, Each agent multiple sessions storage needs exponentially growing. Sound
///     unnecessary?
///     Now when you pair it with an AIContextProvider. Now you can search the entire history of interactions for a
///     specific agent and session to find relevant information amd
///     inject it into the current conversation. Now you have a powerful way to provide context to the agent imaging 100
///     users all providing context sources
///     that can be used at any time to provide relevant information to the agent. This is a powerful way to provide
///     context to the agent and can help improve the quality of the responses provided by the agent.
///     Add vector search capabilities to the database, and now you can grab info that is hyper-relevant to the current
///     conversation and inject it into the conversation to provide even more context to the agent.
/// </summary>
public sealed class SQLChatHistoryProvider : ChatHistoryProvider, ISQLChatHistoryProvider
{

    private readonly ISqlChatHistoryConnectionFactory _connectionFactory;
    private readonly string _defaultAgentId;
    private readonly string _defaultApplicationId;
    private readonly string _defaultUserId;
    private readonly IRuntimeContextAccessor _runtimeContextAccessor;

    private const int ConversationKeyLength = 128;
    private const int DefaultHistoryWindowSize = 200;
    private const int PartitionKeyLength = 128;
    private const int RoleLength = 32;








    public SQLChatHistoryProvider(ISqlChatHistoryConnectionFactory connectionFactory, IRuntimeContextAccessor runtimeContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        ArgumentNullException.ThrowIfNull(runtimeContextAccessor);
        _connectionFactory = connectionFactory;
        _runtimeContextAccessor = runtimeContextAccessor;
        _defaultAgentId = GetType().Name;
        _defaultApplicationId = AppDomain.CurrentDomain.FriendlyName;
        _defaultUserId = Environment.UserName;
    }








    public async ValueTask EnsureInitializedAsync(CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        ;

        foreach ((string? migrationId, string? migrationSql) in ChatHistoryMigrations.All)
        {
            cancellationToken.ThrowIfCancellationRequested();

            bool isApplied = await MigrationAlreadyAppliedAsync(connection, migrationId, cancellationToken).ConfigureAwait(false);
            if (isApplied)
            {
                continue;
            }

            await using SqlTransaction transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            ;

            try
            {
                int unused1 = await ExecuteNonQueryAsync(connection, transaction, migrationSql, [], cancellationToken).ConfigureAwait(false);

                SqlParameter[] insertParameters =
                [
                        new SqlParameter("@MigrationId", migrationId),
                        new SqlParameter("@AppliedOnUtc", DateTimeOffset.UtcNow)
                ];

                const string insertMigrationSql = "INSERT INTO dbo.__ChatHistoryMigrations ([Id], [AppliedOnUtc]) VALUES (@MigrationId, @AppliedOnUtc);";
                int unused = await ExecuteNonQueryAsync(connection, transaction, insertMigrationSql, insertParameters, cancellationToken).ConfigureAwait(false);

                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
        }

        await EnsureEnabledColumnAsync(connection, cancellationToken).ConfigureAwait(false);
    }








    public async ValueTask<PersistedChatMessage> CreateMessageAsync(PersistedChatMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        PersistedChatMessage normalizedMessage = NormalizeMessage(message);

        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

        await using SqlConnection connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        ;
        ;
        await InsertMessageAsync(connection, normalizedMessage, cancellationToken).ConfigureAwait(false);

        return normalizedMessage;
    }








    public async ValueTask<PersistedChatMessage?> GetMessageAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        if (messageId == Guid.Empty)
        {
            throw new ArgumentException("Message id must be a non-empty GUID.", nameof(messageId));
        }

        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

        const string selectSql =
                """
                SELECT [MessageId], [ConversationId], [SessionId], [AgentId], [UserId], [ApplicationId], [Role], [Content], [TimestampUtc], [Metadata]
                FROM dbo.ChatHistoryMessages
                WHERE [MessageId] = @MessageId;
                """;

        await using SqlConnection connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        ;
        ;
        await using SqlCommand command = CreateCommand(connection, null, selectSql, [new SqlParameter("@MessageId", messageId)]);
        ;
        ;
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        ;
        ;

        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false) ? MapMessage(reader) : null;
    }








    public async ValueTask<IReadOnlyList<PersistedChatMessage>> GetMessagesAsync(string conversationId, int? take, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            throw new ArgumentException("Conversation id is required.", nameof(conversationId));
        }

        if (take <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take), "Take must be positive when specified.");
        }

        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

        RuntimeContext runtimeContext = _runtimeContextAccessor.GetCurrent();
        string applicationId = runtimeContext.ApplicationId.ToString("N");
        string userId = GetUserId(runtimeContext);
        bool hasTake = take.HasValue;

        string selectSql = hasTake
                ? """
                  SELECT [MessageId], [ConversationId], [SessionId], [AgentId], [UserId], [ApplicationId], [Role], [Content], [TimestampUtc], [Metadata]
                  FROM (
                      SELECT TOP (@Take)
                          [MessageId], [ConversationId], [SessionId], [AgentId], [UserId], [ApplicationId], [Role], [Content], [TimestampUtc], [Metadata]
                      FROM dbo.ChatHistoryMessages
                      WHERE [ApplicationId] = @ApplicationId
                        AND [UserId] = @UserId
                        AND [ConversationId] = @ConversationId
                        AND [Enabled] = 1
                      ORDER BY [TimestampUtc] DESC, [MessageId] DESC
                  ) AS latest
                  ORDER BY [TimestampUtc] ASC, [MessageId] ASC;
                  """
                : """
                  SELECT [MessageId], [ConversationId], [SessionId], [AgentId], [UserId], [ApplicationId], [Role], [Content], [TimestampUtc], [Metadata]
                  FROM dbo.ChatHistoryMessages
                  WHERE [ApplicationId] = @ApplicationId
                    AND [UserId] = @UserId
                    AND [ConversationId] = @ConversationId
                    AND [Enabled] = 1
                  ORDER BY [TimestampUtc] ASC, [MessageId] ASC;
                  """;

        List<SqlParameter> parameters =
        [
                new SqlParameter("@ApplicationId", applicationId),
                new SqlParameter("@UserId", userId),
                new SqlParameter("@ConversationId", conversationId.Trim())
        ];

        if (hasTake)
        {
            parameters.Add(new SqlParameter("@Take", take!.Value));
        }

        List<PersistedChatMessage> messages = [];

        await using SqlConnection connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        ;
        await using SqlCommand command = CreateCommand(connection, null, selectSql, [.. parameters]);
        ;
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        ;

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            messages.Add(MapMessage(reader));
        }

        return messages;
    }








    public async ValueTask<PersistedChatMessage?> UpdateMessageAsync(Guid messageId, string content, DateTimeOffset timestampUtc, CancellationToken cancellationToken = default)
    {
        if (messageId == Guid.Empty)
        {
            throw new ArgumentException("Message id must be a non-empty GUID.", nameof(messageId));
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Message content cannot be empty.", nameof(content));
        }

        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

        const string updateSql =
                """
                UPDATE dbo.ChatHistoryMessages
                SET [Content] = @Content,
                    [TimestampUtc] = @TimestampUtc
                WHERE [MessageId] = @MessageId;
                """;

        SqlParameter[] updateParameters =
        [
                new SqlParameter("@Content", content.Trim()),
                new SqlParameter("@TimestampUtc", timestampUtc),
                new SqlParameter("@MessageId", messageId)
        ];

        await using SqlConnection connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        ;
        ;
        int affected = await ExecuteNonQueryAsync(connection, null, updateSql, updateParameters, cancellationToken).ConfigureAwait(false);

        return affected == 0
                ? null
                : await GetMessageAsync(messageId, cancellationToken).ConfigureAwait(false);
    }








    public async ValueTask<bool> DeleteMessageAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        if (messageId == Guid.Empty)
        {
            throw new ArgumentException("Message id must be a non-empty GUID.", nameof(messageId));
        }

        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

        const string deleteSql =
                """
                UPDATE dbo.ChatHistoryMessages
                SET [Enabled] = 0
                WHERE [MessageId] = @MessageId;
                """;
        await using SqlConnection connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        ;
        ;

        int affected = await ExecuteNonQueryAsync(connection, null, deleteSql, [new SqlParameter("@MessageId", messageId)], cancellationToken).ConfigureAwait(false);
        return affected > 0;
    }








    public async ValueTask<int> DeleteConversationAsync(string conversationId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            throw new ArgumentException("Conversation id is required.", nameof(conversationId));
        }

        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

        RuntimeContext runtimeContext = _runtimeContextAccessor.GetCurrent();
        string applicationId = runtimeContext.ApplicationId.ToString("N");
        string userId = GetUserId(runtimeContext);

        const string deleteSql =
                """
                UPDATE dbo.ChatHistoryMessages
                SET [Enabled] = 0
                WHERE [ApplicationId] = @ApplicationId
                  AND [UserId] = @UserId
                  AND [ConversationId] = @ConversationId;
                """;
        await using SqlConnection connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        ;
        ;

        return await ExecuteNonQueryAsync(
                        connection,
                        null,
                        deleteSql,
                        [
                                new SqlParameter("@ApplicationId", applicationId),
                                new SqlParameter("@UserId", userId),
                                new SqlParameter("@ConversationId", conversationId.Trim())
                        ],
                        cancellationToken)
                .ConfigureAwait(false);
    }








    public async ValueTask<ChatHistorySessionSnapshot?> GetLatestSessionSnapshotAsync(CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

        RuntimeContext runtimeContext = _runtimeContextAccessor.GetCurrent();
        string applicationId = runtimeContext.ApplicationId.ToString("N");
        string userId = GetUserId(runtimeContext);

        const string selectSql =
                """
                SELECT TOP (1) [ConversationId], [SessionId]
                FROM dbo.ChatHistoryMessages
                WHERE [ApplicationId] = @ApplicationId
                  AND [UserId] = @UserId
                  AND [Enabled] = 1
                ORDER BY [TimestampUtc] DESC, [MessageId] DESC;
                """;

        await using SqlConnection connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        ;
        ;
        await using SqlCommand command = CreateCommand(
                connection,
                null,
                selectSql,
                [
                        new SqlParameter("@ApplicationId", applicationId),
                        new SqlParameter("@UserId", userId)
                ]);
        ;
        ;
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        ;
        ;

        return !await reader.ReadAsync(cancellationToken).ConfigureAwait(false)
                ? null
                : new ChatHistorySessionSnapshot(
                        reader.GetString(0),
                        reader.GetString(1));
    }








    private static SqlCommand CreateCommand(SqlConnection connection, SqlTransaction? transaction, string sql, SqlParameter[] parameters)
    {
        SqlCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        command.CommandType = System.Data.CommandType.Text;
        command.Parameters.AddRange(parameters);
        return command;
    }








    private static async ValueTask EnsureEnabledColumnAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        const string ensureSql =
                """
                IF COL_LENGTH(N'dbo.ChatHistoryMessages', N'Enabled') IS NULL
                BEGIN
                    ALTER TABLE dbo.ChatHistoryMessages ADD [Enabled] bit NOT NULL CONSTRAINT [DF_ChatHistoryMessages_Enabled] DEFAULT(1);
                END
                """;
        _ = await ExecuteNonQueryAsync(connection, null, ensureSql, [], cancellationToken).ConfigureAwait(false);
    }








    private static async ValueTask<int> ExecuteNonQueryAsync(SqlConnection connection, SqlTransaction? transaction, string sql, SqlParameter[] parameters, CancellationToken cancellationToken)
    {
        await using SqlCommand command = CreateCommand(connection, transaction, sql, parameters);
        ;
        ;
        return await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }








    private async ValueTask<IReadOnlyList<PersistedChatMessage>> GetMessagesAsync(ChatHistoryScope scope, int? take, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(scope.ConversationId))
        {
            throw new ArgumentException("Conversation id is required.", nameof(scope));
        }

        if (take <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take), "Take must be positive when specified.");
        }

        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

        bool hasTake = take.HasValue;
        string selectSql = hasTake
                ? """
                  SELECT [MessageId], [ConversationId], [SessionId], [AgentId], [UserId], [ApplicationId], [Role], [Content], [TimestampUtc], [Metadata]
                  FROM (
                      SELECT TOP (@Take)
                          [MessageId], [ConversationId], [SessionId], [AgentId], [UserId], [ApplicationId], [Role], [Content], [TimestampUtc], [Metadata]
                      FROM dbo.ChatHistoryMessages
                      WHERE [ApplicationId] = @ApplicationId
                        AND [UserId] = @UserId
                        AND [ConversationId] = @ConversationId
                        AND [SessionId] = @SessionId
                        AND [AgentId] = @AgentId
                        AND [Enabled] = 1
                      ORDER BY [TimestampUtc] DESC, [MessageId] DESC
                  ) AS latest
                  ORDER BY [TimestampUtc] ASC, [MessageId] ASC;
                  """
                : """
                  SELECT [MessageId], [ConversationId], [SessionId], [AgentId], [UserId], [ApplicationId], [Role], [Content], [TimestampUtc], [Metadata]
                  FROM dbo.ChatHistoryMessages
                  WHERE [ApplicationId] = @ApplicationId
                    AND [UserId] = @UserId
                    AND [ConversationId] = @ConversationId
                    AND [SessionId] = @SessionId
                    AND [AgentId] = @AgentId
                    AND [Enabled] = 1
                  ORDER BY [TimestampUtc] ASC, [MessageId] ASC;
                  """;

        List<SqlParameter> parameters =
        [
                new SqlParameter("@ApplicationId", scope.ApplicationId),
                new SqlParameter("@UserId", scope.UserId),
                new SqlParameter("@ConversationId", scope.ConversationId),
                new SqlParameter("@SessionId", scope.SessionId),
                new SqlParameter("@AgentId", scope.AgentId)
        ];
        if (hasTake)
        {
            parameters.Add(new SqlParameter("@Take", take!.Value));
        }

        List<PersistedChatMessage> messages = [];

        await using SqlConnection connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        ;
        ;
        await using SqlCommand command = CreateCommand(connection, null, selectSql, [.. parameters]);
        ;
        ;
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        ;
        ;

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            messages.Add(MapMessage(reader));
        }

        return messages;
    }








    private string GetUserId(RuntimeContext runtimeContext)
    {
        return !string.IsNullOrWhiteSpace(runtimeContext.UserPrincipalName)
                ? runtimeContext.UserPrincipalName.Trim()
                : !string.IsNullOrWhiteSpace(runtimeContext.DisplayName)
                        ? runtimeContext.DisplayName.Trim()
                        : _defaultUserId;
    }








    private static async ValueTask InsertMessageAsync(SqlConnection connection, PersistedChatMessage message, CancellationToken cancellationToken)
    {
        const string insertSql =
                """
                INSERT INTO dbo.ChatHistoryMessages
                    ([MessageId], [ConversationId], [SessionId], [AgentId], [UserId], [ApplicationId], [Role], [Content], [TimestampUtc], [Metadata], [Enabled])
                VALUES
                    (@MessageId, @ConversationId, @SessionId, @AgentId, @UserId, @ApplicationId, @Role, @Content, @TimestampUtc, @Metadata, @Enabled);
                """;

        SqlParameter[] parameters =
        [
                new SqlParameter("@MessageId", message.MessageId),
                new SqlParameter("@ConversationId", message.ConversationId),
                new SqlParameter("@SessionId", message.SessionId),
                new SqlParameter("@AgentId", message.AgentId),
                new SqlParameter("@UserId", message.UserId),
                new SqlParameter("@ApplicationId", message.ApplicationId),
                new SqlParameter("@Role", message.Role),
                new SqlParameter("@Content", message.Content),
                new SqlParameter("@TimestampUtc", message.TimestampUtc),
                new SqlParameter("@Metadata", message.Metadata?.RootElement.GetRawText() ?? (object)DBNull.Value),
                new SqlParameter("@Enabled", true)
        ];
        _ = await ExecuteNonQueryAsync(connection, null, insertSql, parameters, cancellationToken).ConfigureAwait(false);
    }








    private static PersistedChatMessage MapMessage(SqlDataReader reader)
    {
        string? metadataValue = reader.IsDBNull(9) ? null : reader.GetString(9);
        JsonDocument? metadata = TryParseMetadata(metadataValue, out JsonDocument? parsedMetadata)
                ? parsedMetadata
                : null;

        return new PersistedChatMessage
        {
            MessageId = reader.GetGuid(0),
            ConversationId = reader.GetString(1),
            SessionId = reader.GetString(2),
            AgentId = reader.GetString(3),
            UserId = reader.GetString(4),
            ApplicationId = reader.GetString(5),
            Role = reader.GetString(6),
            Content = reader.GetString(7),
            TimestampUtc = reader.GetDateTimeOffset(8),
            Metadata = metadata
        };
    }








    private static async ValueTask<bool> MigrationAlreadyAppliedAsync(SqlConnection connection, string migrationId, CancellationToken cancellationToken)
    {
        const string sql =
                """
                IF OBJECT_ID(N'dbo.__ChatHistoryMigrations', N'U') IS NULL
                BEGIN
                    SELECT CAST(0 AS bit);
                END
                ELSE
                BEGIN
                    SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.__ChatHistoryMigrations WHERE [Id] = @MigrationId)
                        THEN CAST(1 AS bit)
                        ELSE CAST(0 AS bit)
                    END;
                END
                """;

        await using SqlCommand command = CreateCommand(connection, null, sql, [new SqlParameter("@MigrationId", migrationId)]);
        ;
        ;
        object result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

        return result is bool boolValue && boolValue;
    }








    private static PersistedChatMessage NormalizeMessage(PersistedChatMessage message)
    {
        ValidateMessage(message);

        return message with
        {
            MessageId = message.MessageId == Guid.Empty ? Guid.NewGuid() : message.MessageId,
            ConversationId = message.ConversationId.Trim(),
            SessionId = message.SessionId.Trim(),
            AgentId = message.AgentId.Trim(),
            UserId = message.UserId.Trim(),
            ApplicationId = message.ApplicationId.Trim(),
            Role = message.Role.Trim(),
            Content = message.Content.Trim()
        };
    }








    private static ChatRole ParseRole(string role)
    {
        string normalized = role.Trim();

        return normalized.ToLowerInvariant() switch
        {
            "assistant" => ChatRole.Assistant,
            "user" => ChatRole.User,
            "system" => ChatRole.System,
            "tool" => ChatRole.Tool,
            _ => ChatRole.User
        };
    }








    protected override async ValueTask<IEnumerable<ChatMessage>> ProvideChatHistoryAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        ChatHistoryScope scope = ResolveScope(context.Session);
        IReadOnlyList<PersistedChatMessage> persistedMessages = await GetMessagesAsync(scope, DefaultHistoryWindowSize, cancellationToken).ConfigureAwait(false);

        return persistedMessages.Select(static persistedMessage => new ChatMessage(ParseRole(persistedMessage.Role), persistedMessage.Content));
    }








    private ChatHistoryScope ResolveScope(AgentSession? session)
    {
        RuntimeContext runtimeContext = _runtimeContextAccessor.GetCurrent();

        string applicationId = ChatHistorySessionState.GetOrCreateApplicationId(
                session,
                runtimeContext.ApplicationId.ToString("N"));

        string userId = ChatHistorySessionState.GetOrCreateUserId(session, GetUserId(runtimeContext));
        string conversationId = ChatHistorySessionState.GetOrCreateConversationId(session);
        string sessionId = ChatHistorySessionState.GetOrCreateSessionId(session);
        string agentId = ChatHistorySessionState.GetOrCreateAgentId(session, _defaultAgentId);

        return new ChatHistoryScope(applicationId, userId, conversationId, sessionId, agentId);
    }








    private static bool ShouldPersistRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return false;
        }

        string normalized = role.Trim().ToLowerInvariant();
        return !(normalized.Contains("context") || normalized.Contains("rag"));
    }








    protected override async ValueTask StoreChatHistoryAsync(InvokedContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        if (context.InvokeException is not null)
        {
            return;
        }

        ChatHistoryScope scope = ResolveScope(context.Session);

        IEnumerable<ChatMessage> newMessages = context.RequestMessages.Concat(context.ResponseMessages ?? []);

        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

        await using SqlConnection connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        ;
        ;
        DateTimeOffset baseTimestamp = DateTimeOffset.UtcNow;
        int sequence = 0;

        foreach (ChatMessage message in newMessages)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(message.Text))
            {
                continue;
            }

            string role = message.Role.ToString();
            if (!ShouldPersistRole(role))
            {
                continue;
            }

            PersistedChatMessage persistedMessage = new()
            {
                MessageId = Guid.NewGuid(),
                ConversationId = scope.ConversationId,
                SessionId = scope.SessionId,
                AgentId = scope.AgentId,
                UserId = scope.UserId,
                ApplicationId = scope.ApplicationId,
                Role = role,
                Content = message.Text.Trim(),
                TimestampUtc = baseTimestamp.AddTicks(sequence++)
            };

            await InsertMessageAsync(connection, persistedMessage, cancellationToken).ConfigureAwait(false);
        }
    }








    private static bool TryParseMetadata(string? value, out JsonDocument? document)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            document = null;
            return true;
        }

        try
        {
            using JsonDocument parsed = JsonDocument.Parse(value);
            document = JsonDocument.Parse(parsed.RootElement.GetRawText());
            return true;
        }
        catch (JsonException)
        {
            document = null;
            return false;
        }
    }








    private static void ValidateKey(string value, int maxLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }

        string trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentOutOfRangeException(parameterName, $"Value exceeds maximum length of {maxLength} characters.");
        }
    }








    private static void ValidateMessage(PersistedChatMessage message)
    {
        ValidateKey(message.ConversationId, ConversationKeyLength, nameof(message.ConversationId));
        ValidateKey(message.SessionId, PartitionKeyLength, nameof(message.SessionId));
        ValidateKey(message.AgentId, PartitionKeyLength, nameof(message.AgentId));
        ValidateKey(message.UserId, PartitionKeyLength, nameof(message.UserId));
        ValidateKey(message.ApplicationId, PartitionKeyLength, nameof(message.ApplicationId));
        ValidateKey(message.Role, RoleLength, nameof(message.Role));

        if (string.IsNullOrWhiteSpace(message.Content))
        {
            throw new ArgumentException("Message content is required.", nameof(message));
        }

        if (message.TimestampUtc == default)
        {
            throw new ArgumentException("Message timestamp must be set.", nameof(message));
        }
    }








    private sealed record ChatHistoryScope(string ApplicationId, string UserId, string ConversationId, string SessionId, string AgentId);
}