// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         WorkspaceWhitelist.cs
// Author: Kyle L. Crowder
// Build Num: 194445



namespace AgentAILib.Boundaries;





internal class WorkspaceWhitelist
{
    public static readonly string[] AllowedRoots = { @"F:\_dev_drv_root_\Repos\RAGDataIngestionWPF", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Temp" };
}