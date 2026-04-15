// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         PsListTool.cs
// Author: Kyle L. Crowder
// Build Num: 194516



using System.ComponentModel;




namespace AgentAILib.ToolFunctions.OSTools;





/// <summary>
///     Lists running processes with detailed information.
/// </summary>
[Description("Lists all running processes with memory, CPU, and thread counts. Equivalent to Task Manager process list.")]
public class PsListTool(CommandExecutor executor)
{
    private const string Command = "pslist.exe";








    [Description("Lists all running processes with default columns.")]
    public async Task<CommandResult> ListAll()
    {
        return await executor.ExecuteAsync(Command);
    }








    [Description("Lists processes sorted by memory usage.")]
    public async Task<CommandResult> ListByMemory()
    {
        return await executor.ExecuteAsync(Command, "-m");
    }








    [Description("Lists processes matching a name pattern.")]
    public async Task<CommandResult> ListByName([Description("Process name to filter (e.g., 'chrome', 'svchost')")] string processName)
    {
        return await executor.ExecuteAsync(Command, $"-d {processName}");
    }








    [Description("Shows detailed thread information for a process.")]
    public async Task<CommandResult> ListThreads([Description("Process name or PID")] string process)
    {
        return await executor.ExecuteAsync(Command, $"-t {process}");
    }
}