// Build Date: 2026/04/04
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IAIToolCatalog.cs
// Author: GitHub Copilot


using Microsoft.Extensions.AI;


namespace DataIngestionLib.Contracts;



public interface IAIToolCatalog
{
    IList<AITool> GetAiTools();





    IList<AITool> GetReadOnlyAiTools();





    IList<AITool> GetWritingAiTools();
}
