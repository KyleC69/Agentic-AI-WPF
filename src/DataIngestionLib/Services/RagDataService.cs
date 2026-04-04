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



using System.Diagnostics.CodeAnalysis;

using DataIngestionLib.EFModels;
using DataIngestionLib.HistoryModels;

using Microsoft.Agents.AI;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;




namespace DataIngestionLib.Services;





public class RagDataService(ILogger<RagDataService> logger, IDbContextFactory<AIChatHistoryDb> chatHistoryDbFactory, IDbContextFactory<AIRemoteRagContext> remoteRagDbFactory)
{
    private readonly ILogger<RagDataService> _logger = logger;
    private readonly IDbContextFactory<AIChatHistoryDb> _chatHistoryDbFactory = chatHistoryDbFactory;
    private readonly IDbContextFactory<AIRemoteRagContext> _remoteRagDbFactory = remoteRagDbFactory;








    public async Task<IReadOnlyList<ChatMessage>?> GetChatHistoryByConversationId(Guid convoId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await using AIChatHistoryDb db = await _chatHistoryDbFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);


        //Get previous history messages from DB
        IReadOnlyList<ChatHistoryMessage> chm = await db.ChatHistoryMessages.Where(m => m.ConversationId == convoId.ToString()).ToListAsync(cancellationToken);
        //Convert to ChatMessages
        IReadOnlyList<ChatMessage> cm = chm.ToChatMessages();
        //Tag messages with source for agent request
        IReadOnlyList<ChatMessage> tagged = cm.Select(cd => cd.WithAgentRequestMessageSource(AgentRequestMessageSourceType.ChatHistory)).ToList();

        return (IReadOnlyList<ChatMessage>?)tagged;
    }








    /// <summary>
    ///     Retrieves a list of RAG (Retrieval-Augmented Generation) data entries based on the provided query.
    ///     This instance targets a local index of remote documents and their location, in this case URL.
    ///     This is extremely useful for scenarios where the kb is either too large to ingest or is frequently updated.
    ///     This ingestion source was specifically focused on MS Learn documentation of Agent Framework API and related
    ///     technologies
    ///     for the purpose of creating this very application, and providing agents with up-to-date documentation for use in
    ///     answering
    ///     user questions about the Agent Framework and related technologies.
    /// </summary>
    /// <param name="query">The search query used to retrieve documents related to the provided vector.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a list of
    ///     <see cref="ChatMessage" /> objects representing the RAG data entries.
    /// </returns>
    /// <remarks>
    ///     This method calls a stored procedure in the database and utilizes preview SQL query features VECTOR_DISTANCE and
    ///     AI_GENERATE_EMBEDDINGS
    ///     The VECTOR_DISTANCE feature calculates the distance between vectors, and can throw in an undetermined manner,
    ///     indicating a missing column score,
    ///     which is removed in current SQL versions (4/1/26) but may still be present in some environments, so error handling
    ///     is implemented to catch this and log it without crashing the application.
    ///     The AI_GENERATE_EMBEDDINGS feature generates embeddings for the provided query.
    ///     It logs an error message if an exception occurs during the operation.
    /// </remarks>
    /// <exception cref="Exception">Thrown when an error occurs while fetching RAG data entries.</exception>
    [Experimental("KC00101")]
    public async Task<IEnumerable<ChatMessage>> GetRagDataEntries(string query, CancellationToken cancellationToken = default)
    {
        List<ChatMessage> rags = new();
        cancellationToken.ThrowIfCancellationRequested();
        await using AIRemoteRagContext db = await _remoteRagDbFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        List<sp_LearnDocs_Search_VectorResult>? results = await db.Procedures.sp_LearnDocs_Search_VectorAsync(query, null);

        if (results is not null)
        {
            foreach (sp_LearnDocs_Search_VectorResult? result in results)
            {
                if (!string.IsNullOrWhiteSpace(result?.Content))
                {
                    rags.Add(new ChatMessage(ChatRole.Tool, result.Content));
                }
            }
        }
        else
        {
            _logger.LogError("sp_LearnDocs_Search_VectorAsync returned null results for query: {Query}", query);
        }

        //Tag messages with source for agent request - this allows the agent to know that these messages came from a RAG data source, and can be used for things like tool use decisions, or source attribution in responses.
        IEnumerable<ChatMessage> tagged = rags.Select(ms => ms.WithAgentRequestMessageSource(AgentRequestMessageSourceType.AIContextProvider, this.GetType().Name));
        return tagged;
    }








    //TODO: move to data service change to EF
    public static string HybridSearch(string query, int topK = 5)
    {
        //Database vector search logic here, return the search results as a string. 
        List<FullTextResults> results = [];
        SqlConnection conn = null!;


        using SqlCommand cmd = new("EXEC sp_Search_hybrid @query, @topK", conn);
        SqlParameter unused1 = cmd.Parameters.AddWithValue("@query", query);
        SqlParameter unused = cmd.Parameters.AddWithValue("@topK", topK);

        conn.Open();
        using SqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            results.Add(new FullTextResults
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Summary = reader.GetString(2),
                Keywords = reader.GetString(3).Split(','),
                Score = reader.GetDouble(4)
            });
        }

        return JsonConvert.SerializeObject(results);

    }








    public class vectorSearchResult
    {
        public required string[] Content { get; init; }
        public double Score { get; init; }
    }
}





public sealed class FullTextResults
{
    public int Id { get; init; }
    public string[] Keywords { get; init; } = [];
    public double Score { get; init; }
    public required string Summary { get; init; }
    public required string Title { get; init; }
}