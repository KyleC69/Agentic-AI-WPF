// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IRagDataService.cs
// Author: Kyle L. Crowder
// Build Num: 212854



using Microsoft.Extensions.AI;




namespace DataIngestionLib.Contracts;





public interface IRagDataService
{
    Task<IEnumerable<ChatMessage>> GetRagDataEntries(string query, CancellationToken cancellationToken = default);
}