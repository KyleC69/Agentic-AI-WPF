// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         RemoteRag.cs
// Author: Kyle L. Crowder
// Build Num: 155941



using System.ComponentModel.DataAnnotations;

using Microsoft.Data.SqlTypes;




namespace DataIngestionLib.RAGModels;





public sealed class RemoteRag
{

    public string Description { get; init; } = null!;

    public Guid DocumentId { get; init; }

    public SqlVector<float>? Embedding { get; init; }
    public int Id { get; init; }

    public string? Keywords { get; init; }

    public DateTime MsDate { get; init; }

    public string OgUrl { get; init; } = null!;

    public double? Score { get; init; }

    [MaxLength(4000)] public string? Summary { get; init; }

    public string Title { get; init; } = null!;

    public int? TokenCount { get; init; }

    public DateTime? UpdatedAt { get; init; }

    public int? Version { get; init; }
}