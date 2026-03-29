// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//
//
//
//



using System.Reflection;
using System.Text.Json;

using CommunityToolkit.Diagnostics;

using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Data;
using DataIngestionLib.HistoryModels;
using DataIngestionLib.Models;
using DataIngestionLib.Services.Contracts;

using Microsoft.Agents.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;




namespace DataIngestionLib.Providers;





public sealed class SqlChatHistoryProvider : ChatHistoryProvider
{
    private readonly IAppSettings _appSettings;
    private readonly IHistoryIdentityService _historyIdentityService;

    private readonly SemaphoreSlim _initializationGate = new(1, 1);
    private readonly ILogger<SqlChatHistoryProvider> _logger;
    private readonly int _isInitialized;
    private readonly AIChatHistoryDb? _dbcontext;

    private static readonly HashSet<AgentRequestMessageSourceType> IgnoredRequestSourceTypes =
    [
            AgentRequestMessageSourceType.ChatHistory
    ];








    public SqlChatHistoryProvider(ILogger<SqlChatHistoryProvider> logger, IAppSettings appSettings, IHistoryIdentityService historyIdentityService, IDbContextFactory<AIChatHistoryDb>? dbContextFactory = null)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(appSettings);
        ArgumentNullException.ThrowIfNull(historyIdentityService);

