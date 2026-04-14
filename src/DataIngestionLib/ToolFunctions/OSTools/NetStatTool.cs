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
/// Shows network connection information.
/// </summary>
[Description("Displays TCP/IP network connection information with process ownership.")]
public class NetStatTool(CommandExecutor executor)
{
    private const string Command = "netstat.exe";

    [Description("Shows all active TCP connections.")]
    public async Task<CommandResult> ListTcpConnections()
    {
        return await executor.ExecuteAsync(Command, "-an");
    }

    [Description("Shows connections with owning process IDs.")]
    public async Task<CommandResult> ListWithProcessIds()
    {
        return await executor.ExecuteAsync(Command, "-ano");
    }

    [Description("Shows statistics for each protocol.")]
    public async Task<CommandResult> GetStatistics()
    {
        return await executor.ExecuteAsync(Command, "-s");
    }

    [Description("Shows the routing table.")]
    public async Task<CommandResult> GetRoutingTable()
    {
        return await executor.ExecuteAsync(Command, "-r");
    }
}