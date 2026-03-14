// Build Date: 2026/03/13
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         SqlConnectionFactoryRagKB.cs
// Author: Kyle L. Crowder
// Build Num: 175058



using Microsoft.Data.SqlClient;




namespace DataIngestionLib.Services;





//Provides SQL connection for RAG knowledge base.
public static class SqlConnectionFactoryRagKB
{
    public static SqlConnection CreateConnection()
    {
        var connectionString = Environment.GetEnvironmentVariable("CONN_STRING2") ?? throw new InvalidOperationException("Environment variable 'CONN_STRING2' is not set.");
        return new SqlConnection(connectionString);
    }
}