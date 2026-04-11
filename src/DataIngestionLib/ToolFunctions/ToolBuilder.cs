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



using DataIngestionLib.Contracts;
using DataIngestionLib.ToolFunctions.FileSystemReaders;
using DataIngestionLib.ToolFunctions.FileSystemWriters;
using DataIngestionLib.ToolFunctions.General;
using DataIngestionLib.ToolFunctions.OSTools;

using Microsoft.Extensions.AI;




namespace DataIngestionLib.ToolFunctions;





public sealed class ToolBuilder : IAIToolCatalog
{
    private readonly AITool _eventLogTool;
    private readonly AITool _fileContentsReadingTool;
    private readonly AITool _fileSystemWriterTool;
    private readonly AITool _installedUpdatesReadTool;
    private readonly AITool _listFolderContentsTool;
    private readonly AITool _logFileListerTool;
    private readonly AITool _logFileReaderTool;
    private readonly AITool _networkConfigurationReadTool;
    private readonly AITool _performanceCounterReadTool;
    private readonly AITool _processSnapshotReadTool;
    private readonly AITool _registryReadTool;
    private readonly AITool _reliabilityHistoryReadTool;
    private readonly AITool _serviceHealthReadTool;
    private readonly AITool _startupInventoryReadTool;
    private readonly AITool _storageHealthReadTool;
    private readonly AITool _webSearchTool;
    private readonly AITool _windowsEventChannelReadTool;
    private readonly AITool _windowsWmiReadTool;








    public ToolBuilder(WebSearchPlugin webSearchPlugin,
            SandboxEventLogReader eventLogReader,
            FileContentsReadingTool fileContentsReadingTool,
            FileSystemWriterTool fileSystemWriterTool,
            InstalledUpdatesTool installedUpdatesTool,
            ListFolderContentsTool listFolderContentsTool,
            LogFileListingTool logFileLister,
            LogFileReader logFileReader,
            NetworkConfigurationTool networkConfigurationTool,
            PerformanceCounterTool performanceCounterTool,
            ProcessSnapshotTool processSnapshotTool,
            RegistryReaderTool registryReaderTool,
            ReliabilityHistoryTool reliabilityHistoryTool,
            ServiceHealthTool serviceHealthTool,
            StartupInventoryTool startupInventoryTool,
            StorageHealthTool storageHealthTool,
            WindowsEventChannelReaderTool windowsEventChannelReaderTool,
            WindowsWmiReaderTool windowsWmiReaderTool)
    {
        _webSearchTool = AIFunctionFactory.Create(webSearchPlugin.WebSearch);
        _eventLogTool = AIFunctionFactory.Create(eventLogReader.ReadLog);
        _fileContentsReadingTool = AIFunctionFactory.Create(fileContentsReadingTool.ReadFileContents);
        _fileSystemWriterTool = AIFunctionFactory.Create(fileSystemWriterTool.WriteText);
        _installedUpdatesReadTool = AIFunctionFactory.Create(installedUpdatesTool.ReadInstalledUpdates);
        _listFolderContentsTool = AIFunctionFactory.Create(listFolderContentsTool.ListFolderContents);
        _logFileListerTool = AIFunctionFactory.Create(logFileLister.GetLogFileList);
        _logFileReaderTool = AIFunctionFactory.Create(logFileReader.LogFileReaderTool);
        _networkConfigurationReadTool = AIFunctionFactory.Create(networkConfigurationTool.ReadActiveAdapters);
        _performanceCounterReadTool = AIFunctionFactory.Create(performanceCounterTool.ReadSnapshot);
        _processSnapshotReadTool = AIFunctionFactory.Create(processSnapshotTool.ReadTopProcesses);
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
        return this.GetReadOnlyAiTools();
    }








    public IList<AITool> GetReadOnlyAiTools()
    {
        return
        [
                _webSearchTool,
                _eventLogTool,
                _fileContentsReadingTool,
                _installedUpdatesReadTool,
                _listFolderContentsTool,
                _logFileListerTool,
                _logFileReaderTool,
                _networkConfigurationReadTool,
                _performanceCounterReadTool,
                _processSnapshotReadTool,
                _windowsEventChannelReadTool,
                _registryReadTool,
                _reliabilityHistoryReadTool,
                _serviceHealthReadTool,
                _startupInventoryReadTool,
                _storageHealthReadTool,
                _windowsWmiReadTool

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