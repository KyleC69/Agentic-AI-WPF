// Build Date: 2026/03/13
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         SqlChatHistoryConnectionFactory.cs
// Author: Kyle L. Crowder
// Build Num: 175057



using System.Configuration;

using Microsoft.Data.SqlClient;




namespace DataIngestionLib.Services;





// Is this necessary?
public interface ISqlChatHistoryConnectionFactory
{
    ValueTask<SqlConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
}





public sealed class SqlChatHistoryConnectionFactory : ISqlChatHistoryConnectionFactory
{

    public async ValueTask<SqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connectionString = ConfigurationManager.AppSettings["ChatHistoryConnectionString"]?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Chat history connection string is not configured.");
        }

        SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        return connection;
    }
}