// Build Date: 2026/03/19
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         Document.cs
// Author: Kyle L. Crowder
// Build Num: 044253



using System.ComponentModel.DataAnnotations;




namespace DataIngestionLib.RAGModels;





public sealed class Document
{

    [MaxLength(350)] public string? Breadcrumb { get; init; }

    [MaxLength(16384)] public string ContentRaw { get; init; } = null!;

    public DateTime CreatedAt { get; init; }

    [MaxLength(32768)] public string? DocHtml { get; init; }

    public Guid DocId { get; init; }

    [MaxLength(450)] public string? Hash { get; init; }

    public DateTime? LastFetched { get; init; }

    [MaxLength(16384)] public string? NormalizedMarkdown { get; init; }

    [MaxLength(512)] public string Title { get; init; } = null!;

    public Guid? Uid { get; init; }

    public DateTime? UpdatedAt { get; init; }

    [MaxLength(350)] public string? Url { get; init; }
}