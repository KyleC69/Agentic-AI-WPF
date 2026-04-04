// Build Date: 2026/04/03
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         TokenBudget.cs
// Author: Kyle L. Crowder
// Build Num: 095155

namespace DataIngestionLib.Models;





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