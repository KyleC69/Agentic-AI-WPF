// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         AgentDescriptor.cs
// Author: Kyle L. Crowder
// Build Num: 212848



using DataIngestionLib.Models;

using Microsoft.Extensions.AI;




namespace DataIngestionLib.Agents;





internal class AgentDescriptor
{
    public string Description { get; set; }

    public string ID { get; set; } = string.Empty;
    public string Instructions { get; set; }

    public AIModels Model { get; set; }
    public string Name { get; set; }

    public List<AITool> Tools { get; set; } = new List<AITool>();
}