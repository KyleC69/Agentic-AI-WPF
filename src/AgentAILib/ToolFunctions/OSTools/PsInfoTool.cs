// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         PsInfoTool.cs
// Author: Kyle L. Crowder
// Build Num: 194516



using System.ComponentModel;




namespace AgentAILib.ToolFunctions.OSTools;





/// <summary>
///     Displays detailed system information.
/// </summary>
[Description("Shows comprehensive system information including OS version, hardware, uptime, and installed software.")]
public class PsInfoTool(CommandExecutor executor)
{
    private const string Command = "psinfo.exe";








    [Description("Shows detailed information for a specific system component.")]
    public async Task<CommandResult> GetDetailedInfo([Description("Component: 'volume', 'services', or 'all'")] string component) => await executor.ExecuteAsync(Command, $"-d {component}");








    [Description("Shows basic system information.")]
    public async Task<CommandResult> GetSystemInfo() => await executor.ExecuteAsync(Command);








    [Description("Shows system information including installed applications.")]
    public async Task<CommandResult> GetSystemInfoWithApps() => await executor.ExecuteAsync(Command, "-s");
}