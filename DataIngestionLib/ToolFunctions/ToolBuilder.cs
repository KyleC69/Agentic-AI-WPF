// 2026/03/08
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         ToolBuilder.cs
//   Author: Kyle L. Crowder



using Microsoft.Extensions.AI;
using System.Net.Http;




namespace DataIngestionLib.ToolFunctions;





internal class ToolBuilder
{


    public static IList<AITool> GetAiTools(IHttpClientFactory httpClientFactory)
    {
        FileSystemPlugin fileSystemPlugin = new();
        WebSearchPlugin webSearchPlugin = new(httpClientFactory);
        AgentLogger logger = new(Environment.CurrentDirectory);

        SandboxFileReader fileReader = new(Environment.CurrentDirectory);
        SandboxFileWriter fileWriter = new(Environment.CurrentDirectory);
        SystemInfoTool systemInfoTool = new();
        IList<AITool> tools =
        [

                AIFunctionFactory.Create(logger.Log),
                AIFunctionFactory.Create(fileSystemPlugin.WriteText),
                //  AIFunctionFactory.Create(ragSearchTool.Search),
                AIFunctionFactory.Create(fileReader.ReadFile),
                AIFunctionFactory.Create(webSearchPlugin.WebSearch),
                AIFunctionFactory.Create(fileWriter.WriteFile),
                AIFunctionFactory.Create(systemInfoTool.GetInfo)

        ];


        return tools;
    }
}