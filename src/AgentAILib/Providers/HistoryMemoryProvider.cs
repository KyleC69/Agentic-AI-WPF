// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         HistoryMemoryProvider.cs
// Author: Kyle L. Crowder
// Build Num: 194458



using AgentAILib.Contracts;
using AgentAILib.Services;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.ChatCompletion;

using AIContext = Microsoft.Agents.AI.AIContext;
using AIContextProvider = Microsoft.Agents.AI.AIContextProvider;




namespace AgentAILib.Providers;





/// <summary>
///     Acts as a sliding context window for the conversation maintaining recent messages in memory for quick retrieval.
/// </summary>
/// <remarks></remarks>
public sealed class HistoryMemoryProvider : AIContextProvider
{

    private readonly Dictionary<string, List<ChatMessage>> _conversationWindows = new(StringComparer.OrdinalIgnoreCase);
    private int _currentTokenCount;
    private readonly IHistoryIdentityService _historyIdentityService;
    private readonly ProviderSessionState<HistoryIdentity> _sessionState;
    private readonly Lock _windowLock = new();
    private readonly int _windowSize;
    private readonly Dictionary<string, string> Messages = new();
    private const int DEFAULT_WINDOW_SIZE = 100_000;








    /// <summary>
    ///     Initializes a new instance of the HistoryMemoryProvider class with the specified history identity service and
    ///     optional window size.
    /// </summary>
    /// <param name="historyIdentityService">
    ///     The service used to resolve user or conversation identity for history operations.
    ///     Cannot be null.
    /// </param>
    /// <param name="windowSize">
    ///     The maximum number of messages to retain in memory. If null or less than or equal to zero, a default value is
    ///     used.
    /// </param>
    /// <remarks>
    ///     The base class has params for message filters. If you pass null the default filter is applied. These filters
    ///     are being bypassed here.
    /// </remarks>
    public HistoryMemoryProvider(IHistoryIdentityService historyIdentityService, int? windowSize = 100_000) : base(messages => messages, messages => messages, messages => messages)
    {
        ArgumentNullException.ThrowIfNull(historyIdentityService);
        _currentTokenCount = 0;
        _historyIdentityService = historyIdentityService;
        _windowSize = windowSize.GetValueOrDefault(DEFAULT_WINDOW_SIZE);
        if (_windowSize <= 0)
        {
            _windowSize = DEFAULT_WINDOW_SIZE;
        }

        _sessionState = new ProviderSessionState<HistoryIdentity>(stateInitializer: currentSession => new HistoryIdentity(HistoryIdentityService.GetConversationId()), stateKey: this.GetType().Name);
    }








