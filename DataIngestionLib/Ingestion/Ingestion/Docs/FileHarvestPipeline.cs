// 2026/03/04
//  Solution: DataIngestionService
//  Project:   DataIngestionService
//  File:         FileHarvestPipeline.cs
//   Author: Kyle L. Crowder



using Microsoft.Extensions.Logging;

using DataIngestionLib.EFModels;




namespace DataIngestionLib.DocIngestion;





/// <summary>
///     Reads all files from a local directory tree and routes them through the ingestion pipeline.
///     Acts as the entry point for file-system based ingestion; the actual persistence is
///     delegated to <see cref="SaveInSqlDatabase" /> once that method is fully implemented.
/// </summary>
internal class FileHarvestPipeline
{
    private readonly ILogger<FileHarvestPipeline> _logger;








    /// <summary>
    ///     Initializes a new instance of <see cref="FileHarvestPipeline" />.
    /// </summary>
    /// <param name="logger">Structured logger injected by the DI container.</param>
    public FileHarvestPipeline(ILogger<FileHarvestPipeline> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }








    /// <summary>
    ///     Enumerates all file paths under <paramref name="rootPath" /> recursively.
    /// </summary>
    /// <param name="rootPath">The root directory to enumerate.</param>
    /// <returns>A list of absolute file paths.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="rootPath" /> is null or whitespace.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the directory does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when access is denied or an I/O error occurs during enumeration.</exception>
    private IList<string> EnumerateFilePaths(string rootPath)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            throw new ArgumentException("Root path cannot be null or empty.", nameof(rootPath));
        }

        if (!Directory.Exists(rootPath))
        {
            throw new DirectoryNotFoundException($"The directory '{rootPath}' does not exist.");
        }

        List<string> filePaths = [];

        try
        {
            filePaths.AddRange(Directory.EnumerateFiles(rootPath, "*", SearchOption.AllDirectories));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "EnumerateFilePaths: access denied while enumerating '{RootPath}'.", rootPath);
            throw new InvalidOperationException("Access to one or more directories was denied.", ex);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "EnumerateFilePaths: I/O error while enumerating '{RootPath}'.", rootPath);
            throw new InvalidOperationException("An I/O error occurred while enumerating files.", ex);
        }

        return filePaths;
    }








    /// <summary>
    ///     Reads all files under <paramref name="sourcePath" /> and triggers persistence for each one.
    /// </summary>
    /// <param name="sourcePath">The root directory to harvest. Must be non-empty and must exist.</param>
    /// <returns>A task that completes when all files have been processed.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="sourcePath" /> is null or whitespace.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the directory does not exist.</exception>
    public async Task RunAsync(string sourcePath)
    {
        var files = EnumerateFilePaths(sourcePath);
        _logger.LogInformation("FileHarvestPipeline: processing {Count} files from '{SourcePath}'.", files.Count, sourcePath);

        foreach (var file in files)
        {
            var content = await File.ReadAllTextAsync(file).ConfigureAwait(false);
            SaveInSqlDatabase(file, content);
        }

        _logger.LogInformation("FileHarvestPipeline: harvest complete for '{SourcePath}'.", sourcePath);
    }








    /// <summary>
    ///     Persists a single file's content to the knowledge-base SQL database.
    /// </summary>
    /// <param name="file">The absolute file path.</param>
    /// <param name="content">The raw file content.</param>
    /// <remarks>
    ///     TODO: Implement full persistence logic using the EF <see cref="KBContext" /> and the
    ///     temporal-versioning stored procedures to avoid creating duplicate records on re-ingestion.
    /// </remarks>
    private void SaveInSqlDatabase(string file, string content)
    {
        // TODO: Implement persistence of the file and content into the SQL database.
        // This stub intentionally does nothing to avoid failing the pipeline.
        _logger.LogDebug("SaveInSqlDatabase: stub called for '{File}' ({Length} chars).", file, content?.Length ?? 0);
    }
}