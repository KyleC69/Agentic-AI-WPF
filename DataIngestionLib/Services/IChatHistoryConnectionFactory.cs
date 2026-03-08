// 2026/03/07
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         IChatHistoryConnectionFactory.cs
//   Author: Kyle L. Crowder



using Microsoft.Data.SqlClient;




namespace DataIngestionLib.Services;





public interface IChatHistoryConnectionFactory
{
    ValueTask<SqlConnection> OpenConnectionAsync(CancellationToken cancellationToken);
}