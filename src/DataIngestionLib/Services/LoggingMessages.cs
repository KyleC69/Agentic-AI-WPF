// Build Date: 2026/03/19
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         LoggingMessages.cs
// Author: Kyle L. Crowder
// Build Num: 044258



using DataIngestionLib.DocIngestion;
using DataIngestionLib.ToolFunctions;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;




namespace DataIngestionLib.Services;





public static partial class LoggingMessages
{
    [LoggerMessage(LogLevel.Error, "Error fetching RAG data entries: {Message} with exception")]
    public static partial void LogErrorFetchingRAGDataEntriesMessage(this ILogger<RagDataService> logger, string Message);








    [LoggerMessage(LogLevel.Error, "Failed to extract keywords for remote knowledge source. Exception: {Message}")]
    public static partial void LogFailedToExtractKeywordsForRemoteKnowledgeSource(this ILogger<LearningHtmlRunner> logger, string Message);








    [LoggerMessage(LogLevel.Error, "Failed to summarize content for remote knowledge source. Exception: {Message}")]
    public static partial void LogFailedToSummarizeContentForRemoteKnowledgeSource(this ILogger<LearningHtmlRunner> logger, string Message);








    [LoggerMessage(LogLevel.Error, "{Message} Path: {KeyPath}")]
    public static partial void LogMessagePathKeypath(this ILogger<RegistryReaderTool> logger, string Message, string KeyPath);








    [LoggerMessage(LogLevel.Information, "Page metaTitle='{Title}', Description='{Description}', DocumentId='{DocumentId}', UpdatedAt='{UpdatedAt}', MsDate='{MsDate}', OgUrl='{OgUrl}', Summary='{Summary}'")]
    public static partial void LogPageMetatitleTitleDescriptionDescriptionDocumentidDocumentid(this ILogger<LearningHtmlRunner> logger, string Title, string Description, string DocumentId, DateTime UpdatedAt, DateTime MsDate, string OgUrl, string Summary);








    [LoggerMessage(LogLevel.Error, "Access is only supported on Windows.")]
    public static partial void LogRegistryAccessIsOnlySupportedOnWindows(this ILogger<RegistryReaderTool> logger);








    [LoggerMessage(LogLevel.Error, "Path cannot be null or empty.")]
    public static partial void LogRegistryKeyPathCannotBeNullOrEmpty(this ILogger<RegistryReaderTool> logger);








    [LoggerMessage(LogLevel.Error, "Security exception reading registry key '{KeyPath}' Exception: {Message}.")]
    public static partial void LogSecurityExceptionReadingRegistryKeyKeypath(this ILogger<RegistryReaderTool> logger, string Message, string KeyPath);








    [LoggerMessage(LogLevel.Error, "Unauthorized access exception reading registry key '{KeyPath}' Exception: {Message}")]
    public static partial void LogUnauthorizedAccessExceptionReadingRegistryKeyKeypath(this ILogger<RegistryReaderTool> logger, string Message, string KeyPath);








    [LoggerMessage(LogLevel.Information, "Input tokens: {deetsInputTokenCount}, Cached input tokens: {deetsCachedInputTokenCount}, Output tokens: {deetsOutputTokenCount}, Reasoning tokens: {deetsReasoningTokenCount}, Additional counts: {deetsAdditionalCounts}, Total tokens: {deetsTotalTokenCount}")]
    public static partial void LogUsages(this ILogger<ChatConversationService> logger, long? deetsInputTokenCount, long? deetsCachedInputTokenCount, long? deetsOutputTokenCount, long? deetsReasoningTokenCount, AdditionalPropertiesDictionary<long>? deetsAdditionalCounts, long? deetsTotalTokenCount);
}