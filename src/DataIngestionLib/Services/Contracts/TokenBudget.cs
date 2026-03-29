// Build Date: 2026/03/29
// Solution: File
// Project:   DataIngestionLib
// File:         TokenBudget.cs
// Author: Kyle L. Crowder
// Build Num: 051938



namespace DataIngestionLib.Services.Contracts;





public class TokenBudget
{
    public int BudgetTotal { get; set; }
    public int MaximumContext { get; set; }
    public int MetaBudget { get; set; }
    public int RAGBudget { get; set; }
    public int SessionBudget { get; set; }
    public int SystemBudget { get; set; }
    public int ToolBudget { get; set; }
}