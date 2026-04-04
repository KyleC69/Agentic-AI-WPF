// Build Date: 2026/04/03
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ToolBuilder.cs
// Author: Kyle L. Crowder
// Build Num: 095204



using Microsoft.Extensions.AI;




namespace DataIngestionLib.ToolFunctions;





public sealed class ToolBuilder
{
    private readonly WebSearchPlugin _webSearchPlugin;
    private readonly SandboxEventLogReader _eventLogReader;
    private readonly InstalledUpdatesTool _installedUpdatesTool;
    private readonly NetworkConfigurationTool _networkConfigurationTool;
    private readonly PerformanceCounterTool _performanceCounterTool;
    private readonly ProcessSnapshotTool _processSnapshotTool;
    private readonly RegistryReaderTool _registryReaderTool;
    private readonly ReliabilityHistoryTool _reliabilityHistoryTool;
    private readonly SafeCommandRunner _safeCommandRunner;
    private readonly ServiceHealthTool _serviceHealthTool;
    private readonly StartupInventoryTool _startupInventoryTool;
    private readonly StorageHealthTool _storageHealthTool;
    private readonly WindowsEventChannelReaderTool _windowsEventChannelReaderTool;
    private readonly WindowsWmiReaderTool _windowsWmiReaderTool;






    public ToolBuilder(WebSearchPlugin webSearchPlugin, SandboxEventLogReader eventLogReader, InstalledUpdatesTool installedUpdatesTool, NetworkConfigurationTool networkConfigurationTool, PerformanceCounterTool performanceCounterTool, ProcessSnapshotTool processSnapshotTool, RegistryReaderTool registryReaderTool, ReliabilityHistoryTool reliabilityHistoryTool, SafeCommandRunner safeCommandRunner, ServiceHealthTool serviceHealthTool, StartupInventoryTool startupInventoryTool, StorageHealthTool storageHealthTool, WindowsEventChannelReaderTool windowsEventChannelReaderTool, WindowsWmiReaderTool windowsWmiReaderTool)
    {
        _webSearchPlugin = webSearchPlugin;
        _eventLogReader = eventLogReader;
        _installedUpdatesTool = installedUpdatesTool;
        _networkConfigurationTool = networkConfigurationTool;
        _performanceCounterTool = performanceCounterTool;
        _processSnapshotTool = processSnapshotTool;
        _registryReaderTool = registryReaderTool;
        _reliabilityHistoryTool = reliabilityHistoryTool;
        _safeCommandRunner = safeCommandRunner;
        _serviceHealthTool = serviceHealthTool;
        _startupInventoryTool = startupInventoryTool;
        _storageHealthTool = storageHealthTool;
        _windowsEventChannelReaderTool = windowsEventChannelReaderTool;
        _windowsWmiReaderTool = windowsWmiReaderTool;
    }

    public IList<AITool> GetAiTools()
    {
        return this.GetReadOnlyAiTools();
    }








    internal IList<AITool> GetReadOnlyAiTools()
    {
        return
        [
                AIFunctionFactory.Create(_webSearchPlugin.WebSearch),
                AIFunctionFactory.Create(SystemInfoTool.GetInfo),
                AIFunctionFactory.Create(_eventLogReader.ReadLog),
                AIFunctionFactory.Create(_installedUpdatesTool.ReadInstalledUpdates),
                AIFunctionFactory.Create(_networkConfigurationTool.ReadActiveAdapters),
                AIFunctionFactory.Create(_performanceCounterTool.ReadSnapshot),
                AIFunctionFactory.Create(_processSnapshotTool.ReadTopProcesses),
                AIFunctionFactory.Create(_windowsEventChannelReaderTool.ReadChannel),
                AIFunctionFactory.Create(_registryReaderTool.ReadValue),
                AIFunctionFactory.Create(_reliabilityHistoryTool.ReadRecent),
                AIFunctionFactory.Create(_serviceHealthTool.ReadServices),
                AIFunctionFactory.Create(_startupInventoryTool.ReadStartupItems),
                AIFunctionFactory.Create(_storageHealthTool.ReadLogicalDisks),
                AIFunctionFactory.Create(_windowsWmiReaderTool.ReadClass),
                AIFunctionFactory.Create(_safeCommandRunner.Run),
                AIFunctionFactory.Create(ListFolderContentsTool.ListFolderContents)

        ];
    }








    internal IList<AITool> GetWritingAiTools()
    {
        return
        [
                AIFunctionFactory.Create(FileSystemWriterTool.WriteText)
        ];
    }
}