// Runnable C# script for Agentic tool function tests
//
// Example:
// dotnet script src/AgentAILib/ToolFunctions/QuickScript.csx\

#r "nuget: Microsoft.Extensions.Logging.Console, 10.0.0"
#r "nuget: Microsoft.Extensions.Logging, 10.0.0"
#r "E:\Released\AgentAILib.dll"
#r "nuget: System.Diagnostics.EventLog"


using System.Net.Http;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using AgentAILib.ToolFunctions.General;
using AgentAILib.ToolFunctions.OSTools;



var logger = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Trace);
}).CreateLogger("ToolFunctions.RunEventErrors");


if (!OperatingSystem.IsWindows())
{
    Console.WriteLine("This script only runs on Windows.");
    return;
}

var tool = new WindowsEventChannelReaderTool();
var results = tool.ReadEventLogChannel("Application");

logger.LogInformation(results.FailureReason);