// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         CommandExecutor.cs
// Author: Kyle L. Crowder
// Build Num: 194522



using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;




namespace AgentAILib.ToolFunctions;





/// <summary>
///     Core command execution engine. Handles process spawning, output capture,
///     timeouts, and error handling. All tools use this single implementation.
/// </summary>
public sealed class CommandExecutor
{
    private readonly int _defaultTimeoutMs;
    private readonly string _workingDirectory;








    public CommandExecutor(int defaultTimeoutMs = 30_000)
    {
        _defaultTimeoutMs = Math.Min(defaultTimeoutMs, 120_000); // Max 2 min for readonly
        _workingDirectory = GetSafeWorkingDirectory();
    }








    private ProcessStartInfo BuildStartInfo(string command, string? arguments)
    {
        return new ProcessStartInfo
        {
                FileName = command,
                Arguments = arguments ?? string.Empty,
                WorkingDirectory = _workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
        };
    }








    /// <summary>
    ///     Executes a command and returns structured result.
    /// </summary>
    public async Task<CommandResult> ExecuteAsync(string command, string? arguments = null, int? timeoutMs = null)
    {
        var timeout = Math.Min(timeoutMs ?? _defaultTimeoutMs, 120_000);

        try
        {
            using Process process = new();
            process.StartInfo = BuildStartInfo(command, arguments);

            var unused = process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            using CancellationTokenSource cts = new(timeout);

            try
            {
                await process.WaitForExitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                KillProcessTree(process);
                return CommandResult.Timeout(timeout, await outputTask);
            }

            return CommandResult.FromExitCode(exitCode: process.ExitCode, output: (await outputTask).TrimEnd(), error: (await errorTask).TrimEnd());
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 2)
        {
            return CommandResult.NotFound(command);
        }
        catch (Exception ex)
        {
            return CommandResult.Failure($"Execution failed: {ex.Message}");
        }
    }








    private static string GetSafeWorkingDirectory()
    {
        var temp = Path.GetTempPath();
        var sessionDir = Path.Combine(temp, $"AgentTools_{Environment.ProcessId}");
        _ = Directory.CreateDirectory(sessionDir);
        return sessionDir;
    }








    private static void KillProcessTree(Process process)
    {
        try
        {
            process.Kill(entireProcessTree: true);
        }
        catch
        {
            /* Ignore kill failures */
        }
    }
}





/// <summary>
///     Structured result from command execution.
/// </summary>
public class CommandResult
{
    public string? Error { get; init; }
    public int ExitCode { get; init; }
    public string Output { get; init; } = string.Empty;
    public bool Success { get; init; }
    public bool TimedOut { get; init; }








    public static CommandResult Failure(string error)
    {
        return new() { Success = false, ExitCode = -1, Error = error, Output = string.Empty };
    }








    public static CommandResult FromExitCode(int exitCode, string output, string? error = null)
    {
        return new() { Success = exitCode == 0, ExitCode = exitCode, Output = output, Error = error };
    }








    public static CommandResult NotFound(string command)
    {
        return new() { Success = false, ExitCode = -1, Error = $"Command not found: '{command}'. Ensure the executable is in PATH.", Output = string.Empty };
    }








    public static CommandResult Timeout(int timeoutMs, string partialOutput)
    {
        return new()
        {
                Success = false,
                ExitCode = -1,
                Error = $"Command timed out after {timeoutMs}ms",
                Output = partialOutput,
                TimedOut = true
        };
    }
}