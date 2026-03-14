// Build Date: 2026/03/13
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         RemoteRag.cs
// Author: Kyle L. Crowder
// Build Num: 175055



using Microsoft.Data.SqlTypes;




namespace DataIngestionLib.RAGModels;





public class RemoteRag
{

    public string Description { get; set; } = null!;

    public Guid DocumentId { get; set; }

    public SqlVector<float>? Embedding { get; set; }
    public int Id { get; set; }

    public string? Keywords { get; set; }

    public DateTime MsDate { get; set; }

    public string OgUrl { get; set; } = null!;

    public double? Score { get; set; }

    public string? Summary { get; set; }

    public string Title { get; set; } = null!;

    public int? TokenCount { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? Version { get; set; }
}