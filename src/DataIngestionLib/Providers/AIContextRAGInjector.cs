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



using CommunityToolkit.Diagnostics;

using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Data;
using DataIngestionLib.Services;
using DataIngestionLib.Services.Contracts;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;




namespace DataIngestionLib.Providers;





public sealed class AIContextRAGInjector : MessageAIContextProvider
{
    private readonly AIChatHistoryDb _dbcontext;
    private readonly IHistoryIdentityService _historyIdentityService;
    private readonly ILogger<AIContextRAGInjector> _logger;
    private readonly RagDataService _ragData;
    private readonly ProviderSessionState<HistoryIdentity> _sessionStateHelper;








    public AIContextRAGInjector(RagDataService ragData, ILogger<AIContextRAGInjector> logger, IHistoryIdentityService historyIdentityService)
    {
        Guard.IsNotNull(ragData);
        _ragData = ragData;
        Guard.IsNotNull(logger);
        _logger = logger;



        _logger = logger;
        _historyIdentityService = historyIdentityService;
        _dbcontext = new AIChatHistoryDb();

        // Database keys are stored in the state bag of the session for easy access by the providers and context injectors,
        // and to keep them in sync with the history identity service which is the source of truth for these identifiers.
        _sessionStateHelper = new ProviderSessionState<HistoryIdentity>(currentSession => _historyIdentityService.Current, this.GetType().Name);
        // The key under which to store state in the session for this provider. Make sure it does not clash with the keys of other providers.



    }








    /// <summary>
    ///     Called at the end of the agent invocation to process the invocation results.
    /// </summary>
    /// <param name="context">
    ///     Contains the invocation context including request messages, response messages, and any exception
    ///     that occurred.
    /// </param>
    /// <param name="cancellationToken">
    ///     The <see cref="T:System.Threading.CancellationToken" /> to monitor for cancellation
    ///     requests. The default is <see cref="P:System.Threading.CancellationToken.None" />.
    /// </param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    ///     <para>
    ///         Implementers can use the request and response messages in the provided <paramref name="context" /> to:
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>Update internal state based on conversation outcomes</description>
    ///             </item>
    ///             <item>
    ///                 <description>Extract and store memories or preferences from user messages</description>
    ///             </item>
    ///             <item>
    ///                 <description>Log or audit conversation details</description>
    ///             </item>
    ///             <item>
    ///                 <description>Perform cleanup or finalization tasks</description>
    ///             </item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         This method is called regardless of whether the invocation succeeded or failed.
    ///         To check if the invocation was successful, inspect the
    ///         <see cref="P:Microsoft.Agents.AI.AIContextProvider.InvokedContext.InvokeException" /> property.
    ///     </para>
    ///     <para>
    ///         The default implementation of this method skips execution for any invocation failures,
    ///         filters the request messages using the configured store-input request message filter
    ///         (which defaults to including only <see cref="P:Microsoft.Agents.AI.AgentRequestMessageSourceType.External" />
    ///         messages),
    ///         filters the response messages using the configured store-input response message filter
    ///         (which defaults to a no-op, so all response messages are processed),
    ///         and calls
    ///         <see
    ///             cref="M:Microsoft.Agents.AI.AIContextProvider.StoreAIContextAsync(Microsoft.Agents.AI.AIContextProvider.InvokedContext,System.Threading.CancellationToken)" />
    ///         to process the invocation results.
    ///         For most scenarios, overriding
    ///         <see
    ///             cref="M:Microsoft.Agents.AI.AIContextProvider.StoreAIContextAsync(Microsoft.Agents.AI.AIContextProvider.InvokedContext,System.Threading.CancellationToken)" />
    ///         is sufficient to process invocation results,
    ///         while still benefiting from the default error handling and filtering behavior.
    ///         However, for scenarios that require more control over error handling or message filtering, overriding this
    ///         method
    ///         allows you to directly control the processing of invocation results.
    ///     </para>
    /// </remarks>
    protected override ValueTask InvokedCoreAsync(InvokedContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        return base.InvokedCoreAsync(context, cancellationToken);
    }








