// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   AgentAILib
// File:         DocIngestionPipeline.cs
// Author: Kyle L. Crowder
// Build Num: 212857



using System.Data;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

using AgentAILib.Utils;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;




namespace AgentAILib.DocIngestion;





public sealed class DocIngestionPipeline
{
    private readonly ILoggerFactory? _factory;
    private readonly ChunkMetadataGenerator _generator;
    private readonly ILogger<DocIngestionPipeline> _logger;
    private const int MaxIncludeDepth = 8;
    private const string StartFolder = "e:\\IngestionSource\\docs\\docs\\ai";
    private static readonly Regex ControlCharacterRegex = new("[\\u0000-\\u0008\\u000B\\u000C\\u000E-\\u001F\\u007F]", RegexOptions.Compiled);
    private static readonly Regex DirectiveAttributeRegex = new("(?<key>[A-Za-z0-9_-]+)\\s*=\\s*\"(?<value>[^\"]*)\"", RegexOptions.Compiled);
    private static readonly Regex FencedCodeStartRegex = new(@"^```(?<lang>[^\s`]*)", RegexOptions.Compiled);

    private static readonly Regex HeadingRegex = new(@"^(#{1,6})\s+(.*)$", RegexOptions.Compiled);
    private static readonly Regex HtmlLinkRegex = new(@"<(?<url>https?://[^>]+)>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex MarkdownEscapeRegex = new(@"\\([\\`*_{}\[\]()#+\-.!|>~])", RegexOptions.Compiled);
    private static readonly Regex MarkdownImageRegex = new(@"!\[(?<text>[^\]]*)\]\((?<url>[^\)]+)\)", RegexOptions.Compiled);
    private static readonly Regex MarkdownLinkRegex = new(@"\[(?<text>[^\]]+)\]\((?<url>[^\)]+)\)", RegexOptions.Compiled);
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex XrefRegex = new(@"<xref:(?<xref>[^>]+)>", RegexOptions.Compiled | RegexOptions.IgnoreCase);








    public DocIngestionPipeline(ILoggerFactory factory, ChunkMetadataGenerator generator)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(generator);
        _generator = generator;
        _factory = factory;
        _logger = _factory.CreateLogger<DocIngestionPipeline>();
    }








    internal static IReadOnlyList<ChunkPayload> BuildChunks(string markdownBody)
    {
        List<ChunkPayload> chunks = [];
        List<(int Level, string Heading)> headingStack = [];

        var lines = markdownBody.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        StringBuilder prose = new();
        var chunkIndex = 0;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            Match headingMatch = HeadingRegex.Match(line);
            if (headingMatch.Success)
            {
                FlushProseChunk(chunks, prose, headingStack, ref chunkIndex);

                var level = headingMatch.Groups[1].Value.Length;
                var heading = headingMatch.Groups[2].Value.Trim();

                while (headingStack.Count > 0 && headingStack[^1].Level >= level)
                {
                    headingStack.RemoveAt(headingStack.Count - 1);
                }

                headingStack.Add((level, heading));
                continue;
            }

            Match fenceMatch = FencedCodeStartRegex.Match(line);
            if (fenceMatch.Success)
            {
                FlushProseChunk(chunks, prose, headingStack, ref chunkIndex);
                i++;

                while (i < lines.Length && !lines[i].StartsWith("```", StringComparison.Ordinal))
                {
                    i++;
                }

                continue;
            }

            _ = prose.AppendLine(line);
        }

        FlushProseChunk(chunks, prose, headingStack, ref chunkIndex);
        return chunks;
    }








    private static string BuildCodeFence(string language, string code)
    {
        StringBuilder builder = new();
        _ = builder.Append("```");
        if (!string.IsNullOrWhiteSpace(language))
        {
            _ = builder.Append(language.Trim());
        }

        _ = builder.AppendLine();
        _ = builder.AppendLine(code.TrimEnd());
        _ = builder.Append("```");
        return builder.ToString();
    }








