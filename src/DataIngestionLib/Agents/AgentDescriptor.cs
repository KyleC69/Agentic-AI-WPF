// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//
//
//
//



using Microsoft.Extensions.AI;




namespace DataIngestionLib.Agents;





public class AgentDescriptor
{

    public AgentDescriptor()
    {
        ID = Guid.NewGuid().ToString("D");
    }


    public string ID { get; set; } = string.Empty;

    public string AgentType { get; set; } = "Custom";

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public bool Enabled { get; set; } = true;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Instructions { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public DateTime ModifiedUtc { get; set; } = DateTime.UtcNow;

    public int TokenBudget { get; set; } = 120000;

    public string? ToolPolicyKey { get; set; }

    public List<AITool> Tools { get; set; } = new List<AITool>();
}