    /// <summary>
    ///     Called at the start of agent invocation to provide additional messages.
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
    ///     <see cref="T:System.Collections.Generic.IEnumerable`1" /> to be used by the agent during this invocation.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         Implementers can load any additional messages required at this time, such as:
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>Retrieving relevant information from knowledge bases</description>
    ///             </item>
    ///             <item>
    ///                 <description>Adding system instructions or prompts</description>
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
    ///             cref="M:Microsoft.Agents.AI.MessageAIContextProvider.ProvideMessagesAsync(Microsoft.Agents.AI.MessageAIContextProvider.InvokingContext,System.Threading.CancellationToken)" />
    ///         to get additional messages,
    ///         stamps any messages with <see cref="P:Microsoft.Agents.AI.AgentRequestMessageSourceType.AIContextProvider" />
    ///         source attribution,
    ///         and merges the returned messages with the original (unfiltered) input messages.
    ///         For most scenarios, overriding
    ///         <see
    ///             cref="M:Microsoft.Agents.AI.MessageAIContextProvider.ProvideMessagesAsync(Microsoft.Agents.AI.MessageAIContextProvider.InvokingContext,System.Threading.CancellationToken)" />
    ///         is sufficient to provide additional messages,
    ///         while still benefiting from the default filtering, merging and source stamping behavior.
    ///         However, for scenarios that require more control over message filtering, merging or source stamping, overriding
    ///         this method
    ///         allows you to directly control the full <see cref="T:System.Collections.Generic.IEnumerable`1" /> returned for
    ///         the invocation.
    ///     </para>
    /// </remarks>
    protected override ValueTask<IEnumerable<ChatMessage>> InvokingCoreAsync(InvokingContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        return new(_sessionStateHelper.GetOrInitializeState(context.Session).Messages);
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
    protected override ValueTask<AIContext> InvokingCoreAsync(AIContextProvider.InvokingContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        return base.InvokingCoreAsync(context, cancellationToken);
    }








    /// <summary>
    ///     Provides a collection of relevant chat messages based on the context and the last request message.
    /// </summary>
    /// <param name="context">
    ///     The <see cref="InvokingContext" /> containing the session and request messages.
    /// </param>
    /// <param name="cancellationToken">
    ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an enumerable collection of
    ///     <see cref="ChatMessage" /> objects.
    /// </returns>
    /// <remarks>
    ///     This method retrieves the last request message from the provided context and uses it to search for relevant results
    ///     through the <see cref="RagDataService" />. The results are returned as chat messages.
    /// </remarks>
    protected override async ValueTask<IEnumerable<ChatMessage>> ProvideMessagesAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        _ = _sessionStateHelper.GetOrInitializeState(context.Session);
        IEnumerable<ChatMessage> results = Array.Empty<ChatMessage>();
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogTrace(" Entering ProvideMessagesAsync in AIContextRAGInjector provider - Context Enhancement");

        //Get last request messagefrom context and search for relvant RAG results
        ChatMessage search = context.RequestMessages.Last();

        if (search is not null)
        {
            //Results come back already tagged as External in the RagDataService, so we don't need to do any additional tagging here.
            results = await _ragData.GetRagDataEntries(search.Text);
        }

        return results;

    }








    /// <summary>
    ///     Stores the AI context asynchronously after the invocation of a specific operation.
    /// </summary>
    /// <param name="context">
    ///     The <see cref="AIContextProvider.InvokedContext" /> containing details about the invoked operation and its
    ///     associated data.
    /// </param>
    /// <param name="cancellationToken">
    ///     A <see cref="CancellationToken" /> that can be used to observe cancellation requests.
    /// </param>
    /// <returns>
    ///     A <see cref="ValueTask" /> representing the asynchronous operation.
    /// </returns>
    protected override ValueTask StoreAIContextAsync(InvokedContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Finished with AIContextRAGInjector.");
        return ValueTask.CompletedTask;
    }
}