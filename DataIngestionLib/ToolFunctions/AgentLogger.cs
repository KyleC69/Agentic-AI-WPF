// 2026/03/10
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         AgentLogger.cs
//   Author: Kyle L. Crowder



using System.IO;




namespace DataIngestionLib.ToolFunctions;



/// <summary>
/// Writes timestamped agent log entries to a single file in a specified directory.
/// </summary>
/// <remarks>
/// This logger uses UTC timestamps in round-trip format (<c>O</c>) to preserve
/// ordering and timezone-independent diagnostics across environments.
/// </remarks>
public sealed class AgentLogger
{
    private readonly string _logFile;




    /// <summary>
    /// Initializes a new instance of the <see cref="AgentLogger"/> class.
    /// </summary>
    /// <param name="logDirectory">
    /// The directory where the <c>agent.log</c> file is stored. The directory is created if it does not exist.
    /// </param>
    public AgentLogger(string logDirectory)
    {
        _ = Directory.CreateDirectory(logDirectory);
        _logFile = Path.Combine(logDirectory, "agent.log");
    }





    /// <summary>
    /// Attempts to log a message and ignores the operation result.
    /// </summary>
    /// <param name="message">The message content to append to the log.</param>
    /// <remarks>
    /// Use <see cref="LogMessage"/> when callers need success/failure details.
    /// </remarks>
    public void Log(string message)
    {
        _ = LogMessage(message);
    }





    /// <summary>
    /// Appends a timestamped message to the log file.
    /// </summary>
    /// <param name="message">The message content to append.</param>
    /// <returns>
    /// A successful <see cref="ToolResult{T}"/> when the message is written; otherwise a failed result
    /// containing validation or file access error details.
    /// </returns>
    /// <remarks>
    /// Returns failure when <paramref name="message"/> is null, empty, or whitespace.
    /// </remarks>
    public ToolResult<string> LogMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return ToolResult<string>.Fail("Message cannot be null or whitespace.");
        }

        try
        {
            string line = $"{DateTime.UtcNow:O} | {message}";
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