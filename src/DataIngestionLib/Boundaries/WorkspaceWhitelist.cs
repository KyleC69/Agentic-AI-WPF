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

using System;
using System.Collections.Generic;
using System.Text;

namespace DataIngestionLib.Boundaries;

internal class WorkspaceWhitelist
{
    public static readonly string[] AllowedRoots =
    {
        @"F:\_dev_drv_root_\Repos\RAGDataIngestionWPF",
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Temp"
    };
}