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



using DataIngestionLib.Data;
using DataIngestionLib.EFModels;
using DataIngestionLib.HistoryModels;

using Microsoft.Agents.AI;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;




namespace DataIngestionLib.Services;





public class RagDataService(ILogger<RagDataService> logger)
{
    private readonly ILogger<RagDataService> _logger = logger;








    public static async Task<IReadOnlyList<ChatMessage>?> GetChatHistoryByConversationId(Guid convoId)
    {
        AIChatHistoryDb db = new();



        IReadOnlyList<ChatHistoryMessage> chm = await db.ChatHistoryMessages.Where(m => m.ConversationId == convoId.ToString()).ToListAsync();

        IReadOnlyList<ChatMessage> cm = chm.ToChatMessages();
        IEnumerable<ChatMessage> unused = cm.Select(cd => cd.WithAgentRequestMessageSource(AgentRequestMessageSourceType.ChatHistory));

        return cm;
    }








    public async Task<List<ChatMessage>> GetRagDataEntries(string query)
    {
        List<ChatMessage> rags = new();

        try
        {
            using AIRemoteRagContext context = new();
            List<sp_LearnDocs_Search_VectorResult> results = await context.Procedures.sp_LearnDocs_Search_VectorAsync(query, 10);



            foreach (sp_LearnDocs_Search_VectorResult result in results)
            {
                rags.Add(new ChatMessage(ChatRole.Tool, result.Content));
            }
        }
        catch (Exception ex)
        {

            _logger.LogErrorFetchingRAGDataEntriesMessage(ex.Message);
        }


        IEnumerable<ChatMessage> unused = rags.Select(ms => ms.WithAgentRequestMessageSource(AgentRequestMessageSourceType.AIContextProvider));
        return rags;
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