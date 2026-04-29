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



using System.ComponentModel;




namespace AgentAILib.ToolFunctions.OSTools;





/// <summary>
///     Lists running processes with detailed information.
/// </summary>
[Description("Lists all running processes with memory, CPU, and thread counts. Equivalent to Task Manager process list.")]
public class PsListTool(CommandExecutor executor)
{
    private const string Command = "pslist.exe -nobanner";








    [Description("Lists all running processes with default columns.")]
    public async Task<CommandResult> ListAll()
    {
        return await executor.ExecuteAsync(Command);
    }








    [Description("Lists processes memory usage details.")]
    public async Task<CommandResult> ListByMemory()
    {
        return await executor.ExecuteAsync(Command, "-m");
    }








    [Description("Shows the thread details for the given processName")]
    public async Task<CommandResult> ListByName([Description("Process name to query (e.g., 'chrome', 'svchost')")] string processName)
    {
        return await executor.ExecuteAsync(Command, $"-d {processName}");
    }








    [Description("Shows process tree information for a process.")]
    public async Task<CommandResult> ListProcessTree([Description("Process name or PID")] string process)
    {
        return await executor.ExecuteAsync(Command, $"-t {process}");
    }








    [Description("Shows processes, memory and thread information.")]
    public async Task<CommandResult> ListThreads([Description("Process name or PID")] string process)
    {
        return await executor.ExecuteAsync(Command, $"-x {process}");
    }
}