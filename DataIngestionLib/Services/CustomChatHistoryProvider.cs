using System.Text.Json;

using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;

using Microsoft.Agents.AI;
using Microsoft.Data.SqlClient;

namespace DataIngestionLib.Services;

public sealed class CustomChatHistoryProvider : ChatHistoryProvider, IChatHistoryProvider
{
    private const int ConversationKeyLength = 128;
    private const int PartitionKeyLength = 128;
    private const int RoleLength = 32;

    private readonly IChatHistoryConnectionFactory _connectionFactory;

    public CustomChatHistoryProvider(IChatHistoryConnectionFactory connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        _connectionFactory = connectionFactory;
    }

    public async ValueTask EnsureInitializedAsync(CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        foreach ((string migrationId, string migrationSql) in ChatHistoryMigrations.All)
        {
            cancellationToken.ThrowIfCancellationRequested();

            bool isApplied = await MigrationAlreadyAppliedAsync(connection, migrationId, cancellationToken).ConfigureAwait(false);
            if (isApplied)
            {
                continue;
            }

            await using SqlTransaction transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                await ExecuteNonQueryAsync(connection, transaction, migrationSql, [], cancellationToken).ConfigureAwait(false);

                SqlParameter[] insertParameters =
                [
                    new("@MigrationId", migrationId),
                    new("@AppliedOnUtc", DateTimeOffset.UtcNow)
                ];

                const string insertMigrationSql = "INSERT INTO dbo.__ChatHistoryMigrations ([Id], [AppliedOnUtc]) VALUES (@MigrationId, @AppliedOnUtc);";
                await ExecuteNonQueryAsync(connection, transaction, insertMigrationSql, insertParameters, cancellationToken).ConfigureAwait(false);

                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
        }
    }

    public async ValueTask<PersistedChatMessage> CreateMessageAsync(PersistedChatMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ValidateMessage(message);

        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

        Guid messageId = message.MessageId == Guid.Empty ? Guid.NewGuid() : message.MessageId;

        const string insertSql =
            """
            INSERT INTO dbo.ChatHistoryMessages
                ([MessageId], [ConversationId], [SessionId], [AgentId], [UserId], [ApplicationId], [Role], [Content], [TimestampUtc], [Metadata])
            VALUES
                (@MessageId, @ConversationId, @SessionId, @AgentId, @UserId, @ApplicationId, @Role, @Content, @TimestampUtc, @Metadata);
            """;

        SqlParameter[] parameters =
        [
            new("@MessageId", messageId),
            new("@ConversationId", message.ConversationId),
            new("@SessionId", message.SessionId),
            new("@AgentId", message.AgentId),
            new("@UserId", message.UserId),
            new("@ApplicationId", message.ApplicationId),
            new("@Role", message.Role),
            new("@Content", message.Content),
            new("@TimestampUtc", message.TimestampUtc),
            new("@Metadata", message.Metadata?.RootElement.GetRawText() ?? (object)DBNull.Value)
        ];

        await using SqlConnection connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await ExecuteNonQueryAsync(connection, null, insertSql, parameters, cancellationToken).ConfigureAwait(false);

        return message with { MessageId = messageId };
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
        await using SqlCommand command = CreateCommand(connection, null, selectSql, [new SqlParameter("@MessageId", messageId)]);
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

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

        bool hasTake = take.HasValue;
        string selectSql = hasTake
            ?
            """
            SELECT [MessageId], [ConversationId], [SessionId], [AgentId], [UserId], [ApplicationId], [Role], [Content], [TimestampUtc], [Metadata]
            FROM (
                SELECT TOP (@Take)
                    [MessageId], [ConversationId], [SessionId], [AgentId], [UserId], [ApplicationId], [Role], [Content], [TimestampUtc], [Metadata]
                FROM dbo.ChatHistoryMessages
                WHERE [ConversationId] = @ConversationId
                ORDER BY [TimestampUtc] DESC, [MessageId] DESC
            ) AS latest
            ORDER BY [TimestampUtc] ASC, [MessageId] ASC;
            """
            :
            """
            SELECT [MessageId], [ConversationId], [SessionId], [AgentId], [UserId], [ApplicationId], [Role], [Content], [TimestampUtc], [Metadata]
            FROM dbo.ChatHistoryMessages
            WHERE [ConversationId] = @ConversationId
            ORDER BY [TimestampUtc] ASC, [MessageId] ASC;
            """;

        List<SqlParameter> parameters = [new("@ConversationId", conversationId.Trim())];
        if (hasTake)
        {
            parameters.Add(new SqlParameter("@Take", take!.Value));
        }

        List<PersistedChatMessage> messages = [];

        await using SqlConnection connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using SqlCommand command = CreateCommand(connection, null, selectSql, [.. parameters]);
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

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
            new("@Content", content.Trim()),
            new("@TimestampUtc", timestampUtc),
            new("@MessageId", messageId)
        ];

        await using SqlConnection connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
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

        const string deleteSql = "DELETE FROM dbo.ChatHistoryMessages WHERE [MessageId] = @MessageId;";
        await using SqlConnection connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

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

        const string deleteSql = "DELETE FROM dbo.ChatHistoryMessages WHERE [ConversationId] = @ConversationId;";
        await using SqlConnection connection = await _connectionFactory.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        return await ExecuteNonQueryAsync(connection, null, deleteSql, [new SqlParameter("@ConversationId", conversationId.Trim())], cancellationToken).ConfigureAwait(false);
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

    private static async ValueTask<int> ExecuteNonQueryAsync(SqlConnection connection, SqlTransaction? transaction, string sql, SqlParameter[] parameters, CancellationToken cancellationToken)
    {
        await using SqlCommand command = CreateCommand(connection, transaction, sql, parameters);
        return await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
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

    private async ValueTask<bool> MigrationAlreadyAppliedAsync(SqlConnection connection, string migrationId, CancellationToken cancellationToken)
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
        object? result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

        return result is bool boolValue && boolValue;
    }
}