    /// <summary>
    ///     Called at the start of agent invocation to provide additional context.
    /// </summary>
    /// <param name="context">
    ///     Contains the request context including the caller provided messages that will be used by the
    ///     agent for this invocation.
    /// </param>
    /// <param name="cancellationToken">
    ///     The <see cref="T:System.Threading.CancellationToken" /> to monitor for cancellation
    ///     requests. The default is <see cref="P:System.Threading.CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the
    ///     <see cref="T:Microsoft.Agents.AI.AIContext" /> with additional context to be used by the agent during this
    ///     invocation.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         Implementers can load any additional context required at this time, such as:
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>Retrieving relevant information from knowledge bases</description>
    ///             </item>
    ///             <item>
    ///                 <description>Adding system instructions or prompts</description>
    ///             </item>
    ///             <item>
    ///                 <description>Providing function tools for the current invocation</description>
    ///             </item>
    ///             <item>
    ///                 <description>Injecting contextual messages from conversation history</description>
    ///             </item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         The default implementation of this method filters the input messages using the configured provide-input message
    ///         filter
    ///         (which defaults to including only <see cref="P:Microsoft.Agents.AI.AgentRequestMessageSourceType.External" />
    ///         messages),
    ///         then calls
    ///         <see
    ///             cref="M:Microsoft.Agents.AI.AIContextProvider.ProvideAIContextAsync(Microsoft.Agents.AI.AIContextProvider.InvokingContext,System.Threading.CancellationToken)" />
    ///         to get additional context,
    ///         stamps any messages from the returned context with
    ///         <see cref="P:Microsoft.Agents.AI.AgentRequestMessageSourceType.AIContextProvider" /> source attribution,
    ///         and merges the returned context with the original (unfiltered) input context (concatenating instructions,
    ///         messages, and tools).
    ///         For most scenarios, overriding
    ///         <see
    ///             cref="M:Microsoft.Agents.AI.AIContextProvider.ProvideAIContextAsync(Microsoft.Agents.AI.AIContextProvider.InvokingContext,System.Threading.CancellationToken)" />
    ///         is sufficient to provide additional context,
    ///         while still benefiting from the default filtering, merging and source stamping behavior.
    ///         However, for scenarios that require more control over context filtering, merging or source stamping, overriding
    ///         this method
    ///         allows you to directly control the full <see cref="T:Microsoft.Agents.AI.AIContext" /> returned for the
    ///         invocation.
    ///     </para>
    /// </remarks>
    protected override ValueTask<AIContext> InvokingCoreAsync(InvokingContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        return base.InvokingCoreAsync(context, cancellationToken);
    }








    /// <summary>
    ///     When overridden in a derived class, provides additional AI context to be merged with the input context for the
    ///     current invocation.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method is called from
    ///         <see
    ///             cref="M:Microsoft.Agents.AI.AIContextProvider.InvokingCoreAsync(Microsoft.Agents.AI.AIContextProvider.InvokingContext,System.Threading.CancellationToken)" />
    ///         .
    ///         Note that
    ///         <see
    ///             cref="M:Microsoft.Agents.AI.AIContextProvider.InvokingCoreAsync(Microsoft.Agents.AI.AIContextProvider.InvokingContext,System.Threading.CancellationToken)" />
    ///         can be overridden to directly control context merging and source stamping, in which case
    ///         it is up to the implementer to call this method as needed to retrieve the additional context.
    ///     </para>
    ///     <para>
    ///         In contrast with
    ///         <see
    ///             cref="M:Microsoft.Agents.AI.AIContextProvider.InvokingCoreAsync(Microsoft.Agents.AI.AIContextProvider.InvokingContext,System.Threading.CancellationToken)" />
    ///         , this method only returns additional context to be merged with the input,
    ///         while
    ///         <see
    ///             cref="M:Microsoft.Agents.AI.AIContextProvider.InvokingCoreAsync(Microsoft.Agents.AI.AIContextProvider.InvokingContext,System.Threading.CancellationToken)" />
    ///         is responsible for returning the full merged <see cref="T:Microsoft.Agents.AI.AIContext" /> for the invocation.
    ///     </para>
    ///     <para>
    ///         <strong>Security consideration:</strong> Any messages, tools, or instructions returned by this method will be
    ///         merged into the
    ///         AI request context. If data is retrieved from external or untrusted sources, implementers should validate and
    ///         sanitize it
    ///         to prevent indirect prompt injection attacks.
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
    ///     A task that represents the asynchronous operation. The task result contains an
    ///     <see cref="T:Microsoft.Agents.AI.AIContext" />
    ///     with additional context to be merged with the input context.
    /// </returns>
    protected override ValueTask<AIContext> ProvideAIContextAsync(InvokingContext context, CancellationToken cancellationToken = new CancellationToken())
    {

        IEnumerable<ChatMessage> tagged = (context.AIContext.Messages ?? []).Select(m => m.WithAgentRequestMessageSource(AgentRequestMessageSourceType.ChatHistory, this.GetType().Name)).ToArray();

        if (!tagged.Any())
        {
            return base.ProvideAIContextAsync(context, cancellationToken);
        }

        AgentSession? session = context.Session;
        if (session is null)
        {
            return base.ProvideAIContextAsync(context, cancellationToken);
        }

        HistoryIdentity state = _sessionState.GetOrInitializeState(session);

        //Make sure to include the instructions in the token count as well since they are part of the context
        if (!string.IsNullOrWhiteSpace(context.AIContext.Instructions))
        {
            _currentTokenCount += ComputeTokenEstimate(context.AIContext.Instructions, cancellationToken);
        }

        foreach (ChatMessage m in tagged)
        {
            if (m.Text is not null)
            {
                _currentTokenCount += ComputeTokenEstimate(m.Text, cancellationToken);

                if (_currentTokenCount > _windowSize)
                {
                    //Need to implement a reducer to drop the oldest messages out of context
                    // For now, we'll just break as a placeholder
                    throw new ExceededContextLimitException(_currentTokenCount);

                }
            }

        }

        session.StateBag.SetValue("TokenCount", (object)_currentTokenCount);
        _sessionState.SaveState(session, state);

        return base.ProvideAIContextAsync(context, cancellationToken);
    }








