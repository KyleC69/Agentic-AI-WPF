// 2026/03/07
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         RagSearchTool.cs
//   Author: Kyle L. Crowder



namespace DataIngestionLib.ToolFunctions;





public sealed class RagSearchTool<IRagRetriever>
{
    private readonly IRagRetriever _retriever;








    public RagSearchTool(IRagRetriever retriever)
    {
        _retriever = retriever;
    }








    public IReadOnlyList<RagResult> Search(string query, int topK = 5)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Query cannot be empty.");
        }

        if (topK is < 1 or > 50)
        {
            throw new ArgumentOutOfRangeException(nameof(topK));
        }

        //return _retriever.Search(query, topK);

        return [];



    }
}





public class RagResult
{
    public string Content { get; set; } = "";
    public string Id { get; set; } = "";
    public double Score { get; set; }
}





public interface IRagRetriever
{
    IReadOnlyList<RagResult> Search(string query, int topK);
}





public class RagRetriever : IRagRetriever
{




    public IReadOnlyList<RagResult> Search(string query, int topK)
    {
        // Implement your search logic here
        return [];
    }
}