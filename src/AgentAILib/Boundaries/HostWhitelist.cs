// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         HostWhitelist.cs
// Author: Kyle L. Crowder
// Build Num: 194445



namespace AgentAILib.Boundaries;





public class HostWhitelist
{

    //Allowed paths within agentic application environment.
    public static readonly string[] AllowedRoots = { @"F:\_dev_drv_root_\Repos\RAGDataIngestionWPF\src", @"F:\_dev_drv_root_\Repos\RAGDataIngestionWPF\docs", @"F:\_dev_drv_root_\Repos\RAGDataIngestionWPF\tests", @"F:\_dev_drv_root_\Repos\RAGDataIngestionWPF\sql", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Temp" };
}