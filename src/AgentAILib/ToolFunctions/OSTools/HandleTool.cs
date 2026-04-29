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
///     Lists open handles for processes.
/// </summary>
[Description("Lists open file handles, mutexes, and other kernel objects held by processes.")]
public class HandleTool(CommandExecutor executor)
{
    private const string Command = "handle.exe -nobanner";








    [Description("Lists all open handles on the system. VERY VERBOSE")]
    public async Task<CommandResult> ListAll()
    {
        return await executor.ExecuteAsync(Command, "-a");
    }








    [Description("Lists handles for a specific process.")]
    public async Task<CommandResult> ListForProcess([Description("Process name or PID")] string process)
    {
        return await executor.ExecuteAsync(Command, $"-p {process}");
    }








    [Description("Searches for handles to a specific file or pattern.")]
    public async Task<CommandResult> SearchHandles([Description("File name or pattern to search for")] string pattern)
    {
        return await executor.ExecuteAsync(Command, pattern);
    }
}