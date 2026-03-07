using DataIngestionLib.Options;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace DataIngestionLib.Services;

public sealed class SqlChatHistoryConnectionFactory : IChatHistoryConnectionFactory
{
    private readonly IOptionsMonitor<ChatHistoryOptions> _optionsMonitor;

    public SqlChatHistoryConnectionFactory(IOptionsMonitor<ChatHistoryOptions> optionsMonitor)
    {
        ArgumentNullException.ThrowIfNull(optionsMonitor);
        _optionsMonitor = optionsMonitor;
    }

    public async ValueTask<SqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        string connectionString = _optionsMonitor.CurrentValue.ConnectionString;
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Chat history connection string is not configured.");
        }

        SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        return connection;
    }
}
