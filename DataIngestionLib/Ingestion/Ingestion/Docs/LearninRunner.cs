// 2026/03/04
//  Solution: DataIngestionService
//  Project:   DataIngestionService
//  File:         LearninRunner.cs
//   Author: Kyle L. Crowder



using Markdig;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DataIngestion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML.Tokenizers;

using OllamaSharp;

using DataIngestionLib.EFModels;




namespace DataIngestionLib.DocIngestion;





public sealed class LearninRunner
{
    private readonly IChatClient _chatClient;

    // KBContext is not registered in the DI container (AddDbContext is commented out in App.xaml.cs).
    // It is created directly here and will be replaced with injection once the DB connection is configured.
    private readonly KBContext _dbContext = new();
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embed;
    private readonly ILogger<LearninRunner> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILearnPageParser _parser;





    // The ingestion root path is supplied via IngestionSettings.ModelOnnxPath or as a parameter to RunIngestionAsync;
    // no hardcoded path constant is used at runtime.





    private static readonly Lazy<Task<Tokenizer>> TokenizerFactory = new(CreateTokenizerAsync);








    /// <summary>
    ///     Initializes a new instance of <see cref="LearninRunner" /> with required dependencies.
    /// </summary>
    /// <param name="logger">Logger provided by the DI container.</param>
    /// <param name="loggerFactory">Logger factory used to configure enricher pipelines.</param>
    /// <param name="settings">Ingestion configuration options.</param>
    /// <param name="parser">Page parser for reading markdown documentation files.</param>
    public LearninRunner(
            ILogger<LearninRunner> logger,
            ILoggerFactory loggerFactory,
            IOptions<IngestionSettings> settings,
            ILearnPageParser parser)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        IngestionSettings s = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        Uri baseUrl = new(s.OllamaBaseUrl ?? "http://127.0.0.1:11434");
        var model = s.OllamaModel ?? "mxbai-embed-large-v1";
        _embed = new OllamaApiClient(baseUrl, model);
        _chatClient = new OllamaApiClient(baseUrl, model);
        _logger.LogDebug("LearninRunner initialized with OllamaApiClient for embeddings and chat.");
    }








    public bool IsRunning { get; private set; }





    /// <summary>
    ///     Gets or sets the mode that controls whether content is fetched from a remote web URL
    ///     or read from the local file system.
    /// </summary>
    /// <remarks>
    ///     TODO: This is currently a static mutable property for backward compatibility.
    ///     Future refactoring should pass <see cref="IngestionSourceMode" /> as a method parameter
    ///     or constructor argument to eliminate shared state and enable safe concurrent ingestion runs.
    /// </remarks>
    public static IngestionSourceMode SourceMode { get; set; }








    /// <summary>
    ///     Enumerates <c>*.md</c> and <c>*.cs</c> files under <paramref name="startingPath" />,
    ///     filtering out generated artefact directories (<c>obj</c>, <c>.git</c>, <c>.vs</c>,
    ///     and <c>\includes\</c>).
    /// </summary>
    /// <param name="startingPath">Root directory to enumerate.</param>
    /// <param name="token">A token to observe for cancellation requests.</param>
    /// <returns>
    ///     A tuple of two lazy enumerables: markdown files and C# source files.
    ///     Both sequences are empty when an error occurs during enumeration.
    /// </returns>
    private (IEnumerable<string> mdFilesEnumerable, IEnumerable<string> csFilesEnumerable) BuildTocAsync(string startingPath, CancellationToken token)
    {
        try
        {
            var blacklist = new[] { "\\includes\\", "obj", ".git", ".vs" };

            var mdFilesEnumerable = Directory.EnumerateFiles(startingPath, "*.md", SearchOption.AllDirectories)
                    .Where(file => !blacklist.Any(b => file.Contains(b)));
            var csFilesEnumerable = Directory.EnumerateFiles(startingPath, "*.cs", SearchOption.AllDirectories)
                    .Where(file => !blacklist.Any(b => file.Contains(b)));

            return (mdFilesEnumerable, csFilesEnumerable);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "BuildTocAsync: error enumerating files at '{StartingPath}'.", startingPath);
        }

        return (Enumerable.Empty<string>(), Enumerable.Empty<string>());
    }








    /*


    /// <summary>
    ///     Initiates and manages the document ingestion process, including fetching, parsing,
    ///     chunking, and enriching documents from a specified starting URL.
    /// </summary>
    /// <param name="startingPath">
    ///     The file system path or web URL from which the ingestion process begins. This is used to fetch the initial
    ///     document or table of contents for processing.
    /// </param>
    /// <param name="token">
    ///     A <see cref="CancellationToken" /> that can be used to cancel the ingestion process.
    /// </param>
    /// <returns>
    ///     A <see cref="Task" /> representing the asynchronous operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when critical parameters such as the snapshot source or run ID are null,
    ///     or when the table of contents (TOC) cannot be fetched.
    /// </exception>
    public async Task RunIngestionAsync(string startingPath, CancellationToken token = default)
    {
        Guid? ingestionRunId = Guid.NewGuid();
        Guid? snapshotID = Guid.NewGuid();

        SourceMode = startingPath.StartsWith("http") ? IngestionSourceMode.Web : IngestionSourceMode.FileSystem;

        //         await dbContext.Procedures.sp_BeginIngestionRunAsync("1.2", "run doc ingestion", ingestionRunId, cancellationToken: token);
        //           await dbContext.Procedures.sp_CreateSourceSnapshotAsync(ingestionRunId.Value, "UNK", startingUrl, "main", "", "", "", "", "", snapshotID, cancellationToken: token);
        //       if (snapshotID.Value == Guid.Empty || ingestionRunId.Value == Guid.Empty)
        //          throw new ArgumentNullException("snapshot source or run ID is null Aborting....");

        _ = new
                LearnPageParser();
        // Logger and repository are now resolved through injected dependencies.
        LearninRunner runner = new();


        await runner.IngestAsync(new[] { startingPath }, snapshotID, ingestionRunId, token);

        //  _logger.LogInformation("Doc ingestion is complete...");
        _ = await _dbContext.Procedures.sp_EndIngestionRunAsync(ingestionRunId.Value, cancellationToken: token);
    }


    */








    private async Task<List<IngestionChunk<string>>> CreateChunksAsync(IngestionDocument doc, CancellationToken token)
    {
        if (doc == null)
        {
            throw new ArgumentNullException(nameof(doc), "The document cannot be null.");
        }

        try
        {
            Tokenizer tok = await TokenizerFactory.Value.ConfigureAwait(false);
            HeaderChunker chunker = new(new IngestionChunkerOptions(tok));
            var chunks = chunker.ProcessAsync(doc, token);
            List<IngestionChunk<string>> chunkList = [];
            await foreach (var chunk in chunks.WithCancellation(token).ConfigureAwait(false)) chunkList.Add(chunk);

            return chunkList;
        }
        catch (Exception ex)
        {
            // Log the exception and rethrow or handle it as necessary
            throw new InvalidOperationException("An error occurred while creating chunks.", ex);
        }
    }








    private static async Task<Tokenizer> CreateTokenizerAsync()
    {
        return await BertTokenizer.CreateAsync(@"f:\\ai-models\\mxbai-embed-large-v1\\vocab.txt").ConfigureAwait(false);
    }








    //DataIngestion API testing (API INCOMPLETE)
    public async Task IngestDocumentAsync(string filePath, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        ArgumentException.ThrowIfNullOrEmpty(filePath);
        MarkdownParserContext context = new();




        var markdown = await File.ReadAllTextAsync(filePath, token).ConfigureAwait(false);


        var textdoc = Markdown.ToPlainText(markdown, null, context);



        _logger.LogDebug("Document read successfully. Starting chunking process...");

        IngestionDocumentReader reader = new MarkdownReader();

        //  IngestionDocument dic = await reader.ReadAsync(stream, filePath, "text/markdown", token);

        IngestionChunkerOptions options = new(await TokenizerFactory.Value.ConfigureAwait(false))
        {
                MaxTokensPerChunk = 1000
        };

        //IngestionChunkWriter<string> writer = new MyIngestionChunkWriter(_loggerFactory.CreateLogger<MyIngestionChunkWriter>());

        HeaderChunker chunker = new(new IngestionChunkerOptions(await TokenizerFactory.Value.ConfigureAwait(false))
        {
                MaxTokensPerChunk = 1000
        });

        SummaryEnricher summaryEnricher = new(new EnricherOptions(_chatClient)
        {
                LoggerFactory = _loggerFactory
        }, 150);


        KeywordEnricher keywordEnricher = new(new EnricherOptions(_chatClient)
        {
                LoggerFactory = _loggerFactory
        }, null, 5, 0.7f);

        //     using var pipeline = new IngestionPipeline<string>(reader, chunker, writer, loggerFactory: _loggerFactory)
        {
            //           ChunkProcessors = { summaryEnricher, keywordEnricher }
        }
        ;

        // NOTE: mdFilesEnumerable is not consumed here; left as a placeholder for future IngestionPipeline integration.
        var mdFilesEnumerable = Directory.EnumerateFiles(filePath, "*.md", SearchOption.AllDirectories);

        //   await foreach (IngestionResult result in pipeline.ProcessAsync(Directory.EnumerateFiles(testFile, "*.md",SearchOption.AllDirectories),token)) 
        {
            //     _logger.LogDebug($"Completed processing '{result.DocumentId}'. Succeeded: '{result.Succeeded}'.");
        }



        _logger.LogTrace("Ingestion pipeline processing completed.");

    }








    /// <summary>
    ///     Processes and ingests a collection of URLs by parsing their content, persisting the parsed results,
    ///     and performing additional operations such as chunking and embedding generation.
    /// </summary>
    /// <param name="urls">An array of URLs to be ingested.</param>
    /// <param name="sourceSnapshotId">The unique identifier of the source snapshot associated with the ingestion.</param>
    /// <param name="ingestionRunId">The unique identifier of the ingestion run.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="urls" /> parameter is null.</exception>
    /// <remarks>
    ///     This method handles errors gracefully by logging warnings and continuing with the next URL in case of failures.
    ///     It also includes throttling mechanisms to avoid overwhelming the server.
    /// </remarks>
    public async Task IngestDocumentAsync(string Startingpath, Guid? sourceSnapshotId, Guid? ingestionRunId, CancellationToken cancellationToken = default)
    {
        if (Startingpath == null)
        {
            throw new ArgumentNullException(nameof(Startingpath));
        }

        var (mdFilesEnumerable, csFilesEnumerable) = BuildTocAsync(Startingpath, cancellationToken);

        foreach (var mdFile in mdFilesEnumerable)
        {
            _logger.LogTrace("Processing file {0}", mdFile);



            try
            {
                {
                    DocPage result = await _parser.ParseAsync(mdFile, ingestionRunId, sourceSnapshotId, cancellationToken).ConfigureAwait(false);






                    await SafeSave.SaveOrBackupAsync(_dbContext, result, doc => new DocPageBk
                    {
                            /* map properties from doc to backup */
                            Id = doc.Id,
                            SemanticUid = doc.SemanticUid,
                            SourceSnapshotId = doc.SourceSnapshotId,
                            SourcePath = doc.SourcePath,
                            Title = doc.Title,
                            Language = doc.Language,
                            Url = doc.Url,
                            VersionNumber = doc.VersionNumber,
                            CreatedIngestionRunId = doc.CreatedIngestionRunId,
                            UpdatedIngestionRunId = doc.UpdatedIngestionRunId,
                            RemovedIngestionRunId = doc.RemovedIngestionRunId,
                            ValidFromUtc = doc.ValidFromUtc,
                            ValidToUtc = doc.ValidToUtc,
                            IsActive = doc.IsActive,
                            ContentHash = doc.ContentHash,
                            RawMarkdown = doc.RawMarkdown,
                            RawPageSource = doc.RawPageSource,
                            Description = doc.Description,
                            MetaDate = doc.MetaDate

                    }).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning("Fetch failed Url={0} Error={1}", Startingpath, e.Message);

            }
        }



    }








    public static class SafeSave
    {
        public static async Task<bool> SaveOrBackupAsync<TMain, TBackup>(
                KBContext context,
                TMain entity,
                Func<TMain, TBackup> cloneToBackup)
                where TMain : class
                where TBackup : class
        {
            try
            {
                context.Set<TMain>().Add(entity);
                await context.SaveChangesAsync().ConfigureAwait(false);
                return true;
            }
            catch
            {
                // Detach the failed entity so EF stops tracking it
                context.Entry(entity).State = EntityState.Detached;

                // Convert to backup entity (same shape)
                TBackup backup = cloneToBackup(entity);

                context.Set<TBackup>().Add(backup);
                await context.SaveChangesAsync().ConfigureAwait(false);

                return false;
            }
        }
    }





    /// <summary>Indicates whether the ingestion source is a remote web URL or a local file-system path.</summary>
    public enum IngestionSourceMode
    {
        /// <summary>Content is fetched from a remote web URL.</summary>
        Web,

        /// <summary>Content is read from the local file system.</summary>
        FileSystem
    }
}