        _logger = logger;
        _appSettings = appSettings;
        _historyIdentityService = historyIdentityService;
        _dbcontext = dbContextFactory?.CreateDbContext();
    }














    protected override async ValueTask<IEnumerable<ChatMessage>> InvokingCoreAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {

            var conversationId = _historyIdentityService.Current.ConversationId;

            IReadOnlyList<PersistedChatMessage> persistedMessages = await this.GetMessagesAsync(conversationId, cancellationToken).ConfigureAwait(false);
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

                ChatMessage contextMessage = new(this.ParseRole(persistedMessage.Role), persistedMessage.Content) { CreatedAt = persistedMessage.TimestampUtc, MessageId = sourceId };

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








    /// <summary>
    /// When overridden in a derived class, adds new messages to the chat history at the end of the agent invocation.
    /// </summary>
    /// <param name="context">Contains the invocation context including request messages, response messages, and any exception that occurred.</param>
    /// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> to monitor for cancellation requests. The default is <see cref="P:System.Threading.CancellationToken.None" />.</param>
    /// <returns>A task that represents the asynchronous add operation.</returns>
    /// <remarks>
    /// <para>
    /// Messages should be added in the order they were generated to maintain proper chronological sequence.
    /// The <see cref="T:Microsoft.Agents.AI.ChatHistoryProvider" /> is responsible for preserving message ordering and ensuring that subsequent calls to
    /// <see cref="M:Microsoft.Agents.AI.ChatHistoryProvider.InvokingCoreAsync(Microsoft.Agents.AI.ChatHistoryProvider.InvokingContext,System.Threading.CancellationToken)" /> return messages in the correct chronological order.
    /// </para>
    /// <para>
    /// Implementations may perform additional processing during message addition, such as:
    /// <list type="bullet">
    /// <item><description>Validating message content and metadata</description></item>
    /// <item><description>Applying storage optimizations or compression</description></item>
    /// <item><description>Triggering background maintenance operations</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// This method is called from <see cref="M:Microsoft.Agents.AI.ChatHistoryProvider.InvokedCoreAsync(Microsoft.Agents.AI.ChatHistoryProvider.InvokedContext,System.Threading.CancellationToken)" />.
    /// Note that <see cref="M:Microsoft.Agents.AI.ChatHistoryProvider.InvokedCoreAsync(Microsoft.Agents.AI.ChatHistoryProvider.InvokedContext,System.Threading.CancellationToken)" /> can be overridden to directly control message filtering and error handling, in which case
    /// it is up to the implementer to call this method as needed to store messages.
    /// </para>
    /// <para>
    /// In contrast with <see cref="M:Microsoft.Agents.AI.ChatHistoryProvider.InvokedCoreAsync(Microsoft.Agents.AI.ChatHistoryProvider.InvokedContext,System.Threading.CancellationToken)" />, this method only stores messages,
    /// while <see cref="M:Microsoft.Agents.AI.ChatHistoryProvider.InvokedCoreAsync(Microsoft.Agents.AI.ChatHistoryProvider.InvokedContext,System.Threading.CancellationToken)" /> is also responsible for messages filtering and error handling.
    /// </para>
    /// <para>
    /// The default implementation of <see cref="M:Microsoft.Agents.AI.ChatHistoryProvider.InvokedCoreAsync(Microsoft.Agents.AI.ChatHistoryProvider.InvokedContext,System.Threading.CancellationToken)" /> only calls this method if the invocation succeeded.
    /// </para>
    /// <para>
    /// <strong>Security consideration:</strong> Messages being stored may contain PII and sensitive conversation content.
    /// Implementers should ensure appropriate encryption at rest and access controls for the storage backend.
    /// </para>
    /// </remarks>
    protected override async ValueTask StoreChatHistoryAsync(InvokedContext context, CancellationToken cancellationToken = new CancellationToken())
    {

        HistoryIdentity identity = _historyIdentityService.Current;

        IEnumerable<ChatMessage> latestMessages = context.RequestMessages;
        IEnumerable<ChatMessage>? lastAgentMessage = context.ResponseMessages;

        await this.PersistInteractionAsync(identity, latestMessages, lastAgentMessage, cancellationToken);

        //   return base.StoreChatHistoryAsync(context, cancellationToken);      
    }








    public void Dispose()
    {

    }







    // Duplication??
    public async ValueTask<PersistedChatMessage> CreateMessageAsync(PersistedChatMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        cancellationToken.ThrowIfCancellationRequested();


        await using AIChatHistoryDb dbContext = await this.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        ChatHistoryMessage entity = this.ToEntity(message);

        EntityEntry<ChatHistoryMessage> unused1 = await dbContext.ChatHistoryMessages.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        var unused = await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return this.ToPersisted(entity);
    }











    public async ValueTask<string?> GetLatestConversationIdAsync(string agentId, string userId, string applicationId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();


        await using AIChatHistoryDb dbContext = await this.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var latest = await dbContext.ChatHistoryMessages.AsNoTracking().Where(message => message.AgentId == agentId).Where(message => message.UserId == userId).Where(message => message.ApplicationId == applicationId).Where(message => message.ConversationId != string.Empty).OrderByDescending(message => message.TimestampUtc).ThenByDescending(message => message.CreatedAt).Select(message => message.ConversationId).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        return string.IsNullOrWhiteSpace(latest) ? null : latest;
    }








    public async ValueTask<PersistedChatMessage?> GetMessageAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        Guard.IsNotNull(messageId);

        cancellationToken.ThrowIfCancellationRequested();


        await using AIChatHistoryDb dbContext = await this.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        ChatHistoryMessage? entity = await dbContext.ChatHistoryMessages.AsNoTracking().FirstOrDefaultAsync(message => message.MessageId == messageId, cancellationToken).ConfigureAwait(false);

        return entity is null ? null : this.ToPersisted(entity);
    }








    public async ValueTask<IReadOnlyList<PersistedChatMessage>> GetMessagesAsync(string conversationId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();


        IQueryable<ChatHistoryMessage> ordered = _dbcontext.ChatHistoryMessages.AsNoTracking().Where(message => message.ConversationId == conversationId).OrderByDescending(message => message.TimestampUtc).ThenByDescending(message => message.CreatedAt);


        List<ChatHistoryMessage> entities = await ordered.OrderBy(message => message.TimestampUtc).ThenBy(message => message.CreatedAt).ToListAsync(cancellationToken).ConfigureAwait(false);

        List<PersistedChatMessage> messages = entities.Select(this.ToPersisted).ToList();

        return messages;
    }







    // TODO: justify the need for this method.
    public async ValueTask<PersistedChatMessage?> UpdateMessageAsync(Guid messageId, string content, DateTimeOffset timestampUtc, CancellationToken cancellationToken = default)
    {
        Guard.IsNotNullOrEmpty(content);
        Guard.IsNotDefault(messageId);
        Guard.IsNotDefault(timestampUtc);

        var normalizedContent = NormalizeContent(content, nameof(content));
        cancellationToken.ThrowIfCancellationRequested();

        await using AIChatHistoryDb dbContext = await this.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        ChatHistoryMessage? entity = await dbContext.ChatHistoryMessages.FirstOrDefaultAsync(message => message.MessageId == messageId, cancellationToken).ConfigureAwait(false);
        if (entity is null)
        {
            return null;
        }

        entity.Content = normalizedContent;
        entity.TimestampUtc = timestampUtc.LocalDateTime;

        var unused = await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return this.ToPersisted(entity);
    }








    private async ValueTask<AIChatHistoryDb> CreateDbContextAsync(CancellationToken cancellationToken)
    {
        return new AIChatHistoryDb(null);
    }








    private static IReadOnlyList<ChatMessage> FilterRequestMessages(IEnumerable<ChatMessage>? requestMessages)
    {
        return requestMessages is null
            ? []
            : (IReadOnlyList<ChatMessage>)requestMessages.Where(message => !IgnoredRequestSourceTypes.Contains(message.GetAgentRequestMessageSourceType())).Where(message => !string.IsNullOrWhiteSpace(message.Text)).ToArray();
    }








    private static IReadOnlyList<ChatMessage> GetContextMessages(object context, string propertyName)
    {
        PropertyInfo? property = context.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        return property?.GetValue(context) is IEnumerable<ChatMessage> messages
            ? messages.Where(message => !string.IsNullOrWhiteSpace(message.Text)).ToArray()
            : (IReadOnlyList<ChatMessage>)[];
    }






    private static string NormalizeContent(string content, string parameterName)
    {
        return string.IsNullOrWhiteSpace(content)
            ? throw new ArgumentException("Message content cannot be empty.", parameterName)
            : content.Trim();
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


        return new ChatRole(normalized);
    }








    private async ValueTask PersistInteractionAsync(HistoryIdentity identity, IEnumerable<ChatMessage> requestMessages, IEnumerable<ChatMessage> responseMessages, CancellationToken cancellationToken)
    {
        await using AIChatHistoryDb dbContext = await this.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        List<ChatHistoryMessage> entities = [];
        entities.AddRange(this.ToEntities(requestMessages, identity.ConversationId, identity.AgentId, identity.UserId, identity.ApplicationId));
        entities.AddRange(this.ToEntities(responseMessages, identity.ConversationId, identity.AgentId, identity.UserId, identity.ApplicationId));

        if (entities.Count == 0)
        {
            _logger.LogError("No chat history messages to persist. This indicates a potential issue with the message collection or an invalid response from an agent.");
            return;
        }

        await dbContext.ChatHistoryMessages.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
        var unused = await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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







    /// <summary>
    /// Converts a collection of <see cref="ChatMessage"/> objects into a list of <see cref="ChatHistoryMessage"/> entities.
    /// </summary>
    /// <param name="messages">The collection of chat messages to convert.</param>
    /// <param name="conversationId">The unique identifier of the conversation.</param>
    /// <param name="agentId">The unique identifier of the agent involved in the conversation.</param>
    /// <param name="userId">The unique identifier of the user involved in the conversation.</param>
    /// <param name="applicationId">The unique identifier of the application associated with the conversation.</param>
    /// <returns>A list of <see cref="ChatHistoryMessage"/> entities representing the provided chat messages.</returns>
    /// <remarks>
    /// This method ensures that duplicate messages are excluded based on their unique message IDs.
    /// It also normalizes and validates the provided identifiers and trims the content of each message.
    /// </remarks>
    internal List<ChatHistoryMessage> ToEntities(IEnumerable<ChatMessage> messages, string conversationId, string agentId, string userId, string applicationId)
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
            Guard.IsNotNull(messageId);
            Guard.IsNotDefault(messageId);
            if (!seenMessageIds.Add(messageId))
            {
                continue;
            }

            entities.Add(new ChatHistoryMessage
            {
                MessageId = messageId,
                ConversationId = _historyIdentityService.Current.ConversationId,
                AgentId = _historyIdentityService.Current.AgentId,
                UserId = _historyIdentityService.Current.UserId,
                ApplicationId = _historyIdentityService.Current.ApplicationId,
                Role = message.Role.Value,
                Content = message.Text.Trim(),
                TimestampUtc = DateTime.Now,
                Metadata = this.SerializeMetadata(message.AdditionalProperties),
                CreatedAt = message.CreatedAt.Value.LocalDateTime,
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
            ConversationId = _historyIdentityService.Current.ConversationId,
            AgentId = _historyIdentityService.Current.AgentId,
            UserId = _historyIdentityService.Current.UserId,
            ApplicationId = _historyIdentityService.Current.ApplicationId,
            Role = message.Role,
            Content = NormalizeContent(message.Content, nameof(message.Content)),
            TimestampUtc = message.TimestampUtc.ToLocalTime().LocalDateTime,
            Metadata = message.Metadata?.RootElement.GetRawText(),
            CreatedAt = message.TimestampUtc.ToLocalTime().LocalDateTime,
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
            Metadata = this.ParseMetadata(message.Metadata, message.MessageId)
        };
    }








    private static Guid TryParseMessageId(string? messageId)
    {
        return Guid.TryParse(messageId, out Guid parsedMessageId) ? parsedMessageId : Guid.NewGuid();
    }
}