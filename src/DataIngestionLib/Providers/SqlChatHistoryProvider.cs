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



using System.Diagnostics;
using System.Text.Json;

using CommunityToolkit.Diagnostics;

using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Data;
using DataIngestionLib.HistoryModels;
using DataIngestionLib.Models;
using DataIngestionLib.Services.Contracts;

using Microsoft.Agents.AI;
using Microsoft.Agents.Core.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;




namespace DataIngestionLib.Providers;





public sealed class SqlChatHistoryProvider : ChatHistoryProvider
{
    private readonly IAppSettings _appSettings;
    private readonly IHistoryIdentityService _historyIdentityService;

    private readonly SemaphoreSlim _initializationGate = new(1, 1);
    private readonly ILogger<SqlChatHistoryProvider> _logger;
    private readonly ProviderSessionState<HistoryIdentity> _sessionStateHelper;
    private AIChatHistoryDb? _dbcontext;

    private static readonly HashSet<AgentRequestMessageSourceType> IgnoredRequestSourceTypes =
    [
            AgentRequestMessageSourceType.AIContextProvider
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

        // Database keys are stored in the state bag of the session for easy access by the providers and context injectors,
        // and to keep them in sync with the history identity service which is the source of truth for these identifiers.
        _sessionStateHelper = new ProviderSessionState<HistoryIdentity>(
                // stateInitializer is called when there is no state in the session for this ChatHistoryProvider yet
                currentSession => _historyIdentityService.Current,
                // The key under which to store state in the session for this provider. Make sure it does not clash with the keys of other providers.
                this.GetType().Name);


    }








    /// <summary>
    ///     When overridden in a derived class, provides the chat history messages to be used for the current invocation.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method is called from
    ///         <see
    ///             cref="M:Microsoft.Agents.AI.ChatHistoryProvider.InvokingCoreAsync(Microsoft.Agents.AI.ChatHistoryProvider.InvokingContext,System.Threading.CancellationToken)" />
    ///         .
    ///         Note that
    ///         <see
    ///             cref="M:Microsoft.Agents.AI.ChatHistoryProvider.InvokingCoreAsync(Microsoft.Agents.AI.ChatHistoryProvider.InvokingContext,System.Threading.CancellationToken)" />
    ///         can be overridden to directly control message filtering, merging and source stamping, in which case
    ///         it is up to the implementer to call this method as needed to retrieve the unfiltered/unmerged chat history
    ///         messages.
    ///     </para>
    ///     <para>
    ///         In contrast with
    ///         <see
    ///             cref="M:Microsoft.Agents.AI.ChatHistoryProvider.InvokingCoreAsync(Microsoft.Agents.AI.ChatHistoryProvider.InvokingContext,System.Threading.CancellationToken)" />
    ///         , this method only returns additional messages to be added to the request,
    ///         while
    ///         <see
    ///             cref="M:Microsoft.Agents.AI.ChatHistoryProvider.InvokingCoreAsync(Microsoft.Agents.AI.ChatHistoryProvider.InvokingContext,System.Threading.CancellationToken)" />
    ///         is responsible for returning the full set of messages to be used for the invocation (including caller provided
    ///         messages).
    ///     </para>
    ///     <para>
    ///         Messages are returned in chronological order to maintain proper conversation flow and context for the agent.
    ///         The oldest messages appear first in the collection, followed by more recent messages.
    ///     </para>
    ///     <para>
    ///         <strong>Security consideration:</strong> Messages loaded from storage should be treated with the same caution
    ///         as user-supplied
    ///         messages. A compromised storage backend could alter message roles to escalate trust (e.g., changing <c>user</c>
    ///         messages to
    ///         <c>system</c> messages) or inject adversarial content that influences LLM behavior.
    ///     </para>
    /// </remarks>
    /// <param name="context">
    ///     Contains the request context including the caller provided messages that will be used by the
    ///     agent for this invocation.
    /// </param>
    /// <param name="cancellationToken">
    ///     The <see cref="T:System.Threading.CancellationToken" /> to monitor for cancellation
    ///     requests. The default is <see cref="P:System.Threading.CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a collection of
    ///     <see cref="T:Microsoft.Extensions.AI.ChatMessage" />
    ///     instances in ascending chronological order (oldest first).
    /// </returns>
    protected override ValueTask<IEnumerable<ChatMessage>> ProvideChatHistoryAsync(InvokingContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        return new(_sessionStateHelper.GetOrInitializeState(context.Session).Messages);
    }








