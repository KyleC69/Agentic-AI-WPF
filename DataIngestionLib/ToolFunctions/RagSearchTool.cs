// 2026/03/10
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         RagSearchTool.cs
//   Author: Kyle L. Crowder



namespace DataIngestionLib.ToolFunctions;





public sealed class RagSearchTool
{
    private readonly IRagRetriever _retriever;








    public RagSearchTool(IRagRetriever retriever)
    {
        _retriever = retriever;
    }








    public ToolResult<IReadOnlyList<RagResult>> Search(string query, int topK = 5)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return ToolResult<IReadOnlyList<RagResult>>.Fail("Query cannot be empty.");
        }

        if (topK is < 1 or > 50)
        {
            return ToolResult<IReadOnlyList<RagResult>>.Fail("topK must be between 1 and 50.");
        }

        var results = _retriever.Search(query, topK);
        return ToolResult<IReadOnlyList<RagResult>>.Ok(results);



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