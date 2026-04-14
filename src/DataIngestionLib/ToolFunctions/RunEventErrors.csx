// Runnable C# script for Windows event diagnostics.
// Default behavior: scan top 25 logs, query Critical/Error from last 6 hours,
// and print top 25 newest matching events.
//
// Example:
// dotnet script src/AgentAILib/ToolFunctions/RunEventErrors.csx\

#r "nuget: Microsoft.Extensions.Logging.Console, 10.0.0"
#r "nuget: Microsoft.Extensions.Logging, 10.0.0"
#r "nuget: System.Net.Http, 10.0.4"
#r "E:\Released\AgentAILib.dll"


using System.Net.Http;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using AgentAILib.ToolFunctions.General;


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

var tool = new WebSearchPlugin(new HttpClient(), logger);
var results = tool.WebSearch("Agent Framework", 10);