    private static string BuildHeadingPath(IReadOnlyList<(int Level, string Heading)> headingStack)
    {
        return headingStack.Count == 0 ? string.Empty : string.Join(" > ", headingStack.Select(x => x.Heading));
    }








    private static string CollectDirectiveText(string[] lines, ref int index)
    {
        StringBuilder builder = new();
        var firstLine = lines[index];
        _ = builder.AppendLine(firstLine);

        if (firstLine.TrimEnd().EndsWith(":::", StringComparison.Ordinal))
        {
            index++;
            return builder.ToString();
        }

        index++;
        while (index < lines.Length)
        {
            var line = lines[index];
            _ = builder.AppendLine(line);
            index++;

            if (line.Trim() == ":::")
            {
                break;
            }
        }

        return builder.ToString();
    }








    private static string ComputeSha256Hex(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }








    private static int CountWords(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? 0 : value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
    }








    public async Task DoIngestionAsync(string defaultLearnBaseUrl, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(StartFolder))
        {
            _logger.LogWarning("Markdown ingestion folder does not exist: {StartFolder}", StartFolder);
            return;
        }

        var markdownFiles = Directory.GetFiles(StartFolder, "*.md", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray();

        _logger.LogInformation("Found {FileCount} markdown files for ingestion in {StartFolder}", markdownFiles.Length, StartFolder);

        foreach (var filePath in markdownFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var existingDocumentHash = await GetExistingDocumentHashAsync(filePath, cancellationToken).ConfigureAwait(false);
            try
            {
                ParsedDocumentPayload parsedDocument = await ParseMarkdownDocumentAsync(filePath, cancellationToken).ConfigureAwait(false);

                if (ComputeSha256Hex(parsedDocument.NormalizedMarkdown) == existingDocumentHash)
                {
                    _logger.LogInformation("Skipping unchanged document: {FilePath}", filePath);
                    continue;
                }

                Guid documentId = await SaveDocumentSqlStubAsync(parsedDocument, cancellationToken).ConfigureAwait(false);
                await SaveChunksSqlStubAsync(documentId, parsedDocument.Chunks, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to ingest markdown document: {FilePath}", filePath);
            }
        }

        _logger.LogInformation("Markdown ingestion completed.");
    }








    private static int EstimateTokenCount(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return 0;
        }

        var wordCount = content.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;

        return Math.Max(1, (int)Math.Ceiling(wordCount * 1.35));
    }








    private static string ExtractSnippetById(string sourceCode, string snippetId)
    {
        var startMarker = $"<\u002F?{snippetId}>";
        MatchCollection markerMatches = Regex.Matches(sourceCode, startMarker, RegexOptions.IgnoreCase);

        if (markerMatches.Count < 2)
        {
            return sourceCode;
        }

        var start = markerMatches[0].Index + markerMatches[0].Length;
        var end = markerMatches[1].Index;
        return end <= start ? sourceCode : sourceCode[start..end].Trim();
    }








    private static (string yamlFrontMatter, string body) ExtractYamlFrontMatter(string markdown)
    {
        var normalized = markdown.Replace("\r\n", "\n", StringComparison.Ordinal);
        if (!normalized.StartsWith("---\n", StringComparison.Ordinal))
        {
            return (string.Empty, markdown);
        }

        var end = normalized.IndexOf("\n---\n", 4, StringComparison.Ordinal);
        if (end < 0)
        {
            return (string.Empty, markdown);
        }

        var yaml = normalized[..(end + 5)].TrimEnd();
        var body = normalized[(end + 5)..].TrimStart('\n');

        return (yaml, body);
    }








    private static string FlattenHyperlinks(string markdown)
    {
        var flattened = MarkdownImageRegex.Replace(markdown, m => m.Groups["text"].Value.Trim());
        flattened = MarkdownLinkRegex.Replace(flattened, m =>
        {
            var text = m.Groups["text"].Value.Trim();
            var url = m.Groups["url"].Value.Trim();
            return string.IsNullOrWhiteSpace(text) ? url : $"{text} ({url})";
        });
        flattened = HtmlLinkRegex.Replace(flattened, m => m.Groups["url"].Value.Trim());
        flattened = XrefRegex.Replace(flattened, m => m.Groups["xref"].Value.Trim());

        return flattened;
    }








