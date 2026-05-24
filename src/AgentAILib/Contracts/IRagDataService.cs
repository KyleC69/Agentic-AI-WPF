// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         IRagDataService.cs
// Author: Kyle L. Crowder
// Build Date: 2026/05/24



using Microsoft.Extensions.AI;




namespace AgentAILib.Contracts;





public interface IRagDataService
{
    Task<IEnumerable<ChatMessage>> GetRagDataEntries(string query, CancellationToken cancellationToken = default);
}