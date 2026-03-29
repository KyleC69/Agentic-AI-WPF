// Build Date: 2026/03/29
// Solution: File
// Project:   RAGDataIngestionWPF
// File:         LoggingLevelSwitch.cs
// Author: Kyle L. Crowder
// Build Num: 051953



using Microsoft.Extensions.Logging;




namespace RAGDataIngestionWPF.Models;





/// <summary>
///     A thread-safe mutable holder for the application's minimum log level.
///     Captured by the logging filter lambda at host build time so that runtime
///     changes propagate to every <see cref="ILogger" /> without rebuilding the host.
/// </summary>
public sealed class LoggingLevelSwitch
{
    private volatile int _minimumLevel = (int)LogLevel.Trace;

    /// <summary>
    ///     Gets or sets the current minimum <see cref="LogLevel" />.
    ///     Writes are immediately visible to all threads because the backing field
    ///     is <see langword="volatile" />.
    /// </summary>
    /// <remarks>
    ///     <c>volatile</c> guarantees memory-visibility and prevents reordering, but does
    ///     not guarantee atomicity of read-modify-write sequences.  Because this property
    ///     only ever performs simple, independent assignments (set a new level; no
    ///     conditional update based on the current value), the current implementation is
    ///     safe under concurrent access.  If conditional update semantics are added in
    ///     the future, migrate the backing field to <see cref="System.Threading.Interlocked" />
    ///     operations to preserve correctness.
    /// </remarks>
    public LogLevel MinimumLevel
    {
        get { return (LogLevel)_minimumLevel; }
        set { _minimumLevel = (int)value; }
    }
}