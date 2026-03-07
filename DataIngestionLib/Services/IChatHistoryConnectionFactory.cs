using Microsoft.Data.SqlClient;

namespace DataIngestionLib.Services;

public interface IChatHistoryConnectionFactory
{
    ValueTask<SqlConnection> OpenConnectionAsync(CancellationToken cancellationToken);
}