    /// <summary>
    ///     When overridden in a derived class, adds new messages to the chat history at the end of the agent invocation.
    /// </summary>
    /// <param name="context">
    ///     Contains the invocation context including request messages, response messages, and any exception
    ///     that occurred.
    /// </param>
    /// <param name="cancellationToken">
    ///     The <see cref="T:System.Threading.CancellationToken" /> to monitor for cancellation
    ///     requests. The default is <see cref="P:System.Threading.CancellationToken.None" />.
    /// </param>
    /// <returns>A task that represents the asynchronous add operation.</returns>
    /// <remarks>
    ///     <para>
    ///         Messages should be added in the order they were generated to maintain proper chronological sequence.
    ///         The <see cref="T:Microsoft.Agents.AI.ChatHistoryProvider" /> is responsible for preserving message ordering and
    ///         ensuring that subsequent calls to
    ///         <see
    ///             cref="M:Microsoft.Agents.AI.ChatHistoryProvider.InvokingCoreAsync(Microsoft.Agents.AI.ChatHistoryProvider.InvokingContext,System.Threading.CancellationToken)" />
    ///         return messages in the correct chronological order.
    ///     </para>
    ///     <para>
    ///         Implementations may perform additional processing during message addition, such as:
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>Validating message content and metadata</description>
    ///             </item>
    ///             <item>
    ///                 <description>Applying storage optimizations or compression</description>
    ///             </item>
    ///             <item>
    ///                 <description>Triggering background maintenance operations</description>
    ///             </item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         This method is called from
    ///         <see
    ///             cref="M:Microsoft.Agents.AI.ChatHistoryProvider.InvokedCoreAsync(Microsoft.Agents.AI.ChatHistoryProvider.InvokedContext,System.Threading.CancellationToken)" />
    ///         .
    ///         Note that
    ///         <see
    ///             cref="M:Microsoft.Agents.AI.ChatHistoryProvider.InvokedCoreAsync(Microsoft.Agents.AI.ChatHistoryProvider.InvokedContext,System.Threading.CancellationToken)" />
    ///         can be overridden to directly control message filtering and error handling, in which case
    ///         it is up to the implementer to call this method as needed to store messages.
    ///     </para>
    ///     <para>
    ///         In contrast with
    ///         <see
    ///             cref="M:Microsoft.Agents.AI.ChatHistoryProvider.InvokedCoreAsync(Microsoft.Agents.AI.ChatHistoryProvider.InvokedContext,System.Threading.CancellationToken)" />
    ///         , this method only stores messages,
    ///         while
    ///         <see
    ///             cref="M:Microsoft.Agents.AI.ChatHistoryProvider.InvokedCoreAsync(Microsoft.Agents.AI.ChatHistoryProvider.InvokedContext,System.Threading.CancellationToken)" />
    ///         is also responsible for messages filtering and error handling.
    ///     </para>
    ///     <para>
    ///         The default implementation of
    ///         <see
    ///             cref="M:Microsoft.Agents.AI.ChatHistoryProvider.InvokedCoreAsync(Microsoft.Agents.AI.ChatHistoryProvider.InvokedContext,System.Threading.CancellationToken)" />
    ///         only calls this method if the invocation succeeded.
    ///     </para>
    ///     <para>
    ///         <strong>Security consideration:</strong> Messages being stored may contain PII and sensitive conversation
    ///         content.
    ///         Implementers should ensure appropriate encryption at rest and access controls for the storage backend.
    ///     </para>
    /// </remarks>
    protected override async ValueTask StoreChatHistoryAsync(InvokedContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            HistoryIdentity state = _sessionStateHelper.GetOrInitializeState(context.Session);
            List<ChatMessage> cm = this.GetContextMessages(context);

            //Need to filter out bad tool results and empty messages before saving to the session state and database,
            //but we want to keep the full set of messages in the session state for any providers that run after this one in the pipeline and may need access to the unfiltered messages.
            IEnumerable<ChatMessage> filtered = FilterMessages(cm);

            state.Messages.AddRange(filtered);
            _sessionStateHelper.SaveState(context.Session, state);
            _logger.LogTrace("Beginning to save chat messages for conversation {0}", state.ConversationId);
            await this.PersistInteractionAsync(state, filtered, cancellationToken);

        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unable to persist chat history to SQL store..");
        }
    }








    /// <summary>
    ///     Gets the set of keys used to store the provider state in the
    ///     <see cref="P:Microsoft.Agents.AI.AgentSession.StateBag" />.
    /// </summary>
    /// <remarks>
    ///     The default value is a single-element set containing the name of the concrete type (e.g.
    ///     <c>"InMemoryChatHistoryProvider"</c>).
    ///     Implementations may override this to provide custom keys, for example when multiple
    ///     instances of the same provider type are used in the same session, or when a provider
    ///     stores state under more than one key.
    /// </remarks>
    public override IReadOnlyList<string> StateKeys => new[] { _sessionStateHelper.StateKey };








    /// <summary>
    ///     Validates and converts a nullable <see cref="DateTimeOffset" /> to a <see cref="DateTime" /> in UTC format.
    /// </summary>
    /// <param name="offset">The nullable <see cref="DateTimeOffset" /> to be validated and converted.</param>
    /// <returns>
    ///     A <see cref="DateTime" /> in UTC format. If <paramref name="offset" /> is <c>null</c>, the current UTC time is
    ///     returned.
    /// </returns>
    private DateTime CheckDateStamp(DateTimeOffset? offset)
    {
        return offset?.UtcDateTime ?? DateTime.UtcNow;
    }








    private async ValueTask<AIChatHistoryDb> CreateDbContextAsync(CancellationToken cancellationToken)
    {
        return new AIChatHistoryDb();
    }








    private static IEnumerable<ChatMessage> FilterMessages(IEnumerable<ChatMessage> allMessages)
    {


        //REmoves messages that are tagged with ignored source types or that have empty/whitespace content,
        //as these are not useful to keep in the chat history and can cause issues with some LLM providers if included in the prompt.
        ChatMessage[] clean = allMessages.Where(message => !IgnoredRequestSourceTypes.Contains(message.GetAgentRequestMessageSourceType())).Where(message => !string.IsNullOrWhiteSpace(message.Text)).ToArray();
        IEnumerable<ChatMessage> good = clean.Where(msg => !IsErroredToolResult(msg)).ToList();
        return good;
    }








    /// <summary>
    ///     Retrieves a combined list of context messages from the provided <see cref="InvokedContext" />.
    /// </summary>
    /// <param name="context">
    ///     The context containing the request and response messages to be aggregated.
    /// </param>
    /// <returns>
    ///     A list of <see cref="ChatMessage" /> objects that includes both request and response messages.
    /// </returns>
    private List<ChatMessage> GetContextMessages(InvokedContext context)
    {
        List<ChatMessage> msgs = [];
        Debug.Assert(context.ResponseMessages != null, "context.ResponseMessages != null");
        foreach (ChatMessage m in context.ResponseMessages)
        {
            msgs.Add(m);
        }

        foreach (ChatMessage m in context.RequestMessages)
        {
            msgs.Add(m);
        }

        return msgs;
    }








    public async ValueTask<string?> GetLatestConversationIdAsync(string agentId, string userId, string applicationId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await using AIChatHistoryDb dbContext = await this.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var latest = await dbContext.ChatHistoryMessages.AsNoTracking().Where(message => message.AgentId == agentId).Where(message => message.UserId == userId).Where(message => message.ApplicationId == applicationId).Where(message => message.ConversationId != string.Empty).OrderByDescending(message => message.TimestampUtc).ThenByDescending(message => message.CreatedAt).Select(message => message.ConversationId).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        return string.IsNullOrWhiteSpace(latest) ? null : latest;
    }








    //TODO: Refactor
    public async ValueTask<IReadOnlyList<PersistedChatMessage>> GetMessagesAsync(string conversationId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _dbcontext = new();
        _logger.LogTrace("Fetching chat history messages for conversation {ConversationId}", conversationId);
        var ordered = _dbcontext.ChatHistoryMessages.Where(message => message.ConversationId == conversationId).ToList();

        IReadOnlyList<ChatMessage> chatMessages = ordered.ToChatMessages();

        IEnumerable<ChatMessage> tagged = chatMessages.Select(m => m.WithAgentRequestMessageSource(AgentRequestMessageSourceType.ChatHistory));


        var messages = ordered.Select(this.ToPersisted).ToList();

        return messages;
    }








    /// <summary>
    ///     Determines whether the specified chat message represents an errored tool result.
    /// </summary>
    /// <param name="msg">The chat message to evaluate.</param>
    /// <returns>
    ///     <c>true</c> if the chat message is from a tool and contains an error; otherwise, <c>false</c>.
    /// </returns>
    private static bool IsErroredToolResult(ChatMessage msg)
    {
        if (msg.Role != ChatRole.Tool)
        {
            return false;
        }

        _ = msg.ToJsonElements();

        foreach (AIContent content in msg.Contents)
        {
            if (content is FunctionResultContent rc)
            {
                if (rc.Result is not null && rc.Result is JsonElement resultElement && resultElement.TryGetProperty("error", out _))
                {
                    return true;
                }
            }
        }

        return false;
    }








    private static string NormalizeContent(string content, string parameterName)
    {
        return string.IsNullOrWhiteSpace(content) ? throw new ArgumentException("Message content cannot be empty.", parameterName) : content.Trim();
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








    private async ValueTask PersistInteractionAsync(HistoryIdentity identity, IEnumerable<ChatMessage> messages, CancellationToken cancellationToken)
    {
        await using AIChatHistoryDb dbContext = await this.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        Guard.IsNotNull(dbContext);

        List<ChatHistoryMessage> entities = [];
        entities.AddRange(this.ToEntities(messages, identity));

        if (entities.Count == 0)
        {
            _logger.LogError("No chat history messages to persist. This indicates a potential issue with the message collection or an invalid response from an agent.");
            return;
        }

        try
        {
            _logger.LogTrace("Persisting chat history messages.For conversation {ConversationId}", identity.ConversationId);
            await dbContext.ChatHistoryMessages.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
            var unused = await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occured during record save.");
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








    /// <summary>
    ///     Converts a collection of <see cref="ChatMessage" /> objects into a list of <see cref="ChatHistoryMessage" />
    ///     entities.
    /// </summary>
    /// <param name="messages">The collection of chat messages to convert.</param>
    /// <param name="conversationId">The unique identifier of the conversation.</param>
    /// <param name="agentId">The unique identifier of the agent involved in the conversation.</param>
    /// <param name="userId">The unique identifier of the user involved in the conversation.</param>
    /// <param name="applicationId">The unique identifier of the application associated with the conversation.</param>
    /// <returns>A list of <see cref="ChatHistoryMessage" /> entities representing the provided chat messages.</returns>
    /// <remarks>
    ///     This method ensures that duplicate messages are excluded based on their unique message IDs.
    ///     It also normalizes and validates the provided identifiers and trims the content of each message.
    /// </remarks>
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
                CreatedAt = this.CheckDateStamp(message.CreatedAt),
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
            TimestampUtc = message.TimestampUtc,
            Metadata = message.Metadata?.RootElement.GetRawText(),
            CreatedAt = message.CreatedAt,
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