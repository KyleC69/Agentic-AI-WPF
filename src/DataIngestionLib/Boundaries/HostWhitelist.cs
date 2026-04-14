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

namespace AgentAILib.Boundaries;

public class HostWhitelist
{

    //Allowed paths within agentic application environment.
    public static readonly string[] AllowedRoots =
    {
            @"F:\_dev_drv_root_\Repos\RAGDataIngestionWPF\src",
            @"F:\_dev_drv_root_\Repos\RAGDataIngestionWPF\docs",
            @"F:\_dev_drv_root_\Repos\RAGDataIngestionWPF\tests",
            @"F:\_dev_drv_root_\Repos\RAGDataIngestionWPF\sql",
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Temp"
    };


}
