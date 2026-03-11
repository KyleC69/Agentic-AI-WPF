// Build Date: 2026/03/11
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ChunksText.cs
// Author: Kyle L. Crowder
// Build Num: 105652



using Microsoft.Data.SqlTypes;




namespace DataIngestionLib.ExternalKnowledge.RAGModels;





public class ChunksText
{
    public Guid ChunkId { get; set; }

    public string ChunkText { get; set; } = null!;

    public Guid DocId { get; set; }

    public SqlVector<float> Embedding { get; set; }
}