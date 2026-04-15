// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         IAIToolCatalog.cs
// Author: Kyle L. Crowder
// Build Num: 194445



using Microsoft.Extensions.AI;




namespace AgentAILib.Contracts;





public interface IAIToolCatalog
{
    IList<AITool> GetAiTools();


    IList<AITool> GetReadOnlyAiTools();


    IList<AITool> GetWritingAiTools();
}