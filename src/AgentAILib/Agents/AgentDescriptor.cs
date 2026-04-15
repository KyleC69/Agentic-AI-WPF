// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         AgentDescriptor.cs
// Author: Kyle L. Crowder
// Build Num: 194438



using Microsoft.Extensions.AI;




namespace AgentAILib.Agents;





public class AgentDescriptor
{

    public AgentDescriptor()
    {
        ID = Guid.NewGuid().ToString("D");
    }








    public string AgentType { get; set; } = "Custom";

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public string Description { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;

    public string ID { get; set; } = string.Empty;

    public string Instructions { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public DateTime ModifiedUtc { get; set; } = DateTime.UtcNow;

    public string Name { get; set; } = string.Empty;

    public int TokenBudget { get; set; } = 120000;

    public string? ToolPolicyKey { get; set; }

    public List<AITool> Tools { get; set; } = new List<AITool>();
}