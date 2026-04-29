// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//
//
//
//



using AgentAILib.Contracts;
using AgentAILib.ToolFunctions.FileSystemReaders;
using AgentAILib.ToolFunctions.FileSystemWriters;
using AgentAILib.ToolFunctions.General;
using AgentAILib.ToolFunctions.OSTools;

using Microsoft.Extensions.AI;





public sealed class ToolBuilder : IAIToolCatalog
{
    private readonly AITool _eventErrorsTool;
    private readonly AITool _eventLogSweepTool;
    private readonly AITool _fileContentsReadingTool;
    private readonly AITool _fileSystemWriterTool;

    private readonly AITool _handleListAll;
    private readonly AITool _handleListForProcess;
    private readonly AITool _handleSearchHandles;
    private readonly AITool _installedUpdatesReadTool;
    private readonly AITool _listFolderContentsTool;
    private readonly AITool _logFileListerTool;
    private readonly AITool _logFileReaderTool;
    private readonly AITool _netStatGetRoutingTable;

    private readonly AITool _netStatGetStatistics;
    private readonly AITool _netStatListTcpConnections;
    private readonly AITool _netStatListWithProcessIds;
    private readonly AITool _networkConfigurationReadTool;

    private readonly AITool _nsLookupResolve;
    private readonly AITool _nsLookupResolveWithServer;
    private readonly AITool _nsLookupReverseLookup;
    private readonly AITool _performanceCounterReadTool;
    private readonly AITool _powerShellTool;
    private readonly AITool _psInfoGetDetailedInfo;

    // Multi-function tools (each function wrapped once)
    private readonly AITool _psInfoGetSystemInfo;
    private readonly AITool _psInfoGetSystemInfoWithApps;

    private readonly AITool _psListAll;
    private readonly AITool _psListByMemory;
    private readonly AITool _psListByName;
    private readonly AITool _psListThreads;
    private readonly AITool _registryReadTool;
    private readonly AITool _reliabilityHistoryReadTool;
    private readonly AITool _serviceHealthReadTool;
    private readonly AITool _startupInventoryReadTool;

    private readonly AITool _storageHealthReadTool;

    // Single-function tools (wrapped once)
    private readonly AITool _webSearchTool;
    private readonly AITool _windowsEventChannelReadTool;
    private readonly AITool _windowsWmiReadTool;








