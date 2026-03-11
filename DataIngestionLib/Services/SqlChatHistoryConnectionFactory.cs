// 2026/03/10
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         SqlChatHistoryConnectionFactory.cs
//   Author: Kyle L. Crowder



using DataIngestionLib.Options;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;




namespace DataIngestionLib.Services;





// Is this necessary?
public interface ISqlChatHistoryConnectionFactory
{
    ValueTask<SqlConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
}





public sealed class SqlChatHistoryConnectionFactory : ISqlChatHistoryConnectionFactory
{
    private readonly IOptionsMonitor<ChatHistoryOptions> _optionsMonitor;








    public SqlChatHistoryConnectionFactory(IOptionsMonitor<ChatHistoryOptions> optionsMonitor)
    {
        ArgumentNullException.ThrowIfNull(optionsMonitor);
        _optionsMonitor = optionsMonitor;
    }








    public async ValueTask<SqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connectionString = _optionsMonitor.CurrentValue.ConnectionString?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Chat history connection string is not configured.");
        }

        SqlConnection connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        return connection;
    }
}