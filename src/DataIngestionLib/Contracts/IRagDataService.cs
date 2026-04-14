// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   AgentAILib
// File:         IRagDataService.cs
// Author: Kyle L. Crowder
// Build Num: 212854



using Microsoft.Extensions.AI;




namespace AgentAILib.Contracts;





public interface IRagDataService
{
    Task<IEnumerable<ChatMessage>> GetRagDataEntries(string query, CancellationToken cancellationToken = default);
}