// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   AgentAILib
// File:         LoggingMessages.cs
// Author: Kyle L. Crowder
// Build Num: 212914



using AgentAILib.ToolFunctions.OSTools;

using Microsoft.Extensions.Logging;




namespace AgentAILib.Services;





public static partial class LoggingMessages
{
    [LoggerMessage(LogLevel.Error, "Error fetching RAG data entries: {Message} with exception")]
    public static partial void LogErrorFetchingRAGDataEntriesMessage(this ILogger<RagDataService> logger, string Message);








    [LoggerMessage(LogLevel.Error, "{Message} Path: {KeyPath}")]
    public static partial void LogMessagePathKeypath(this ILogger<RegistryReaderTool> logger, string Message, string KeyPath);








    [LoggerMessage(LogLevel.Error, "Access is only supported on Windows.")]
    public static partial void LogRegistryAccessIsOnlySupportedOnWindows(this ILogger<RegistryReaderTool> logger);








    [LoggerMessage(LogLevel.Error, "Path cannot be null or empty.")]
    public static partial void LogRegistryKeyPathCannotBeNullOrEmpty(this ILogger<RegistryReaderTool> logger);








    [LoggerMessage(LogLevel.Error, "Security exception reading registry key '{KeyPath}' Exception: {Message}.")]
    public static partial void LogSecurityExceptionReadingRegistryKeyKeypath(this ILogger<RegistryReaderTool> logger, string Message, string KeyPath);








    [LoggerMessage(LogLevel.Error, "Unauthorized access exception reading registry key '{KeyPath}' Exception: {Message}")]
    public static partial void LogUnauthorizedAccessExceptionReadingRegistryKeyKeypath(this ILogger<RegistryReaderTool> logger, string Message, string KeyPath);
}