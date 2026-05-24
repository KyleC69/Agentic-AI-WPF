// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         OSWhitelist.cs
// Author: Kyle L. Crowder
// Build Date: 2026/05/24



namespace AgentAILib.Boundaries;





public class OSWhitelist
{

    public static string[] AllowedPaths = { @"%windir%\", @"%temp%\", @"%appdata%\", @"%localappdata%\" };
}