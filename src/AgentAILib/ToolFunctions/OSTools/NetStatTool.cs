// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         NetStatTool.cs
// Author: Kyle L. Crowder
// Build Num: 194512



using System.ComponentModel;




namespace AgentAILib.ToolFunctions.OSTools;





/// <summary>
///     Shows network connection information.
/// </summary>
[Description("Displays TCP/IP network connection information with process ownership.")]
public class NetStatTool(CommandExecutor executor)
{
    private const string Command = "netstat.exe";








    [Description("Shows the routing table.")]
    public async Task<CommandResult> GetRoutingTable()
    {
        return await executor.ExecuteAsync(Command, "-r");
    }








    [Description("Shows statistics for each protocol.")]
    public async Task<CommandResult> GetStatistics()
    {
        return await executor.ExecuteAsync(Command, "-s");
    }








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
}