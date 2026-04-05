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



namespace DataIngestionLib;





// Settings for the DataIngestionLib.
public class AppSettings
{

    public Guid ApplicationId { get; set; } = Guid.Parse("15A53D0F-041D-44DD-A150-DFB8D0F133FF");
    public string RestEndpoint { get; } = "http://127.0.0.1:11434";


}