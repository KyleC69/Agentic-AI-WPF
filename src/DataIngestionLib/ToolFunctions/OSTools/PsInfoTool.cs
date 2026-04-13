using System.ComponentModel;

using DataIngestionLib.ToolFunctions;




namespace DataIngestionLib.ToolFunctions.OSTools;





/// <summary>
/// Displays detailed system information.
/// </summary>
[Description("Shows comprehensive system information including OS version, hardware, uptime, and installed software.")]
public class PsInfoTool(CommandExecutor executor)
{
    private const string Command = "psinfo.exe";

    [Description("Shows basic system information.")]
    public async Task<CommandResult> GetSystemInfo()
        => await executor.ExecuteAsync(Command);

    [Description("Shows system information including installed applications.")]
    public async Task<CommandResult> GetSystemInfoWithApps()
        => await executor.ExecuteAsync(Command, "-s");

    [Description("Shows detailed information for a specific system component.")]
    public async Task<CommandResult> GetDetailedInfo(
            [Description("Component: 'volume', 'services', or 'all'")] string component)
        => await executor.ExecuteAsync(Command, $"-d {component}");
}