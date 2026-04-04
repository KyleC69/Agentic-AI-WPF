// Build Date: 2026/04/04
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IRagDataService.cs
// Author: GitHub Copilot


using Microsoft.Extensions.AI;


namespace DataIngestionLib.Contracts;



public interface IRagDataService
{
    Task<IEnumerable<ChatMessage>> GetRagDataEntries(string query, CancellationToken cancellationToken = default);
}
