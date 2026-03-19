// Build Date: 2026/03/19
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         RagDataService.cs
// Author: Kyle L. Crowder
// Build Num: 044258



using System.Collections.ObjectModel;

using DataIngestionLib.Data;
using DataIngestionLib.RAGModels;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;




namespace DataIngestionLib.Services;





public class RagDataService(ILogger<RagDataService> logger)
{
    private readonly ILogger<RagDataService> _logger = logger;








    public static string FullTextSearch(string query, int topK = 5)
    {
        //Database full text search logic here, return the search results as a string. 
        List<FullTextResults> results = [];
        using SqlConnection conn = SqlConnectionFactoryRagKb.CreateConnection();

        using SqlCommand cmd = new("EXEC sp_Search_FullText @query, @topK", conn);
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








    public ObservableCollection<RemoteRag> GetRagDataEntries()
    {
        ObservableCollection<RemoteRag> rags = [];

        try
        {

            RAGContext context = new();
            context.RemoteRags.Load();
            rags = context.RemoteRags.Local.ToObservableCollection();

        }
        catch (Exception ex)
        {

            _logger.LogErrorFetchingRAGDataEntriesMessage(ex.Message);
        }

        return rags;
    }








    public static string HybridSearch(string query, int topK = 5)
    {
        //Database vector search logic here, return the search results as a string. 
        List<FullTextResults> results = [];
        using SqlConnection conn = SqlConnectionFactoryRagKb.CreateConnection();

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
}





public sealed class FullTextResults
{
    public int Id { get; init; }
    public string[] Keywords { get; init; } = [];
    public double Score { get; init; }
    public required string Summary { get; init; }
    public required string Title { get; init; }
}