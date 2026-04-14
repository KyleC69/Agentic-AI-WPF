// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   AgentAILib
// File:         IAIToolCatalog.cs
// Author: Kyle L. Crowder
// Build Num: 212854



using Microsoft.Extensions.AI;




namespace AgentAILib.Contracts;





public interface IAIToolCatalog
{
    IList<AITool> GetAiTools();


    IList<AITool> GetReadOnlyAiTools();


    IList<AITool> GetWritingAiTools();
}