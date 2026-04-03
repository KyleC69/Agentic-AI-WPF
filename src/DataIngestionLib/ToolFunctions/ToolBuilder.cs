// Build Date: 2026/04/03
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ToolBuilder.cs
// Author: Kyle L. Crowder
// Build Num: 095204



using System.Net.Http;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;




namespace DataIngestionLib.ToolFunctions;





internal sealed class ToolBuilder
{

    public static IList<AITool> GetAiTools()
    {
        return GetReadOnlyAiTools();
    }








    internal static IList<AITool> GetReadOnlyAiTools()
    {
        WebSearchPlugin webSearchPlugin = new(new HttpClient());
        SandboxEventLogReader eventLogReader = new();
        InstalledUpdatesTool installedUpdatesTool = new();
        NetworkConfigurationTool networkConfigurationTool = new();
        PerformanceCounterTool performanceCounterTool = new();
        ProcessSnapshotTool processSnapshotTool = new();
        RegistryReaderTool registryReaderTool = new(NullLoggerFactory.Instance);
        ReliabilityHistoryTool reliabilityHistoryTool = new();
        SafeCommandRunner safeCommandRunner = new(Environment.CurrentDirectory);
        ServiceHealthTool serviceHealthTool = new();
        StartupInventoryTool startupInventoryTool = new();
        StorageHealthTool storageHealthTool = new();
        WindowsEventChannelReaderTool windowsEventChannelReaderTool = new();
        WindowsWmiReaderTool windowsWmiReaderTool = new();




        return
        [
                AIFunctionFactory.Create(webSearchPlugin.WebSearch),
                AIFunctionFactory.Create(SystemInfoTool.GetInfo),
                AIFunctionFactory.Create(eventLogReader.ReadLog),
                AIFunctionFactory.Create(installedUpdatesTool.ReadInstalledUpdates),
                AIFunctionFactory.Create(networkConfigurationTool.ReadActiveAdapters),
                AIFunctionFactory.Create(performanceCounterTool.ReadSnapshot),
                AIFunctionFactory.Create(processSnapshotTool.ReadTopProcesses),
                AIFunctionFactory.Create(windowsEventChannelReaderTool.ReadChannel),
                AIFunctionFactory.Create(registryReaderTool.ReadValue),
                AIFunctionFactory.Create(reliabilityHistoryTool.ReadRecent),
                AIFunctionFactory.Create(serviceHealthTool.ReadServices),
                AIFunctionFactory.Create(startupInventoryTool.ReadStartupItems),
                AIFunctionFactory.Create(storageHealthTool.ReadLogicalDisks),
                AIFunctionFactory.Create(windowsWmiReaderTool.ReadClass),
                AIFunctionFactory.Create(safeCommandRunner.Run),
                AIFunctionFactory.Create(ListFolderContentsTool.ListFolderContents)

        ];
    }








    internal static IList<AITool> GetWritingAiTools()
    {
        return
        [
                AIFunctionFactory.Create(FileSystemWriterTool.WriteText)
        ];
    }
}