    /// <summary>
    ///     Gets the set of keys used to store the provider state in the
    ///     <see cref="P:Microsoft.Agents.AI.AgentSession.StateBag" />.
    /// </summary>
    /// <remarks>
    ///     The default value is a single-element set containing the name of the concrete type (e.g.
    ///     <c>"TextSearchProvider"</c>).
    ///     Implementations may override this to provide custom keys, for example when multiple
    ///     instances of the same provider type are used in the same session, or when a provider
    ///     stores state under more than one key.
    /// </remarks>
    public override IReadOnlyList<string> StateKeys
    {
        get { return new[] { _sessionState.StateKey }; }
    }








    private int ComputeTokenEstimate(string messageText, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // This is a very rough estimate. In practice, you would want to use a more accurate method based on the specific tokenizer used by your model.
        return messageText.Length / 4;
    }








    private static bool HasExplicitSourceType(ChatMessage message, AgentRequestMessageSourceType sourceType)
    {
        return message.AdditionalProperties is not null && message.AdditionalProperties.TryGetValue(AgentRequestMessageSourceAttribution.AdditionalPropertiesKey, out var value) && value is AgentRequestMessageSourceAttribution attribution && attribution.SourceType == sourceType;
    }








    private static ChatRole MapRole(AuthorRole role)
    {
        return role == AuthorRole.System ? ChatRole.System : role == AuthorRole.Assistant ? ChatRole.Assistant : role == AuthorRole.Tool ? ChatRole.Tool : ChatRole.User;
    }








    private string ResolveConversationId(string? conversationId)
    {
        return !string.IsNullOrWhiteSpace(conversationId) ? conversationId : _historyIdentityService.Current.ConversationId;
    }








    /// <summary>
    ///     Determines whether the specified chat message should be included in the context.
    /// </summary>
    /// <param name="message">The chat message to evaluate. Cannot be null.</param>
    /// <returns>
    ///     <c>true</c> if the message should be included in the context; otherwise, <c>false</c>.
    ///     Messages with empty or whitespace-only text, or those with a role of <see cref="ChatRole.Tool" />, are excluded.
    /// </returns>
    private static bool ShouldIncludeInContext(ChatMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.Text))
        {
            return false;
        }

        // Tool results are available only in the current turn and should not be replayed.
        return message.Role != ChatRole.Tool;
    }








    private static bool ShouldPersistMessage(ChatMessage message)
    {
        return ShouldIncludeInContext(message) && !HasExplicitSourceType(message, AgentRequestMessageSourceType.ChatHistory);
    }
}





public class ExceededContextLimitException : Exception
{
    public ExceededContextLimitException(int currentTokenCount) : base($"The current token count of {currentTokenCount} exceeds the allowed context window size.")
    {
    }
}