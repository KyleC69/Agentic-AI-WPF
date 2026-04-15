// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         IRagDataService.cs
// Author: Kyle L. Crowder
// Build Num: 194446



using Microsoft.Extensions.AI;




namespace AgentAILib.Contracts;





public interface IRagDataService
{
    Task<IEnumerable<ChatMessage>> GetRagDataEntries(string query, CancellationToken cancellationToken = default);
}