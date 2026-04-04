// Build Date: 2026/04/03
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ToolBuilder.cs
// Author: Kyle L. Crowder
// Build Num: 095204



using DataIngestionLib.Contracts;

using Microsoft.Extensions.AI;




namespace DataIngestionLib.ToolFunctions;





public sealed class ToolBuilder : IAIToolCatalog
{
    private readonly Lazy<AITool> _webSearchTool;
    private readonly Lazy<AITool> _systemInfoTool;
    private readonly Lazy<AITool> _eventLogTool;
    private readonly Lazy<AITool> _installedUpdatesReadTool;
    private readonly Lazy<AITool> _networkConfigurationReadTool;
    private readonly Lazy<AITool> _performanceCounterReadTool;
    private readonly Lazy<AITool> _processSnapshotReadTool;
    private readonly Lazy<AITool> _windowsEventChannelReadTool;
    private readonly Lazy<AITool> _registryReadTool;
    private readonly Lazy<AITool> _reliabilityHistoryReadTool;
    private readonly Lazy<AITool> _serviceHealthReadTool;
    private readonly Lazy<AITool> _startupInventoryReadTool;
    private readonly Lazy<AITool> _storageHealthReadTool;
    private readonly Lazy<AITool> _windowsWmiReadTool;
    private readonly Lazy<AITool> _safeCommandRunTool;
    private readonly Lazy<AITool> _listFolderContentsTool;
    private readonly Lazy<AITool> _fileSystemWriterTool;






    public ToolBuilder(WebSearchPlugin webSearchPlugin, SandboxEventLogReader eventLogReader, InstalledUpdatesTool installedUpdatesTool, NetworkConfigurationTool networkConfigurationTool, PerformanceCounterTool performanceCounterTool, ProcessSnapshotTool processSnapshotTool, RegistryReaderTool registryReaderTool, ReliabilityHistoryTool reliabilityHistoryTool, SafeCommandRunner safeCommandRunner, ServiceHealthTool serviceHealthTool, StartupInventoryTool startupInventoryTool, StorageHealthTool storageHealthTool, WindowsEventChannelReaderTool windowsEventChannelReaderTool, WindowsWmiReaderTool windowsWmiReaderTool)
    {
        _webSearchTool = new(() => AIFunctionFactory.Create(webSearchPlugin.WebSearch));
        _systemInfoTool = new(() => AIFunctionFactory.Create(SystemInfoTool.GetInfo));
        _eventLogTool = new(() => AIFunctionFactory.Create(eventLogReader.ReadLog));
        _installedUpdatesReadTool = new(() => AIFunctionFactory.Create(installedUpdatesTool.ReadInstalledUpdates));
        _networkConfigurationReadTool = new(() => AIFunctionFactory.Create(networkConfigurationTool.ReadActiveAdapters));
        _performanceCounterReadTool = new(() => AIFunctionFactory.Create(performanceCounterTool.ReadSnapshot));
        _processSnapshotReadTool = new(() => AIFunctionFactory.Create(processSnapshotTool.ReadTopProcesses));
        _windowsEventChannelReadTool = new(() => AIFunctionFactory.Create(windowsEventChannelReaderTool.ReadChannel));
        _registryReadTool = new(() => AIFunctionFactory.Create(registryReaderTool.ReadValue));
        _reliabilityHistoryReadTool = new(() => AIFunctionFactory.Create(reliabilityHistoryTool.ReadRecent));
        _serviceHealthReadTool = new(() => AIFunctionFactory.Create(serviceHealthTool.ReadServices));
        _startupInventoryReadTool = new(() => AIFunctionFactory.Create(startupInventoryTool.ReadStartupItems));
        _storageHealthReadTool = new(() => AIFunctionFactory.Create(storageHealthTool.ReadLogicalDisks));
        _windowsWmiReadTool = new(() => AIFunctionFactory.Create(windowsWmiReaderTool.ReadClass));
        _safeCommandRunTool = new(() => AIFunctionFactory.Create(safeCommandRunner.Run));
        _listFolderContentsTool = new(() => AIFunctionFactory.Create(ListFolderContentsTool.ListFolderContents));
        _fileSystemWriterTool = new(() => AIFunctionFactory.Create(FileSystemWriterTool.WriteText));
    }

    public IList<AITool> GetAiTools()
    {
        return this.GetReadOnlyAiTools();
    }








    public IList<AITool> GetReadOnlyAiTools()
    {
        return
        [
                _webSearchTool.Value,
                _systemInfoTool.Value,
                _eventLogTool.Value,
                _installedUpdatesReadTool.Value,
                _networkConfigurationReadTool.Value,
                _performanceCounterReadTool.Value,
                _processSnapshotReadTool.Value,
                _windowsEventChannelReadTool.Value,
                _registryReadTool.Value,
                _reliabilityHistoryReadTool.Value,
                _serviceHealthReadTool.Value,
                _startupInventoryReadTool.Value,
                _storageHealthReadTool.Value,
                _windowsWmiReadTool.Value,
                _safeCommandRunTool.Value,
                _listFolderContentsTool.Value

        ];
    }








    public IList<AITool> GetWritingAiTools()
    {
        return
        [
                _fileSystemWriterTool.Value
        ];
    }
}