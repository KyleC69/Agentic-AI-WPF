// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ToolBuilder.cs
// Author: Kyle L. Crowder
// Build Num: 091000



using System.Net.Http;

using Microsoft.Extensions.AI;




namespace DataIngestionLib.ToolFunctions;





internal sealed class ToolBuilder
{

    public static IList<AITool> GetAiTools()
    {
        WebSearchPlugin webSearchPlugin = new(new HttpClient());
        AgentLogger logger = new();

        SandboxEventLogReader eventLogReader = new();
        SafeCommandRunner safeCommandRunner = new(Environment.CurrentDirectory);




        return
        [

                AIFunctionFactory.Create(logger.LogMessage),
                AIFunctionFactory.Create(FileSystemWriterTool.WriteText),
                AIFunctionFactory.Create(FileSystemReaderTool.ReadFile),
                //AIFunctionFactory.Create(FullTextRagSearchTool.Search),
                AIFunctionFactory.Create(webSearchPlugin.WebSearch),
                AIFunctionFactory.Create(SystemInfoTool.GetInfo),
                AIFunctionFactory.Create(eventLogReader.ReadLog),
                AIFunctionFactory.Create(safeCommandRunner.Run)

        ];
    }
}