// Build Date: 2026/03/11
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ToolBuilder.cs
// Author: Kyle L. Crowder
// Build Num: 105649



using System.Net.Http;

using Microsoft.Extensions.AI;




namespace DataIngestionLib.ToolFunctions;





internal class ToolBuilder
{


    public static IList<AITool> GetAiTools(IHttpClientFactory httpClientFactory)
    {
        WebSearchPlugin webSearchPlugin = new(httpClientFactory);
        AgentLogger logger = new(Environment.CurrentDirectory);

        SystemInfoTool systemInfoTool = new();
        SandboxEventLogReader eventLogReader = new();
        SafeCommandRunner safeCommandRunner = new(Environment.CurrentDirectory);




        IList<AITool> tools =
        [

                AIFunctionFactory.Create(logger.LogMessage),
                AIFunctionFactory.Create(FileSystemWriterTool.WriteText),
                AIFunctionFactory.Create(FileSystemReaderTool.ReadFile),
                AIFunctionFactory.Create(FullTextRagSearchTool.Search),
                AIFunctionFactory.Create(webSearchPlugin.WebSearch),
                AIFunctionFactory.Create(systemInfoTool.GetInfo),
                AIFunctionFactory.Create(eventLogReader.ReadLog),
                AIFunctionFactory.Create(safeCommandRunner.Run)

        ];


        return tools;
    }
}