// 2026/03/10
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         ToolBuilder.cs
//   Author: Kyle L. Crowder



using System.Diagnostics.Eventing.Reader;
using System.Net.Http;

using Microsoft.Extensions.AI;




namespace DataIngestionLib.ToolFunctions;





internal class ToolBuilder
{


    public static IList<AITool> GetAiTools(IHttpClientFactory httpClientFactory)
    {
        FileSystemWriterTool fileSystemPlugin = new();
        WebSearchPlugin webSearchPlugin = new(httpClientFactory);
        AgentLogger logger = new(Environment.CurrentDirectory);

        SandboxFileReader fileReader = new(Environment.CurrentDirectory);
        SandboxFileWriter fileWriter = new(Environment.CurrentDirectory);
        SystemInfoTool systemInfoTool = new();
        SandboxEventLogReader eventLogReader = new();
        SafeCommandRunner safeCommandRunner = new(Environment.CurrentDirectory);




        IList<AITool> tools =
          [

                  AIFunctionFactory.Create(logger.LogMessage),
                AIFunctionFactory.Create(fileSystemPlugin.WriteText),
                //  AIFunctionFactory.Create(ragSearchTool.Search),
                AIFunctionFactory.Create(fileReader.ReadFile),
                AIFunctionFactory.Create(webSearchPlugin.WebSearch),
                AIFunctionFactory.Create(fileWriter.WriteFile),
                AIFunctionFactory.Create(systemInfoTool.GetInfo),   
                  AIFunctionFactory.Create(eventLogReader.ReadLog),
                  AIFunctionFactory.Create(safeCommandRunner.Run),
                  


          ];


        return tools;
    }
}