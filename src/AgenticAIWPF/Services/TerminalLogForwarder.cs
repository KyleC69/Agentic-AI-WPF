// Build Date: 2026/04/29
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         TerminalLogForwarder.cs
// Author: Kyle L. Crowder
// Build Num: 010000



using System.Diagnostics;
using System.Globalization;
using System.IO;

using Microsoft.Extensions.Logging;



namespace AgenticAIWPF.Services;



#nullable enable



internal sealed class TerminalLogForwarder : IDisposable
{
    private readonly object _syncRoot = new();
    private readonly Process? _process;
    private readonly StreamWriter? _writer;

    public TerminalLogForwarder(string appLocation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appLocation);

        var consolePath = Path.Combine(appLocation, "AgentConsole.exe");
        if (!File.Exists(consolePath))
        {
            return;
        }

        try
        {
            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = consolePath,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    CreateNoWindow = false,
                },
                EnableRaisingEvents = false,
            };

            _process.Start();
            _writer = _process.StandardInput;
            _writer.AutoFlush = true;
        }
        catch
        {
            _writer = null;
            _process = null;
        }
    }

    public void WriteLine(string message)
    {
        ArgumentNullException.ThrowIfNull(message);

        lock (_syncRoot)
        {
            if (_writer is null)
            {
                return;
            }

            try
            {
                _writer.WriteLine(message);
            }
            catch (IOException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }

    public void Dispose()
    {
        lock (_syncRoot)
        {
            _writer?.Dispose();
            _process?.Dispose();
        }
    }
}



internal sealed class TerminalLoggerProvider(TerminalLogForwarder forwarder) : ILoggerProvider
{
    private readonly TerminalLogForwarder _forwarder = forwarder;

    public ILogger CreateLogger(string categoryName)
    {
        return new TerminalLogger(categoryName, _forwarder);
    }

    public void Dispose()
    {
    }
}



internal sealed class TerminalLogger(string categoryName, TerminalLogForwarder forwarder) : ILogger
{
    private readonly string _categoryName = categoryName;
    private readonly TerminalLogForwarder _forwarder = forwarder;

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(formatter);

        var message = formatter(state, exception);
        if (string.IsNullOrWhiteSpace(message) && exception is null)
        {
            return;
        }

        var rendered = string.Format(CultureInfo.InvariantCulture, "[{0:O}] [{1}] {2}: {3}", DateTimeOffset.Now, logLevel, _categoryName, message);
        if (exception != null)
        {
            rendered = string.Concat(rendered, Environment.NewLine, exception);
        }

        _forwarder.WriteLine(rendered);
    }
}
