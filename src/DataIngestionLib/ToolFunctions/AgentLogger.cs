// Build Date: 2026/03/30
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         AgentLogger.cs
// Author: Kyle L. Crowder
// Build Num: 233139



using System.IO;




namespace DataIngestionLib.ToolFunctions;





/// <summary>
///     Writes timestamped agent log entries to a single file in a specified directory.
/// </summary>
/// <remarks>
///     This logger uses UTC timestamps in round-trip format (<c>O</c>) to preserve
///     ordering and timezone-independent diagnostics across environments.
/// </remarks>
public sealed class AgentLogger
{
    private readonly string _logFile;
    private const string LogsDirectoryName = "logs";








    /// <summary>
    ///     Initializes a new instance of the <see cref="AgentLogger" /> class.
    /// </summary>
    public AgentLogger() : this(Environment.CurrentDirectory)
    {
    }








    internal AgentLogger(string sandboxRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sandboxRoot);

        var logRoot = Path.Combine(SandboxPathResolver.NormalizeRoot(sandboxRoot), LogsDirectoryName);
        _ = Directory.CreateDirectory(logRoot);
        _logFile = Path.Combine(logRoot, "agent.log");
    }








    /// <summary>
    ///     Attempts to log a message and ignores the operation result.
    /// </summary>
    /// <param name="message">The message content to append to the log.</param>
    /// <remarks>
    ///     Use <see cref="LogMessage" /> when callers need success/failure details.
    /// </remarks>
    public void Log(string message)
    {
        _ = LogMessage(message);
    }








    /// <summary>
    ///     Appends a timestamped message to the log file.
    /// </summary>
    /// <param name="message">The message content to append.</param>
    /// <returns>
    ///     A successful <see cref="ToolResult{T}" /> when the message is written; otherwise a failed result
    ///     containing validation or file access error details.
    /// </returns>
    /// <remarks>
    ///     Returns failure when <paramref name="message" /> is null, empty, or whitespace.
    /// </remarks>
    public ToolResult<string> LogMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return ToolResult<string>.Fail("Message cannot be null or whitespace.");
        }

        try
        {
            var line = $"{DateTime.Now:O} | {message}";
            File.AppendAllLines(_logFile, [line]);
            return ToolResult<string>.Ok("Message logged.");
        }
        catch (IOException ex)
        {
            return ToolResult<string>.Fail($"I/O error writing to log: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            return ToolResult<string>.Fail($"Access denied writing to log: {ex.Message}");
        }
    }
}