// 2026/03/04
//  Solution: DataIngestionService
//  Project:   DataIngestionService
//  File:         LocalFileParser.cs
//   Author: Kyle L. Crowder



using Markdig;
using Markdig.Renderers.Normalize;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.DataIngestion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML.Tokenizers;




namespace DataIngestionLib.DocIngestion;





internal class LocalFileParser : ILocalFileParser
{
    private readonly IChatClient _chatClient;

    private readonly ILogger<LocalFileParser> _logger;
    private readonly string _vocab;








    /// <summary>
    ///     Initializes a new instance of <see cref="LocalFileParser" /> using dependencies
    ///     supplied by the DI container rather than the service locator.
    /// </summary>
    /// <param name="logger">Logger provided by the DI container.</param>
    /// <param name="settings">Ingestion configuration options.</param>
    /// <param name="chatClient">Chat client used for keyword and summary enrichment.</param>
    public LocalFileParser(
            ILogger<LocalFileParser> logger,
            IOptions<IngestionSettings> settings,
            IChatClient chatClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        IngestionSettings s = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _vocab = s.VocabPath ?? throw new InvalidOperationException("Missing Ingestion:VocabPath configuration.");
        _chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
    }








    public async Task<LearnPageParseResult> ParseAsync(string path, Guid ingestionRunId, Guid sourceSnapshotId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        using IDisposable scope = _logger.BeginScope(new Dictionary<string, object>
        {
                ["Url"] = path,
                ["IngestionRunId"] = ingestionRunId,
                ["SourceSnapshotId"] = sourceSnapshotId
        });

        LearnPageParseResult result = new();

        try
        {
            using (StreamReader sreader = new(path))
            {
                var rawFile = await sreader.ReadToEndAsync(token).ConfigureAwait(false);

                IngestionDocument doc = new("newdoc1");
                IngestionDocumentReader reader = new MarkdownReader();

                MarkdownPipeline pipe = new MarkdownPipelineBuilder()
                        .UsePipeTables()
                        .UseTaskLists()
                        .UseEmojiAndSmiley()
                        .UseCustomContainers()
                        .UseAdvancedExtensions()
                        .Build();


                //Remove any links in the markdown to avoid confusion for the chunker and enrichers. We will keep the link text but remove the actual links.
                MarkdownDocument rem = Markdown.Parse(rawFile, pipe);
                foreach (AutolinkInline item in rem.Descendants<AutolinkInline>()) item.Remove();


                // Convert the modified markdown document back to text
                await using StringWriter writer = new();
                NormalizeRenderer renderer = new(writer);
                pipe.Setup(renderer);
                _ = renderer.Render(rem);
                var modifiedMarkdown = writer.ToString();


                //Convert the Markdown Document back to
                MemoryStream ms = new(Encoding.UTF8.GetBytes(modifiedMarkdown));
                doc = await reader.ReadAsync(ms, "doc1", "md", token).ConfigureAwait(false);

                //########################################################
                // This should be Microsoft.ML.Tokenizers. for the Chunker below.
                Tokenizer tok = BertTokenizer.Create(_vocab, new BertOptions());
                // Testing the Header Chunker.
                HeaderChunker chunker = new(new IngestionChunkerOptions(tok));
                // Process the document and get the chunks as an async stream.
                var chunks = chunker.ProcessAsync(doc, token);
                IList<IngestionChunk<string>> chunkList = [];
                await foreach (var chunk in chunks.ConfigureAwait(false)) chunkList.Add(chunk);


                //###########################################
                //##
                //##    Enrichers from Data Ingestion namespace

                //##   Keyword Enricher
                List<IngestionChunk<string>> wordlist = [];
                KeywordEnricher keywordEnricher = new(new EnricherOptions(_chatClient), null, 5, 0.7f);
                var keywords = keywordEnricher.ProcessAsync(ToAsyncEnumerable(chunkList, token), token);
                await foreach (var item in keywords.ConfigureAwait(false)) wordlist.Add(item);

                List<IngestionChunk<string>> summaries = [];
                SummaryEnricher summaryEnricher = new(new EnricherOptions(_chatClient), 50);
                var gist = summaryEnricher.ProcessAsync(ToAsyncEnumerable(chunkList, token), token);
                await foreach (var g in gist.ConfigureAwait(false)) summaries.Add(g);
            }


        }
        catch (Exception ex)
        {
            // Handle exceptions as needed
            _logger.LogError(ex, "Error reading file: {Path}", path);
        }

        return result;
    }








    private static async IAsyncEnumerable<IngestionChunk<string>> ToAsyncEnumerable(
            IList<IngestionChunk<string>> items,
            [System.Runtime.CompilerServices.EnumeratorCancellation]
            CancellationToken cancellationToken = default)
    {
        foreach (var item in items)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return item;
        }
    }
}