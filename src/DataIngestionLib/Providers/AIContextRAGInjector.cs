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

using DataIngestionLib.Contracts;
using DataIngestionLib.Services;

using Microsoft.Agents.AI;
using Microsoft.Agents.Builder.State;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;




namespace DataIngestionLib.Providers;





public sealed class AIContextRAGInjector : MessageAIContextProvider
{
    private readonly IHistoryIdentityService _historyIdentityService;
    private readonly ILogger<AIContextRAGInjector> _logger;
    private readonly IRagDataService _ragData;
    private readonly ProviderSessionState<HistoryIdentity> _sessionState;
    private static readonly AsyncLocal<TurnState?> CurrentTurnState = new();







    public AIContextRAGInjector(IRagDataService ragData, ILogger<AIContextRAGInjector> logger, IHistoryIdentityService historyIdentityService)
    {
        Guard.IsNotNull(ragData);
        _ragData = ragData;
        Guard.IsNotNull(logger);
        _logger = logger;

        _historyIdentityService = historyIdentityService;

        // Database keys are stored in the state bag of the session for easy access by the providers and context injectors,
        // and to keep them in sync with the history identity service which is the source of truth for these identifiers.
        _sessionState = new ProviderSessionState<HistoryIdentity>(currentSession => _historyIdentityService.Current, this.GetType().Name);
        // The key under which to store state in the session for this provider. Make sure it does not clash with the keys of other providers.



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
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogTrace(" Entering ProvideMessagesAsync in AIContextRAGInjector provider - Context Enhancement");
        _ = _sessionState.GetOrInitializeState(context.Session);
        ChatMessage request = context.RequestMessages.Last();
        var searchTxt = request.Text;
        if (!this.AgentFrameworkKeywordDetector(searchTxt))
        {
            _logger.LogTrace("RAG failed gate1");
            return [];
        }


        //Checking for short or long queries that are unlikely to yield useful results and can add unnecessary overhead to the system. This is a common practice in search implementations to improve performance and relevance of results.
        if (!this.IsValidQuery(searchTxt))
        {
            _logger.LogTrace("RAG failed gate2");
            return [];
        }

        //Relevance gate - only provide messages if the context is relevant for this provider.
        //This allows us to have multiple providers in the pipeline and only execute the ones that are relevant for the current context.
        if (!this.IsRelevant(context))
        {
            _logger.LogTrace("RAG failed gate3");
            return [];
        }



        //Results come back already tagged as External in the RagDataService, so we don't need to do any additional tagging here.
        IEnumerable<ChatMessage> results = await _ragData.GetRagDataEntries(searchTxt);

        return results ?? [];
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

        _logger.LogInformation("Current turn state is: {turnstate}", CurrentTurnState.Value);

        _logger.LogTrace("Finished with AIContextRAGInjector.");
        return ValueTask.CompletedTask;
    }








    private bool AgentFrameworkKeywordDetector(string text)
    {
        var FrameworkWords = new[] { "agent", "framework", "keyword", "workflow", "IChatClient", "AIAgent", "MAF", "chat", "chat client", "AI", "foundary", "copilot" };
        //base keyword search
        var answer = FrameworkWords.Any(word => text.Contains(word, StringComparison.OrdinalIgnoreCase));
        if (!answer)
        {
            _logger.LogTrace("User text did not pass keyword gate, no context enhancement is injected");
        }

        return answer;


    }








    private bool IsRelevant(InvokingContext context)
    {
        var text = string.Join(" ", context.RequestMessages.Where(m => m.Role == ChatRole.User).Select(m => m.Text));

        var answer = this.AgentFrameworkKeywordDetector(text);
        if (!answer)
        {
            _logger.LogTrace("User text did not pass relevency test, no context enhancement is injected");
        }

        return answer;
    }








    public bool IsValidQuery(string text)
    {
        //Basic validation to filter out queries that are too short or too long, which are unlikely to yield useful results and can add unnecessary overhead to the system.
        if (string.IsNullOrWhiteSpace(text) || text.Length < 5 || text.Length > 1000)
        {
            _logger.LogTrace("Query did not pass validation - Query: {Query}", text);
            return false;
        }

        return true;
    }
}