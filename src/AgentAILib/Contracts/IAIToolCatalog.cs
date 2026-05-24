// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         IAIToolCatalog.cs
// Author: Kyle L. Crowder
// Build Date: 2026/05/24



using Microsoft.Extensions.AI;




namespace AgentAILib.Contracts;





public interface IAIToolCatalog
{
    IList<AITool> GetAiTools();


    IList<AITool> GetReadOnlyAiTools();


    IList<AITool> GetWritingAiTools();
}