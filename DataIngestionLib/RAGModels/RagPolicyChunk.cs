// Build Date: 2026/03/11
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         RagPolicyChunk.cs
// Author: Kyle L. Crowder
// Build Num: 105653



using Microsoft.Data.SqlTypes;




namespace DataIngestionLib.ExternalKnowledge.RAGModels;





public class RagPolicyChunk
{

    public string? Category { get; set; }

    public Guid ChunkId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public SqlVector<float> Embedding { get; set; }
    public int Id { get; set; }

    public string? Tags { get; set; }
}