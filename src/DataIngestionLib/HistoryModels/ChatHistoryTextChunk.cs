// Build Date: 2026/04/03
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ChatHistoryTextChunk.cs
// Author: Kyle L. Crowder
// Build Num: 095146



using Microsoft.Data.SqlTypes;




namespace DataIngestionLib.History.HistoryModels;





public class ChatHistoryTextChunk
{

    public long ChunkLength { get; set; }

    public long ChunkOffset { get; set; }

    public long ChunkOrder { get; set; }
    public int ChunkRecordId { get; set; }

    public long ChunkSetId { get; set; }

    public string ChunkText { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public SqlVector<float>? Embedding { get; set; }

    public Guid MessageId { get; set; }
}