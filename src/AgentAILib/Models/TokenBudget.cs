// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         TokenBudget.cs
// Author: Kyle L. Crowder
// Build Num: 194456



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