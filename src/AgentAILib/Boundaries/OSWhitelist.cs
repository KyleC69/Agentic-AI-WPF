// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         OSWhitelist.cs
// Author: Kyle L. Crowder
// Build Num: 194445



namespace AgentAILib.Boundaries;





public class OSWhitelist
{

    public static string[] AllowedPaths = { @"%windir%\", @"%temp%\", @"%appdata%\", @"%localappdata%\" };
}