    private static void FlushProseChunk(ICollection<ChunkPayload> chunks, StringBuilder prose, IReadOnlyList<(int Level, string Heading)> headingStack, ref int chunkIndex)
    {
        var content = prose.ToString().Trim();
        _ = prose.Clear();

        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        var headingPath = BuildHeadingPath(headingStack);
        var heading = headingStack.Count == 0 ? string.Empty : headingStack[^1].Heading;
        int? headingLevel = headingStack.Count == 0 ? null : headingStack[^1].Level;

        chunks.Add(new ChunkPayload(chunkIndex, heading, headingLevel, headingPath, "text", null, content, EstimateTokenCount(content)));

        chunkIndex++;
    }








    private async Task<string?> GetExistingDocumentHashAsync(string filePath, CancellationToken cancellationToken)
    {
        await using SqlConnection connection = await OpenRemoteRagConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using SqlCommand command = new(@"SELECT [content_hash] FROM [dbo].[md_documents] WHERE [file_path] = @file_path", connection);
        command.Parameters.Add("@file_path", SqlDbType.NVarChar, 512).Value = Truncate(Path.GetRelativePath(StartFolder, filePath), 512);
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var hash = reader.GetString(0);
            return hash;
        }

        return null;

    }








    private static string NormalizeMarkdownForIngestion(string markdown)
    {
        var flattened = FlattenHyperlinks(markdown);
        var withoutCodeBlocks = RemoveFencedCodeBlocks(flattened);
        var withoutControlChars = RemoveControlCharacters(withoutCodeBlocks);
        var withoutEscapes = RemoveMarkdownEscapes(withoutControlChars);
        return NormalizeWhitespace(withoutEscapes);
    }








    private static string NormalizeWhitespace(string markdown)
    {
        var lines = markdown.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        StringBuilder builder = new();
        var previousLineWasBlank = true;

        foreach (var rawLine in lines)
        {
            var line = WhitespaceRegex.Replace(rawLine, " ").Trim();
            if (line.Length == 0)
            {
                if (previousLineWasBlank)
                {
                    continue;
                }

                _ = builder.AppendLine();
                previousLineWasBlank = true;
                continue;
            }

            _ = builder.AppendLine(line);
            previousLineWasBlank = false;
        }

        return builder.ToString().Trim();
    }








    private static async Task<SqlConnection> OpenRemoteRagConnectionAsync(CancellationToken cancellationToken)
    {
        var connectionString = Environment.GetEnvironmentVariable("REMOTE_RAG");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Environment variable 'REMOTE_RAG' is not set.");
        }

        SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        return connection;
    }








    private static Dictionary<string, string> ParseDirectiveAttributes(string directiveText)
    {
        Dictionary<string, string> attributes = new(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in DirectiveAttributeRegex.Matches(directiveText))
        {
            var key = match.Groups["key"].Value;
            var value = match.Groups["value"].Value;

            _ = attributes.TryAdd(key, value);
        }

        return attributes;
    }








    internal async Task<ParsedDocumentPayload> ParseMarkdownDocumentAsync(string filePath, CancellationToken cancellationToken)
    {
        var rawFileContent = await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
        var normalizedMarkdown = ParserTools.CleanRawText(rawFileContent);
        var (yamlFrontMatter, markdownBody) = ExtractYamlFrontMatter(normalizedMarkdown);

        HashSet<string> includeStack = new(StringComparer.OrdinalIgnoreCase);
        var resolvedMarkdown = await ResolveDocFxDirectivesAsync(filePath, markdownBody, includeStack, 0, cancellationToken).ConfigureAwait(false);
        var normalizedMarkdownBody = NormalizeMarkdownForIngestion(resolvedMarkdown);
        var normalizedDocument = string.IsNullOrWhiteSpace(yamlFrontMatter) ? normalizedMarkdownBody : $"{yamlFrontMatter}{Environment.NewLine}{Environment.NewLine}{normalizedMarkdownBody}";

        var chunks = BuildChunks(normalizedMarkdownBody);

        var relativePath = Path.GetRelativePath(StartFolder, filePath);

        return new ParsedDocumentPayload(Guid.NewGuid(), relativePath, Path.GetFileName(filePath), yamlFrontMatter, normalizedMarkdown, normalizedDocument, chunks);
    }








    internal static (string? title, string? description, string? author, DateTime? msDate, string? msTopic) ParseYamlFields(string yamlFrontMatter)
    {
        if (string.IsNullOrWhiteSpace(yamlFrontMatter))
        {
            return (null, null, null, null, null);
        }

        Dictionary<string, string> fields = new(StringComparer.OrdinalIgnoreCase);
        var lines = yamlFrontMatter.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line == "---" || line.StartsWith('#'))
            {
                continue;
            }

            var separatorIndex = line.IndexOf(':');
            if (separatorIndex <= 0 || separatorIndex == line.Length - 1)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim().Trim('"', '\'');

            _ = fields.TryAdd(key, value);
        }

        DateTime? msDate = null;
        if (fields.TryGetValue("ms.date", out var msDateRaw) && !string.IsNullOrWhiteSpace(msDateRaw))
        {
            if (DateTime.TryParse(msDateRaw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime parsed))
            {
                msDate = parsed.Date;
            }
        }

        _ = fields.TryGetValue("title", out var title);
        _ = fields.TryGetValue("description", out var description);
        _ = fields.TryGetValue("author", out var author);
        _ = fields.TryGetValue("ms.topic", out var msTopic);

        return (title, description, author, msDate, msTopic);
    }








    private static string RemoveControlCharacters(string markdown)
    {
        return ControlCharacterRegex.Replace(markdown, string.Empty);
    }








    private static string RemoveFencedCodeBlocks(string markdown)
    {
        var lines = markdown.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        StringBuilder builder = new();

        var inFence = false;
        string? fenceMarker = null;

        foreach (var line in lines)
        {
            var trimmed = line.TrimStart();

            if (!inFence && (trimmed.StartsWith("```", StringComparison.Ordinal) || trimmed.StartsWith("~~~", StringComparison.Ordinal)))
            {
                inFence = true;
                fenceMarker = trimmed[..3];
                continue;
            }

            if (inFence)
            {
                if (fenceMarker is not null && trimmed.StartsWith(fenceMarker, StringComparison.Ordinal))
                {
                    inFence = false;
                    fenceMarker = null;
                }

                continue;
            }

            _ = builder.AppendLine(line);
        }

        return builder.ToString();
    }








    private static string RemoveMarkdownEscapes(string markdown)
    {
        return MarkdownEscapeRegex.Replace(markdown, "$1");
    }








    /// <summary>
    ///     Resolves a `:::code` directive.
    /// </summary>
    /// <param name="owningFilePath">
    ///     The file path of the document that contains the `:::code` directive.
    /// </param>
    /// <param name="directiveText">
    ///     The text of the `:::code` directive, including its attributes.
    /// </param>
    /// <param name="cancellationToken">
    ///     A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is always an empty string,
    ///     because code content is intentionally discarded during ingestion normalization.
    /// </returns>
    /// <remarks>
    ///     Code content from directives is intentionally skipped so ingestion persists only prose content.
    /// </remarks>
    internal async Task<string> ResolveCodeDirectiveAsync(string owningFilePath, string directiveText, CancellationToken cancellationToken)
    {
        _ = directiveText;
        _ = cancellationToken;
        _logger.LogDebug("Skipping :::code directive content for {FilePath} during ingestion normalization.", owningFilePath);
        await Task.CompletedTask.ConfigureAwait(false);
        return string.Empty;
    }








    internal async Task<string> ResolveDocFxDirectivesAsync(string owningFilePath, string markdown, HashSet<string> includeStack, int depth, CancellationToken cancellationToken)
    {
        if (depth > MaxIncludeDepth)
        {
            _logger.LogWarning("Max include recursion depth reached while processing {FilePath}", owningFilePath);
            return markdown;
        }

        var normalizedPath = Path.GetFullPath(owningFilePath);
        if (!includeStack.Add(normalizedPath))
        {
            _logger.LogWarning("Circular include detected for {FilePath}; include skipped.", owningFilePath);
            return string.Empty;
        }

        try
        {
            StringBuilder output = new();
            var lines = markdown.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
            var index = 0;

            while (index < lines.Length)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var line = lines[index];
                var trimmed = line.Trim();

                if (trimmed.StartsWith(":::code", StringComparison.OrdinalIgnoreCase))
                {
                    var directiveText = CollectDirectiveText(lines, ref index);
                    var codeBlock = await ResolveCodeDirectiveAsync(owningFilePath, directiveText, cancellationToken).ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(codeBlock))
                    {
                        _ = output.AppendLine(codeBlock);
                    }

                    continue;
                }

                if (trimmed.StartsWith(":::include", StringComparison.OrdinalIgnoreCase))
                {
                    var directiveText = CollectDirectiveText(lines, ref index);
                    var includeContent = await ResolveIncludeDirectiveAsync(owningFilePath, directiveText, includeStack, depth + 1, cancellationToken).ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(includeContent))
                    {
                        _ = output.AppendLine(includeContent);
                    }

                    continue;
                }

                _ = output.AppendLine(line);
                index++;
            }

            return output.ToString();
        }
        finally
        {
            _ = includeStack.Remove(normalizedPath);
        }
    }








    internal async Task<string> ResolveIncludeDirectiveAsync(string owningFilePath, string directiveText, HashSet<string> includeStack, int depth, CancellationToken cancellationToken)
    {
        var attributes = ParseDirectiveAttributes(directiveText);
        if (!attributes.TryGetValue("file", out var includePath) && !attributes.TryGetValue("path", out includePath) && !attributes.TryGetValue("source", out includePath))
        {
            _logger.LogDebug(":::include directive in {FilePath} does not declare file/path/source.", owningFilePath);
            return string.Empty;
        }

        var includeAbsolutePath = ResolveRelativePath(owningFilePath, includePath);
        if (!File.Exists(includeAbsolutePath))
        {
            _logger.LogWarning("Included file not found: {IncludePath} referenced by {FilePath}", includeAbsolutePath, owningFilePath);
            return string.Empty;
        }

        var includeMarkdown = await File.ReadAllTextAsync(includeAbsolutePath, cancellationToken).ConfigureAwait(false);
        return await ResolveDocFxDirectivesAsync(includeAbsolutePath, includeMarkdown, includeStack, depth, cancellationToken).ConfigureAwait(false);
    }








    internal static string ResolveRelativePath(string owningFilePath, string path)
    {
        if (Path.IsPathRooted(path))
        {
            return path;
        }

        var owningDirectory = Path.GetDirectoryName(owningFilePath) ?? StartFolder;
        return Path.GetFullPath(Path.Combine(owningDirectory, path));
    }








    internal async Task SaveChunksSqlStubAsync(Guid documentId, IReadOnlyList<ChunkPayload> chunks, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await using SqlConnection connection = await OpenRemoteRagConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using SqlTransaction transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {


            await using SqlCommand insertCommand = new(@"
INSERT INTO [dbo].[md_document_chunks]
(
    [doc_id],
    [chunk_index],
    [heading_path],
    [heading],
    [heading_level],
    [chunk_type],
    [language],
    [content],
    [content_hash],
    [token_count],
[summary],
[keywords],
[embeddings]
)
VALUES
(
    @doc_id,
    @chunk_index,
    @heading_path,
    @heading,
    @heading_level,
    @chunk_type,
    @language,
    @content,
@content_hash,
    @token_count,
@summary,
@keywords,
CAST(@embeddings AS vector(1024))
);", connection, transaction);

            _ = insertCommand.Parameters.Add("@doc_id", SqlDbType.UniqueIdentifier);
            _ = insertCommand.Parameters.Add("@chunk_index", SqlDbType.Int);
            _ = insertCommand.Parameters.Add("@heading_path", SqlDbType.NVarChar, 1024);
            _ = insertCommand.Parameters.Add("@heading", SqlDbType.NVarChar, 512);
            _ = insertCommand.Parameters.Add("@heading_level", SqlDbType.TinyInt);
            _ = insertCommand.Parameters.Add("@chunk_type", SqlDbType.NVarChar, 16);
            _ = insertCommand.Parameters.Add("@language", SqlDbType.NVarChar, 64);
            _ = insertCommand.Parameters.Add("@content", SqlDbType.NVarChar);
            _ = insertCommand.Parameters.Add("@content_hash", SqlDbType.NVarChar, 64);
            _ = insertCommand.Parameters.Add("@token_count", SqlDbType.Int);
            _ = insertCommand.Parameters.Add("@summary", SqlDbType.NVarChar);
            _ = insertCommand.Parameters.Add("@keywords", SqlDbType.NVarChar, 400);
            _ = insertCommand.Parameters.Add("@embeddings", SqlDbType.NVarChar, -1);

            foreach (ChunkPayload chunk in chunks)
            {
                cancellationToken.ThrowIfCancellationRequested();
                GeneratedChunkMetadata metadata = await _generator.GenerateAsync(chunk.Content, cancellationToken).ConfigureAwait(false);
                insertCommand.Parameters["@doc_id"].Value = documentId;
                insertCommand.Parameters["@chunk_index"].Value = chunk.ChunkIndex;
                insertCommand.Parameters["@heading_path"].Value = ToDbValue(Truncate(chunk.HeadingPath, 1024));
                insertCommand.Parameters["@heading"].Value = ToDbValue(Truncate(chunk.Heading, 512));
                insertCommand.Parameters["@heading_level"].Value = ToDbValue(chunk.HeadingLevel);
                insertCommand.Parameters["@chunk_type"].Value = Truncate(chunk.ChunkType, 16);
                insertCommand.Parameters["@language"].Value = ToDbValue(Truncate(chunk.Language, 64));
                insertCommand.Parameters["@content"].Value = chunk.Content;
                insertCommand.Parameters["@content_hash"].Value = ComputeSha256Hex(chunk.Content);
                insertCommand.Parameters["@token_count"].Value = chunk.TokenCount;
                insertCommand.Parameters["@summary"].Value = metadata.Summary;
                insertCommand.Parameters["@keywords"].Value = metadata.Keywords;
                insertCommand.Parameters["@embeddings"].Value = await _generator.GenerateEmbeddingsAsync(chunk.Content, cancellationToken).ConfigureAwait(false);

                _ = await insertCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
        }

        _logger.LogInformation("Saved markdown chunks: DocumentId={DocumentId}, ChunkCount={ChunkCount}", documentId, chunks.Count);
    }








    internal async Task<Guid> SaveDocumentSqlStubAsync(ParsedDocumentPayload document, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var (title, description, author, msDate, msTopic) = ParseYamlFields(document.YamlFrontMatter);
        var contentHash = ComputeSha256Hex(document.NormalizedMarkdown);
        var wordCount = CountWords(document.NormalizedMarkdown);

        await using SqlConnection connection = await OpenRemoteRagConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using SqlCommand command = new(@"
SET NOCOUNT ON;

DECLARE @resolved_doc_id UNIQUEIDENTIFIER;

SELECT @resolved_doc_id = [doc_id]
FROM [dbo].[md_documents]
WHERE [file_path] = @file_path;

IF @resolved_doc_id IS NULL
BEGIN
    SET @resolved_doc_id = @doc_id;

    INSERT INTO [dbo].[md_documents]
    (
        [doc_id],
        [file_path],
        [file_name],
        [title],
        [description],
        [author],
        [ms_date],
        [ms_topic],
        [yaml_front_matter],
        [content_raw],
        [content_normalized],
        [content_hash],
        [word_count],
        [updated_at]
    )
    VALUES
    (
        @resolved_doc_id,
        @file_path,
        @file_name,
        @title,
        @description,
        @author,
        @ms_date,
        @ms_topic,
        @yaml_front_matter,
        @content_raw,
        @content_normalized,
        @content_hash,
        @word_count,
        SYSUTCDATETIME()
    );
END
ELSE
BEGIN
    UPDATE [dbo].[md_documents]
    SET
        [file_name] = @file_name,
        [title] = @title,
        [description] = @description,
        [author] = @author,
        [ms_date] = @ms_date,
        [ms_topic] = @ms_topic,
        [yaml_front_matter] = @yaml_front_matter,
        [content_raw] = @content_raw,
        [content_normalized] = @content_normalized,
        [content_hash] = @content_hash,
        [word_count] = @word_count,
        [updated_at] = SYSUTCDATETIME()
    WHERE [doc_id] = @resolved_doc_id;
END

SELECT @resolved_doc_id;", connection);

        command.Parameters.Add("@doc_id", SqlDbType.UniqueIdentifier).Value = document.DocumentId;
        command.Parameters.Add("@file_path", SqlDbType.NVarChar, 512).Value = Truncate(document.RelativePath, 512);
        command.Parameters.Add("@file_name", SqlDbType.NVarChar, 260).Value = Truncate(document.FileName, 260);
        command.Parameters.Add("@title", SqlDbType.NVarChar, 512).Value = ToDbValue(Truncate(title, 512));
        command.Parameters.Add("@description", SqlDbType.NVarChar, 2048).Value = ToDbValue(Truncate(description, 2048));
        command.Parameters.Add("@author", SqlDbType.NVarChar, 256).Value = ToDbValue(Truncate(author, 256));
        command.Parameters.Add("@ms_date", SqlDbType.Date).Value = ToDbValue(msDate);
        command.Parameters.Add("@ms_topic", SqlDbType.NVarChar, 256).Value = ToDbValue(Truncate(msTopic, 256));
        command.Parameters.Add("@yaml_front_matter", SqlDbType.NVarChar, 4000).Value = ToDbValue(Truncate(document.YamlFrontMatter, 4000));
        command.Parameters.Add("@content_raw", SqlDbType.NVarChar).Value = document.RawMarkdown;
        command.Parameters.Add("@content_normalized", SqlDbType.NVarChar).Value = document.NormalizedMarkdown;
        command.Parameters.Add("@content_hash", SqlDbType.NVarChar, 64).Value = contentHash;
        command.Parameters.Add("@word_count", SqlDbType.Int).Value = wordCount;

        var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        Guid persistedDocumentId = result is Guid guid ? guid : document.DocumentId;

        _logger.LogInformation("Saved markdown document: {RelativePath}, DocumentId={DocumentId}, Chunks={ChunkCount}", document.RelativePath, persistedDocumentId, document.Chunks.Count);

        return persistedDocumentId;
    }








    internal static object ToDbValue<T>(T? value)
    {
        return value is null ? DBNull.Value : value is string stringValue && string.IsNullOrWhiteSpace(stringValue) ? DBNull.Value : value;
    }








    internal static string Truncate(string? value, int maxLength)
    {
        return string.IsNullOrEmpty(value) || value.Length <= maxLength ? value ?? string.Empty : value[..maxLength];
    }








    internal sealed record ParsedDocumentPayload(Guid DocumentId, string RelativePath, string FileName, string YamlFrontMatter, string RawMarkdown, string NormalizedMarkdown, IReadOnlyList<ChunkPayload> Chunks);





    internal sealed record ChunkPayload(int ChunkIndex, string Heading, int? HeadingLevel, string HeadingPath, string ChunkType, string? Language, string Content, int TokenCount);
}