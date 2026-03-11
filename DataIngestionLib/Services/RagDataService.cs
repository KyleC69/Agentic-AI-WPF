// Build Date: 2026/03/11
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         RagDataService.cs
// Author: Kyle L. Crowder
// Build Num: 105642



using System.Collections.ObjectModel;
using System.Text;

using DataIngestionLib.Data;
using DataIngestionLib.ExternalKnowledge.RAGModels;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;




namespace DataIngestionLib.Services;





/// <summary>
///     various functions for search the RAG knowledge base, including vector search and full text search.
///     This is the main entry point for RAG search related functions. Not a tool function, but a service function that can
///     be used by tool functions.
/// </summary>
public static class RagDataService
{
    public static string FullTextSearch(string query, int topK = 5)
    {
        //Database full text search logic here, return the search results as a string. 
        List<FullTextResults> results = [];
        using SqlConnection conn = SqlConnectionFactoryRagKB.CreateConnection();

        using SqlCommand cmd = new("EXEC sp_Search_FullText @query, @topK", conn);
        SqlParameter unused1 = cmd.Parameters.AddWithValue("@query", query);
        SqlParameter unused = cmd.Parameters.AddWithValue("@topK", topK);

        conn.Open();
        using SqlDataReader reader = cmd.ExecuteReader();
        StringBuilder result = new();
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

        string json = JsonConvert.SerializeObject(results);


        return json;
    }








    public static ObservableCollection<RemoteRag> GetRagDataEntries()
    {
        RAGContext context = new();
        context.RemoteRags.Load();
        ObservableCollection<RemoteRag> rags = context.RemoteRags.Local.ToObservableCollection();

        return rags;
    }








    public static string HybridSearch(string query, int topK = 5)
    {
        //Database vector search logic here, return the search results as a string. 
        List<FullTextResults> results = [];
        using SqlConnection conn = SqlConnectionFactoryRagKB.CreateConnection();

        using SqlCommand cmd = new("EXEC sp_Search_hybrid @query, @topK", conn);
        SqlParameter unused1 = cmd.Parameters.AddWithValue("@query", query);
        SqlParameter unused = cmd.Parameters.AddWithValue("@topK", topK);

        conn.Open();
        using SqlDataReader reader = cmd.ExecuteReader();
        StringBuilder result = new();
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

        string json = JsonConvert.SerializeObject(results);


        return json;

    }
}





public sealed class FullTextResults
{
    public int Id { get; set; }
    public string[] Keywords { get; set; } = [];
    public double Score { get; set; }
    public required string Summary { get; set; }
    public required string Title { get; set; }
}