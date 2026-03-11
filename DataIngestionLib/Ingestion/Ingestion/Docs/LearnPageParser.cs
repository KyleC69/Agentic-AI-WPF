// 2026/03/04
//  Solution: DataIngestionService
//  Project:   DataIngestionService
//  File:         LearnPageParser.cs
//   Author: Kyle L. Crowder



#nullable enable

using Microsoft.Extensions.Logging;

using DataIngestionLib.EFModels;




namespace DataIngestionLib.DocIngestion;





public sealed class LearnPageParser : ILearnPageParser
{
    private readonly ILogger<LearnPageParser> _logger;








    /// <summary>
    ///     Initializes a new instance of <see cref="LearnPageParser" /> with required dependencies.
    /// </summary>
    /// <param name="logger">Logger instance provided by the DI container.</param>
    public LearnPageParser(ILogger<LearnPageParser> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }








    public async Task<DocPage?> ParseAsync(string pagePath, Guid? ingestionRunId, Guid? sourceSnapshotId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pagePath))
        {
            throw new ArgumentException("Page path cannot be null or whitespace.", nameof(pagePath));
        }

        cancellationToken.ThrowIfCancellationRequested();
        DocPage page = new();

        using IDisposable? scope = _logger.BeginScope(new Dictionary<string, object>
        {
                ["PagePath"] = pagePath,
                ["IngestionRunId"] = ingestionRunId,
                ["SourceSnapshotId"] = sourceSnapshotId
        });

        try
        {

            var markdown = await File.ReadAllTextAsync(pagePath, cancellationToken).ConfigureAwait(false);
            Guid PageID = Guid.NewGuid();
            cancellationToken.ThrowIfCancellationRequested();

            //  var metblock = MarkdownRagExtractor.ReadFirstLines(pagePath);

            /*     page.Title = metblock.FirstOrDefault(line => line.StartsWith("title:", StringComparison.OrdinalIgnoreCase))?.TrimStart("title:".ToCharArray()).Trim();
                 page.Description = metblock.FirstOrDefault(line => line.StartsWith("description:", StringComparison.OrdinalIgnoreCase))?.TrimStart("description:".ToCharArray()).Trim();
                 var date = metblock.FirstOrDefault(line => line.StartsWith("ms.date:", StringComparison.OrdinalIgnoreCase))?.TrimStart("ms.date:".ToCharArray()).Trim();
                 page.MetaDate = DateTime.TryParse(date, out DateTime parsedDate) ? parsedDate : null;

                 page.Url = pagePath; // Assuming URL is the same as the file path for now TODO: change db schema to this can map to either url or path depending on source type
                 page.CreatedIngestionRunId = ingestionRunId ?? Guid.Empty;
                 page.SourceSnapshotId = sourceSnapshotId ?? Guid.Empty;
                 page.Id = PageID;
                 page.Language = "English"; // default to English for now, we can enhance the parser later to detect language from metadata or content
                 page.RawMarkdown = markdown;
                 page.IsActive = true;
                 page.RawPageSource = Markdown.ToPlainText(markdown);
                 page.SourcePath = pagePath;
                 page.ValidFromUtc = DateTime.Now;
                 page.ContentHash = HashUtils.ComputeSha256(page.RawMarkdown);
                 page.SemanticUid = HashUtils.ComputeSemanticUidForPage(page.SourcePath, page.Description, page.Title);

     */
            _logger.LogInformation("Parsing page. Title='{Title}' Complete", page.Title);

            return page;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse learn page.");
            return null;
        }
    }
}