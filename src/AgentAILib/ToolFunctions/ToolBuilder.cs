// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         ToolBuilder.cs
// Author: Kyle L. Crowder
// Build Num: 194523



using AgentAILib.Contracts;
using AgentAILib.ToolFunctions.FileSystemReaders;
using AgentAILib.ToolFunctions.FileSystemWriters;
using AgentAILib.ToolFunctions.General;
using AgentAILib.ToolFunctions.OSTools;

using Microsoft.Extensions.AI;




namespace AgentAILib.ToolFunctions;





public sealed class ToolBuilder : IAIToolCatalog
{
    private readonly AITool _eventErrorsTool;
    private readonly AITool _eventLogTool;
    private readonly AITool _fileContentsReadingTool;
    private readonly AITool _fileSystemWriterTool;
    private readonly HandleTool _handleTool;
    private readonly AITool _installedUpdatesReadTool;
    private readonly AITool _listFolderContentsTool;
    private readonly AITool _logFileListerTool;
    private readonly AITool _logFileReaderTool;
    private readonly NetStatTool _netStatTool;
    private readonly AITool _networkConfigurationReadTool;
    private readonly NsLookupTool _nsLookupTool;
    private readonly AITool _performanceCounterReadTool;
    private readonly AITool _powerShellTool;
    private readonly PsInfoTool _psInfoTool;
    private readonly PsListTool _psListTool;
    private readonly AITool _registryReadTool;
    private readonly AITool _reliabilityHistoryReadTool;
    private readonly AITool _serviceHealthReadTool;
    private readonly AITool _startupInventoryReadTool;
    private readonly AITool _storageHealthReadTool;
    private readonly AITool _webSearchTool;
    private readonly AITool _windowsEventChannelReadTool;
    private readonly AITool _windowsWmiReadTool;








    public ToolBuilder(WebSearchPlugin webSearchPlugin, SandboxEventLogReader eventLogReader, EventErrorsTool eventErrorsTool, PowerShellTool powerShellTool, FileContentsReadingTool fileContentsReadingTool, FileSystemWriterTool fileSystemWriterTool, InstalledUpdatesTool installedUpdatesTool, ListFolderContentsTool listFolderContentsTool, LogFileListingTool logFileLister, LogFileReader logFileReader, NetworkConfigurationTool networkConfigurationTool, PerformanceCounterTool performanceCounterTool, RegistryReaderTool registryReaderTool, ReliabilityHistoryTool reliabilityHistoryTool, ServiceHealthTool serviceHealthTool, StartupInventoryTool startupInventoryTool, StorageHealthTool storageHealthTool, WindowsEventChannelReaderTool windowsEventChannelReaderTool, WindowsWmiReaderTool windowsWmiReaderTool, PsInfoTool psInfoTool, PsListTool psListTool, NsLookupTool nsLookupTool, NetStatTool netStatTool, HandleTool handleTool)
    {
        _psInfoTool = psInfoTool;
        _psListTool = psListTool;
        _nsLookupTool = nsLookupTool;
        _netStatTool = netStatTool;
        _handleTool = handleTool;
        _webSearchTool = AIFunctionFactory.Create(webSearchPlugin.WebSearch);
        _eventErrorsTool = AIFunctionFactory.Create(eventErrorsTool.ReadRecentCriticalAndErrorEvents);
        _powerShellTool = AIFunctionFactory.Create(powerShellTool.RunReadOnly);
        _eventLogTool = AIFunctionFactory.Create(eventLogReader.ReadLog);
        _fileContentsReadingTool = AIFunctionFactory.Create(fileContentsReadingTool.ReadFileContents);
        _fileSystemWriterTool = AIFunctionFactory.Create(fileSystemWriterTool.WriteText);
        _installedUpdatesReadTool = AIFunctionFactory.Create(installedUpdatesTool.ReadInstalledUpdates);
        _listFolderContentsTool = AIFunctionFactory.Create(listFolderContentsTool.ListFolderContents);
        _logFileListerTool = AIFunctionFactory.Create(logFileLister.GetLogFileList);
        _logFileReaderTool = AIFunctionFactory.Create(logFileReader.LogFileReaderTool);
        _networkConfigurationReadTool = AIFunctionFactory.Create(networkConfigurationTool.ReadActiveAdapters);
        _performanceCounterReadTool = AIFunctionFactory.Create(performanceCounterTool.ReadSnapshot);
        _windowsEventChannelReadTool = AIFunctionFactory.Create(windowsEventChannelReaderTool.ReadChannel);
        _registryReadTool = AIFunctionFactory.Create(registryReaderTool.ReadValue);
        _reliabilityHistoryReadTool = AIFunctionFactory.Create(reliabilityHistoryTool.ReadRecent);
        _serviceHealthReadTool = AIFunctionFactory.Create(serviceHealthTool.ReadServices);
        _startupInventoryReadTool = AIFunctionFactory.Create(startupInventoryTool.ReadStartupItems);
        _storageHealthReadTool = AIFunctionFactory.Create(storageHealthTool.ReadLogicalDisks);
        _windowsWmiReadTool = AIFunctionFactory.Create(windowsWmiReaderTool.ReadClass);
    }








    public IList<AITool> GetAiTools()
    {
        return GetReadOnlyAiTools();
    }








    public IList<AITool> GetReadOnlyAiTools()
    {
        return
        [
                _webSearchTool,
                _eventErrorsTool,
                _powerShellTool,
                _eventLogTool,
                _fileContentsReadingTool,
                _installedUpdatesReadTool,
                _listFolderContentsTool,
                _logFileListerTool,
                _logFileReaderTool,
                _networkConfigurationReadTool,
                _performanceCounterReadTool,
                _windowsEventChannelReadTool,
                _registryReadTool,
                _reliabilityHistoryReadTool,
                _serviceHealthReadTool,
                _startupInventoryReadTool,
                _storageHealthReadTool,
                _windowsWmiReadTool,
                AIFunctionFactory.Create(_psInfoTool.GetSystemInfo),
                AIFunctionFactory.Create(_psInfoTool.GetDetailedInfo),
                AIFunctionFactory.Create(_psInfoTool.GetSystemInfoWithApps),
                AIFunctionFactory.Create(_psListTool.ListAll),
                AIFunctionFactory.Create(_psListTool.ListByMemory),
                AIFunctionFactory.Create(_psListTool.ListByName),
                AIFunctionFactory.Create(_psListTool.ListThreads),
                AIFunctionFactory.Create(_nsLookupTool.Resolve),
                AIFunctionFactory.Create(_nsLookupTool.ResolveWithServer),
                AIFunctionFactory.Create(_nsLookupTool.ReverseLookup),
                AIFunctionFactory.Create(_netStatTool.GetStatistics),
                AIFunctionFactory.Create(_netStatTool.GetRoutingTable),
                AIFunctionFactory.Create(_netStatTool.ListTcpConnections),
                AIFunctionFactory.Create(_netStatTool.ListWithProcessIds),
                AIFunctionFactory.Create(_handleTool.ListAll),
                AIFunctionFactory.Create(_handleTool.ListForProcess),
                AIFunctionFactory.Create(_handleTool.SearchHandles)


        ];
    }








    public IList<AITool> GetWritingAiTools()
    {
        return
        [
                _fileSystemWriterTool
        ];
    }
}