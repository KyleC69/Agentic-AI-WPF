// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         sp_LearnDocs_Search_VectorResult.cs
// Author: Kyle L. Crowder
// Build Num: 194453



namespace AgentAILib.EFModels;





public class sp_LearnDocs_Search_VectorResult
{

    public sp_LearnDocs_Search_VectorResult(string failureInfo)
    {
        FailureInfo = failureInfo;
    }








    public string? Content { get; set; } = string.Empty;
    public double? Distance { get; set; }
    public string? FailureInfo { get; set; } = string.Empty;
    public int? Id { get; set; }
}