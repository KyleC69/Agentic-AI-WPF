// Build Date: 2026/03/11
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         Document.cs
// Author: Kyle L. Crowder
// Build Num: 105652



namespace DataIngestionLib.ExternalKnowledge.RAGModels;





public class Document
{

    public string? Breadcrumb { get; set; }

    public string ContentRaw { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string? DocHtml { get; set; }
    public Guid DocId { get; set; }

    public string? Hash { get; set; }

    public DateTime? LastFetched { get; set; }

    public string? NormalizedMarkdown { get; set; }

    public string Title { get; set; } = null!;

    public Guid? Uid { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? Url { get; set; }
}