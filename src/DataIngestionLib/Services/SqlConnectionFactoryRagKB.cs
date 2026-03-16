// Build Date: 2026/03/16
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         SqlConnectionFactoryRagKB.cs
// Author: Kyle L. Crowder
// Build Num: 051928



using Microsoft.Data.SqlClient;




namespace DataIngestionLib.Services;





/// <summary>
///     Provides SQL connection for RAG knowledge base.
/// </summary>
public static class SqlConnectionFactoryRagKb
{
    public static SqlConnection CreateConnection()
    {
        var connectionString = Environment.GetEnvironmentVariable("CONN_STRING2") ?? throw new InvalidOperationException("Environment variable 'CONN_STRING2' is not set.");
        return new SqlConnection(connectionString);
    }
}