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

public class OSWhitelist
{

    public static string[] AllowedPaths =
    {
            @"%windir%\", @"%temp%\", @"%appdata%\", @"%localappdata%\"

    };
}
