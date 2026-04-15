// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         SqlTableMaint.cs
// Author: Kyle L. Crowder
// Build Num: 194450



using System.Data;
using System.Diagnostics;

using AgentAILib.Utils;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;




namespace AgentAILib.DocIngestion;





public sealed class SqlTableMaint
{
    private readonly int _batchSize;

    private readonly Func<string> _connectionStringProvider;
    private readonly ILogger<SqlTableMaint> _logger;
    private readonly ChunkMetadataGenerator _metadataGenerator;
    private const int DefaultBatchSize = 5;
    private const int SqlCommandTimeoutSeconds = 120;








    public SqlTableMaint(string remoteRagConnectionString, ChunkMetadataGenerator metadataGenerator, ILogger<SqlTableMaint> logger)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(remoteRagConnectionString);
        ArgumentNullException.ThrowIfNull(metadataGenerator);
        ArgumentNullException.ThrowIfNull(logger);

        _connectionStringProvider = () => ResolveConnectionString(remoteRagConnectionString);
        _metadataGenerator = metadataGenerator;
        _logger = logger;
        _batchSize = DefaultBatchSize;
    }








    internal SqlTableMaint(Func<string> connectionStringProvider, ChunkMetadataGenerator metadataGenerator, ILogger<SqlTableMaint> logger, int batchSize = DefaultBatchSize)
    {
        ArgumentNullException.ThrowIfNull(connectionStringProvider);
        ArgumentNullException.ThrowIfNull(metadataGenerator);
        ArgumentNullException.ThrowIfNull(logger);

        if (batchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(batchSize), batchSize, "Batch size must be greater than zero.");
        }

        _connectionStringProvider = connectionStringProvider;
        _metadataGenerator = metadataGenerator;
        _logger = logger;
        _batchSize = batchSize;
    }








    private async Task<IReadOnlyList<PendingChunk>> FetchPendingChunkBatchAsync(CancellationToken cancellationToken)
    {
        const string query = """
                             SELECT TOP (@batchSize)
                                 [chunk_id],
                                 [content]
                             FROM [dbo].[md_document_chunks]
                             WHERE summary is null
                             """;

        List<PendingChunk> pendingChunks = [];

        await using SqlConnection connection = await OpenRemoteRagConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using SqlCommand command = new(query, connection) { CommandType = CommandType.Text, CommandTimeout = SqlCommandTimeoutSeconds };

        _ = command.Parameters.Add("@batchSize", SqlDbType.Int);
        command.Parameters["@batchSize"].Value = _batchSize;

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            Guid chunkId = reader.GetGuid(reader.GetOrdinal("chunk_id"));

            var content = reader.GetString(reader.GetOrdinal("content"));

            pendingChunks.Add(new PendingChunk(chunkId, content));
        }

        return pendingChunks;
    }








    private static bool IsNullOrWhiteSpace(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) || string.IsNullOrWhiteSpace(reader.GetString(ordinal));
    }








    private async Task<SqlConnection> OpenRemoteRagConnectionAsync(CancellationToken cancellationToken)
    {
        var connectionString = _connectionStringProvider();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Remote RAG connection string is not configured.");
        }

        SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        return connection;
    }








    private async Task PersistChunkMetadataAsync(PendingChunk pendingChunk, GeneratedChunkMetadata metadata, CancellationToken cancellationToken)
    {
        const string updateStatement = """
                                       UPDATE [dbo].[md_document_chunks]
                                       SET
                                           [keywords] = CASE
                                               WHEN NULLIF(LTRIM(RTRIM([keywords])), N'') IS NULL THEN COALESCE(@keywords, [keywords])
                                               ELSE [keywords]
                                           END,
                                           [summary] = CASE
                                               WHEN NULLIF(LTRIM(RTRIM([summary])), N'') IS NULL THEN COALESCE(@summary, [summary])
                                               ELSE [summary]
                                           END
                                       WHERE [chunk_id] = @chunk_id;
                                       """;

        await using SqlConnection connection = await OpenRemoteRagConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using SqlCommand command = new(updateStatement, connection) { CommandType = CommandType.Text, CommandTimeout = SqlCommandTimeoutSeconds };

        _ = command.Parameters.Add("@chunk_id", SqlDbType.UniqueIdentifier);
        _ = command.Parameters.Add("@keywords", SqlDbType.NVarChar, -1);
        _ = command.Parameters.Add("@summary", SqlDbType.NVarChar, 4000);

        command.Parameters["@chunk_id"].Value = pendingChunk.ChunkId;
        command.Parameters["@keywords"].Value = metadata.Keywords is null ? DBNull.Value : metadata.Keywords;
        command.Parameters["@summary"].Value = metadata.Summary is null ? DBNull.Value : metadata.Summary;

        var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        if (rowsAffected != 1)
        {
            throw new InvalidOperationException($"Expected to update one chunk row for '{pendingChunk.ChunkId}', but updated {rowsAffected} rows.");
        }
    }








    internal async Task<MetadataUpdateResult> ProcessPendingChunksAsync(IReadOnlyList<PendingChunk> pendingChunks, Func<PendingChunk, GeneratedChunkMetadata, CancellationToken, Task> persistAsync, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pendingChunks);
        ArgumentNullException.ThrowIfNull(persistAsync);

        var updatedCount = 0;
        var failedCount = 0;

        foreach (PendingChunk pendingChunk in pendingChunks)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                GeneratedChunkMetadata metadata = await _metadataGenerator.GenerateAsync(pendingChunk.Content, cancellationToken).ConfigureAwait(false);

                await persistAsync(pendingChunk, metadata, cancellationToken).ConfigureAwait(false);
                updatedCount++;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                failedCount++;
                _logger.LogWarning(exception, "Failed to update metadata for chunk {ChunkId}Continuing with the remaining batch.", pendingChunk.ChunkId);

            }
        }

        return new MetadataUpdateResult(updatedCount, failedCount);
    }








    private static string ResolveConnectionString(string remoteRagConnectionString)
    {
        return !string.IsNullOrWhiteSpace(remoteRagConnectionString) ? remoteRagConnectionString : Environment.GetEnvironmentVariable("REMOTE_RAG") ?? string.Empty;
    }








    public async Task<MetadataUpdateResult> UpdateMetadataAsync(CancellationToken cancellationToken)
    {
        var updatedCount = 0;
        var failedCount = 0;
        var batchNumber = 0;

        _logger.LogInformation("Starting markdown chunk metadata update with batch size {BatchSize}.", _batchSize);
        Stopwatch sw = Stopwatch.StartNew();
        try
        {
            var loopCount = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var pendingChunks = await FetchPendingChunkBatchAsync(cancellationToken).ConfigureAwait(false);
                if (pendingChunks.Count == 0)
                {
                    MetadataUpdateResult completedResult = new(updatedCount, failedCount);
                    _logger.LogInformation("Markdown chunk metadata update completed. Updated {UpdatedCount} chunk(s); {FailedCount} failed.", completedResult.UpdatedCount, completedResult.FailedCount);
                    _logger.LogInformation("Completed metadata batch {BatchNumber}. Total updated so far: {TotalUpdatedCount}; total failed so far: {TotalFailedCount}. LoopCount {loopcount} took: {time}", batchNumber, updatedCount, failedCount, loopCount, sw.Elapsed.TotalSeconds);
                    return completedResult;
                }

                batchNumber++;
                _logger.LogInformation("Processing metadata batch {BatchNumber} containing {ChunkCount} chunk(s).", batchNumber, pendingChunks.Count);

                MetadataUpdateResult batchResult = await ProcessPendingChunksAsync(pendingChunks, PersistChunkMetadataAsync, cancellationToken).ConfigureAwait(false);

                updatedCount += batchResult.UpdatedCount;
                failedCount += batchResult.FailedCount;
                loopCount++;
                _logger.LogInformation("Completed metadata batch {BatchNumber}. Updated {BatchUpdatedCount} chunk(s); {BatchFailedCount} failed. Total updated so far: {TotalUpdatedCount}; total failed so far: {TotalFailedCount}. LoopCount {loopcount} took: {time}", batchNumber, batchResult.UpdatedCount, batchResult.FailedCount, updatedCount, failedCount, loopCount, sw.Elapsed.TotalSeconds);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Markdown chunk metadata update was canceled after updating {UpdatedCount} chunk(s); {FailedCount} failed.", updatedCount, failedCount);
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An error occurred during markdown chunk metadata update after updating {UpdatedCount} chunk(s); {FailedCount} failed.", updatedCount, failedCount);
            throw;
        }
        finally
        {
            sw.Stop();
            _logger.LogInformation("Markdown chunk metadata update process finished in {ElapsedSeconds} seconds.", sw.Elapsed.TotalSeconds);
        }
    }








    internal readonly record struct PendingChunk(Guid ChunkId, string Content);
}





public readonly record struct MetadataUpdateResult(int UpdatedCount, int FailedCount);