    public ToolBuilder(EventLogSweepTool eventLogSweepTool, WebSearchPlugin webSearchPlugin, PowerShellTool powerShellTool, FileContentsReadingTool fileContentsReadingTool, FileSystemWriterTool fileSystemWriterTool, InstalledUpdatesTool installedUpdatesTool, ListFolderContentsTool listFolderContentsTool, LogFileListingTool logFileLister, LogFileReader logFileReader, NetworkConfigurationTool networkConfigurationTool, PerformanceCounterTool performanceCounterTool, RegistryReaderTool registryReaderTool, ReliabilityHistoryTool reliabilityHistoryTool, ServiceHealthTool serviceHealthTool, StartupInventoryTool startupInventoryTool, StorageHealthTool storageHealthTool, WindowsEventChannelReaderTool windowsEventChannelReaderTool, WindowsWmiReaderTool windowsWmiReaderTool, PsInfoTool psInfoTool, PsListTool psListTool, NsLookupTool nsLookupTool, NetStatTool netStatTool, HandleTool handleTool)
    {
        // Single-function tools
        _webSearchTool = AIFunctionFactory.Create(webSearchPlugin.WebSearch);
        _powerShellTool = AIFunctionFactory.Create(powerShellTool.RunReadOnly);
        _fileContentsReadingTool = AIFunctionFactory.Create(fileContentsReadingTool.ReadFileContents);
        _fileSystemWriterTool = AIFunctionFactory.Create(fileSystemWriterTool.WriteText);
        _installedUpdatesReadTool = AIFunctionFactory.Create(installedUpdatesTool.ReadInstalledUpdates);
        _listFolderContentsTool = AIFunctionFactory.Create(listFolderContentsTool.ListFolderContents);
        _logFileListerTool = AIFunctionFactory.Create(logFileLister.GetLogFileList);
        _logFileReaderTool = AIFunctionFactory.Create(logFileReader.LogFileReaderTool);
        _networkConfigurationReadTool = AIFunctionFactory.Create(networkConfigurationTool.ReadActiveAdapters);
        _performanceCounterReadTool = AIFunctionFactory.Create(performanceCounterTool.ReadSnapshot);
        _windowsEventChannelReadTool = AIFunctionFactory.Create(windowsEventChannelReaderTool.ReadEventLogChannel);
        _registryReadTool = AIFunctionFactory.Create(registryReaderTool.ReadValue);
        _reliabilityHistoryReadTool = AIFunctionFactory.Create(reliabilityHistoryTool.ReadRecent);
        _serviceHealthReadTool = AIFunctionFactory.Create(serviceHealthTool.ReadServices);
        _startupInventoryReadTool = AIFunctionFactory.Create(startupInventoryTool.ReadStartupItems);
        _storageHealthReadTool = AIFunctionFactory.Create(storageHealthTool.ReadLogicalDisks);
        _windowsWmiReadTool = AIFunctionFactory.Create(windowsWmiReaderTool.ReadClass);
        _eventLogSweepTool = AIFunctionFactory.Create(eventLogSweepTool.ReadRecentWarningsAndErrorsAcrossAllLogs);


        // Multi-function tools (each function wrapped once)
        _psInfoGetSystemInfo = AIFunctionFactory.Create(psInfoTool.GetSystemInfo);
        _psInfoGetDetailedInfo = AIFunctionFactory.Create(psInfoTool.GetDetailedInfo);
        _psInfoGetSystemInfoWithApps = AIFunctionFactory.Create(psInfoTool.GetSystemInfoWithApps);

        _psListAll = AIFunctionFactory.Create(psListTool.ListAll);
        _psListByMemory = AIFunctionFactory.Create(psListTool.ListByMemory);
        _psListByName = AIFunctionFactory.Create(psListTool.ListByName);
        _psListThreads = AIFunctionFactory.Create(psListTool.ListThreads);

        _nsLookupResolve = AIFunctionFactory.Create(nsLookupTool.Resolve);
        _nsLookupResolveWithServer = AIFunctionFactory.Create(nsLookupTool.ResolveWithServer);
        _nsLookupReverseLookup = AIFunctionFactory.Create(nsLookupTool.ReverseLookup);

        _netStatGetStatistics = AIFunctionFactory.Create(netStatTool.GetStatistics);
        _netStatGetRoutingTable = AIFunctionFactory.Create(netStatTool.GetRoutingTable);
        _netStatListTcpConnections = AIFunctionFactory.Create(netStatTool.ListTcpConnections);
        _netStatListWithProcessIds = AIFunctionFactory.Create(netStatTool.ListWithProcessIds);

        _handleListAll = AIFunctionFactory.Create(handleTool.ListAll);
        _handleListForProcess = AIFunctionFactory.Create(handleTool.ListForProcess);
        _handleSearchHandles = AIFunctionFactory.Create(handleTool.SearchHandles);
    }








    public IList<AITool> GetAiTools()
    {
        return this.GetReadOnlyAiTools();
    }








    public IList<AITool> GetReadOnlyAiTools()
    {
        return
        [
                // Single-function tools
                _webSearchTool,
                _eventErrorsTool,
                _powerShellTool,
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
                _eventLogSweepTool,

                // Multi-function tools
                _psInfoGetSystemInfo,
                _psInfoGetDetailedInfo,
                _psInfoGetSystemInfoWithApps,

                _psListAll,
                _psListByMemory,
                _psListByName,
                _psListThreads,

                _nsLookupResolve,
                _nsLookupResolveWithServer,
                _nsLookupReverseLookup,

                _netStatGetStatistics,
                _netStatGetRoutingTable,
                _netStatListTcpConnections,
                _netStatListWithProcessIds,

                _handleListAll,
                _handleListForProcess,
                _handleSearchHandles
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