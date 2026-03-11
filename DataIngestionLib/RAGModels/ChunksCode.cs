// Build Date: 2026/03/11
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ChunksCode.cs
// Author: Kyle L. Crowder
// Build Num: 105652



using Microsoft.Data.SqlTypes;




namespace DataIngestionLib.ExternalKnowledge.RAGModels;





public class ChunksCode
{
    public Guid ChunkId { get; set; }

    public string CodeText { get; set; } = null!;

    public Guid DocId { get; set; }

    public SqlVector<float> Embedding { get; set; }
}