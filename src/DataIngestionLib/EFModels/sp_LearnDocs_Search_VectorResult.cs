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

namespace DataIngestionLib.EFModels;

public partial class sp_LearnDocs_Search_VectorResult
{
    public int Id { get; set; }
    public string? Content { get; set; } = string.Empty;
    public string? FailureInfo { get; set; } = string.Empty;
    public double Distance { get; set; }

}
