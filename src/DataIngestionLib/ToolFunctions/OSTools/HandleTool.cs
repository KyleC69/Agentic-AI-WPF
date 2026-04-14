using System.ComponentModel;





namespace AgentAILib.ToolFunctions.OSTools;





/// <summary>
/// Lists open handles for processes.
/// </summary>
[Description("Lists open file handles, mutexes, and other kernel objects held by processes.")]
public class HandleTool(CommandExecutor executor)
{
    private const string Command = "handle.exe";

    [Description("Lists all open handles on the system.")]
    public async Task<CommandResult> ListAll()
        => await executor.ExecuteAsync(Command, "-a");

    [Description("Lists handles for a specific process.")]
    public async Task<CommandResult> ListForProcess([Description("Process name or PID")] string process)
        => await executor.ExecuteAsync(Command, $"-p {process}");

    [Description("Searches for handles to a specific file or pattern.")]
    public async Task<CommandResult> SearchHandles([Description("File name or pattern to search for")] string pattern)
        => await executor.ExecuteAsync(Command, pattern);
}