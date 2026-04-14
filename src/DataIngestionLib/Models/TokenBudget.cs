// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   AgentAILib
// File:         TokenBudget.cs
// Author: Kyle L. Crowder
// Build Num: 212905



namespace AgentAILib.Models;





public class TokenBudget
{
    public int BudgetTotal { get; set; } = 130000;
    public int MaximumContext { get; set; } = 100000;
    public int MetaBudget { get; set; } = 5000;
    public int RAGBudget { get; set; } = 10000;
    public int SessionBudget { get; set; } = 5000;
    public int SystemBudget { get; set; } = 5000;
    public int ToolBudget { get; set; } = 5000;
}