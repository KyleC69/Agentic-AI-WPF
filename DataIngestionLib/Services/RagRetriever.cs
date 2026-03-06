// 2026/03/05
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         RagRetriever.cs
//   Author: Kyle L. Crowder



using Microsoft.Data.SqlClient;




namespace DataIngestionLib.Services;





public sealed class RagRetriever : IRagRetriever
{
    private readonly string _connectionString;
    private readonly double _fullTextWeight;
    private readonly double _semanticWeight;

    // Default weights (can be tuned)
    private readonly double _vectorWeight;








    public RagRetriever(
            string connectionString,
            double vectorWeight = 0.6,
            double fullTextWeight = 0.3,
            double semanticWeight = 0.1)
    {
        _connectionString = connectionString;
        _vectorWeight = vectorWeight;
        _fullTextWeight = fullTextWeight;
        _semanticWeight = semanticWeight;
    }








    public IReadOnlyList<RagResult> Search(string query, int topK)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Query cannot be empty.");
        }

        if (topK is < 1 or > 50)
        {
            throw new ArgumentOutOfRangeException(nameof(topK));
        }

        // 1. Run vector search
        Dictionary<int, double> vectorResults = RunVectorSearch(query, topK);

        // 2. Run full-text search
        Dictionary<int, double> fullTextResults = RunFullTextSearch(query, topK);

        // 3. Run semantic similarity
        Dictionary<int, double> semanticResults = RunSemanticSimilarity(query, topK);

        // 4. Merge and re-rank
        return MergeAndRank(vectorResults, fullTextResults, semanticResults, topK);
    }








    // -----------------------------
    // Merge + Weighted Ranking
    // -----------------------------
    private IReadOnlyList<RagResult> MergeAndRank(
            Dictionary<int, double> vector,
            Dictionary<int, double> fullText,
            Dictionary<int, double> semantic,
            int topK)
    {
        IEnumerable<int> allIds = vector.Keys
                .Union(fullText.Keys)
                .Union(semantic.Keys)
                .Distinct();

        List<RagResult> results = [];

        foreach (int id in allIds)
        {
            double v = vector.TryGetValue(id, out double vv) ? vv : 0;
            double f = fullText.TryGetValue(id, out double ff) ? ff : 0;
            double s = semantic.TryGetValue(id, out double ss) ? ss : 0;

            double finalScore =
                    (v * _vectorWeight) +
                    (f * _fullTextWeight) +
                    (s * _semanticWeight);

            results.Add(new(id, finalScore));
        }

        return results
                .OrderByDescending(r => r.Score)
                .Take(topK)
                .ToList();
    }








    // -----------------------------
    // Full-Text Search
    // -----------------------------
    private Dictionary<int, double> RunFullTextSearch(string query, int topK)
    {
        Dictionary<int, double> results = [];

        using SqlConnection conn = new(_connectionString);
        conn.Open();

        using SqlCommand cmd = new(@"
            SELECT TOP (@k) doc_id, RANK
            FROM CONTAINSTABLE(dbo.Documents, content, @query)
            ORDER BY RANK DESC;", conn);

        cmd.Parameters.AddWithValue("@query", query);
        cmd.Parameters.AddWithValue("@k", topK);

        using SqlDataReader? reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            int id = reader.GetInt32(0);
            double rank = reader.GetInt32(1) / 1000.0; // normalize 0–1
            results[id] = rank;
        }

        return results;
    }








    // -----------------------------
    // Semantic Similarity
    // -----------------------------
    private Dictionary<int, double> RunSemanticSimilarity(string query, int topK)
    {
        Dictionary<int, double> results = [];

        using SqlConnection conn = new(_connectionString);
        conn.Open();

        using SqlCommand cmd = new(@"
            SELECT TOP (@k) doc_id, similarity
            FROM dbo.SemanticSimilarity(@query)
            ORDER BY similarity DESC;", conn);

        cmd.Parameters.AddWithValue("@query", query);
        cmd.Parameters.AddWithValue("@k", topK);

        using SqlDataReader? reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            int id = reader.GetInt32(0);
            double score = reader.GetDouble(1);
            results[id] = score;
        }

        return results;
    }








    // -----------------------------
    // Vector Search
    // -----------------------------
    private Dictionary<int, double> RunVectorSearch(string query, int topK)
    {
        Dictionary<int, double> results = [];

        using SqlConnection conn = new(_connectionString);
        conn.Open();

        using SqlCommand cmd = new(@"
            SELECT TOP (@k) doc_id, similarity_score
            FROM dbo.VectorSearch(@query)
            ORDER BY similarity_score DESC;", conn);

        cmd.Parameters.AddWithValue("@query", query);
        cmd.Parameters.AddWithValue("@k", topK);

        using SqlDataReader? reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            int id = reader.GetInt32(0);
            double score = reader.GetDouble(1);
            results[id] = score;
        }

        return results;
    }
}





// -----------------------------
// Result Model
// -----------------------------
public sealed class RagResult
{

    public RagResult(int documentId, double score)
    {
        DocumentId = documentId;
        Score = score;
    }








    public int DocumentId { get; }
    public double Score { get; }
}





// -----------------------------
// Interface
// -----------------------------
public interface IRagRetriever
{
    IReadOnlyList<RagResult> Search(string query, int topK);
}