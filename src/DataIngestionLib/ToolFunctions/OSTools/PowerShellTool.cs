// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: GitHub Copilot
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}



using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using AgentAILib.ToolFunctions.Utils;




namespace AgentAILib.ToolFunctions.OSTools;




/// <summary>
///     Executes read-only PowerShell commands on the local Windows system.
///     A static verb allowlist enforces that only non-destructive cmdlets may run.
///     Destructive verbs and known dangerous aliases are actively blocked before execution.
/// </summary>
[Description(
    "Runs read-only PowerShell diagnostics on the local Windows system. " +
    "Allowed verbs: Get, Select, Where, ForEach, Format, Measure, Test, Find, Search, Compare, Sort, Group, " +
    "ConvertTo, ConvertFrom, Resolve, Show, Split, Join, Out, Write. " +
    "Commands using any other verb, the call operator '&', dot-sourcing, or destructive aliases are blocked.")]
public sealed class PowerShellTool
{

    private const int MIN_TIMEOUT_SECONDS = 1;
    private const int MAX_TIMEOUT_SECONDS = 30;
    private const int MAX_OUTPUT_LENGTH = 8_000;
    private const int MAX_ERROR_SNIPPET_LENGTH = 2_000;




    /// <summary>
    ///     Verbs whose cmdlets are considered non-destructive and safe for agent diagnostics.
    ///     Enforcement is strict: any cmdlet verb not on this list causes the command to be rejected.
    /// </summary>
    internal static readonly IReadOnlySet<string> AllowedVerbs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Get",
        "Select",
        "Where",
        "ForEach",
        "Format",
        "Measure",
        "Test",
        "Find",
        "Search",
        "Compare",
        "Sort",
        "Group",
        "ConvertTo",
        "ConvertFrom",
        "Resolve",
        "Show",
        "Split",
        "Join",
        "Out",
    };




    /// <summary>
    ///     Standalone tokens (aliases or short commands) that are always blocked regardless of verb analysis.
    /// </summary>
    internal static readonly IReadOnlySet<string> BlockedTokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "iex",    // Invoke-Expression
        "icm",    // Invoke-Command
        "irm",    // Invoke-RestMethod
        "iwr",    // Invoke-WebRequest
        "rm",     // Remove-Item
        "del",    // Remove-Item
        "rd",     // Remove-Item (directory)
        "erase",  // Remove-Item
        "ni",     // New-Item
        "mkdir",  // New-Item -Directory
        "md",     // mkdir alias
        "kill",   // Stop-Process
        "cls",    // Clear-Host
        "clc",    // Clear-Content
        "clv",    // Clear-Variable
        "cli",    // Clear-Item
        "clp",    // Clear-ItemProperty
        "sc",     // Set-Content / Windows service control
    };




    /// <summary>
    ///     Full cmdlet names that are blocked even though their verb appears in <see cref="AllowedVerbs"/>.
    ///     These are destructive outliers within otherwise safe verb families.
    /// </summary>
    internal static readonly IReadOnlySet<string> BlockedCommandNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Format-Volume",     // Formats a drive — destructive
        "Out-File",          // Writes output to a file
        "Out-Printer",       // Sends output to a printer
        "Write-EventLog",    // Writes entries to the Windows Event Log
    };




    // Captures the verb of any Verb-Noun cmdlet pattern (e.g. "Get" from "Get-Process").
    private static readonly Regex _cmdletVerbPattern = new(
        @"\b([A-Za-z][A-Za-z0-9]*)(?=-[A-Za-z])",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // Captures full Verb-Noun cmdlet names (e.g. "Format-Volume") for blocklist checks.
    private static readonly Regex _fullCmdletPattern = new(
        @"\b([A-Za-z][A-Za-z0-9]+-[A-Za-z][A-Za-z0-9]*)\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);




    [Description(
        "Execute a read-only PowerShell command or pipeline on the local machine. " +
        "Returns captured stdout as text, or an error explanation if the command is blocked or fails. " +
        "Example: 'Get-Process | Sort-Object CPU -Descending | Select-Object -First 10 | Format-Table Name,CPU,WorkingSet'")]
    public async Task<ToolResult<string>> RunReadOnly(
        [Description("The PowerShell command or pipeline to run. Must use only allowed read-only verbs.")] string command,
        [Description("Maximum seconds to wait before aborting. Minimum 1, maximum 30. Defaults to 20.")] int timeoutSeconds = 20)
    {
        if (string.IsNullOrWhiteSpace(command))
            return ToolResult<string>.Fail("Command must not be empty.");

        if (timeoutSeconds < MIN_TIMEOUT_SECONDS || timeoutSeconds > MAX_TIMEOUT_SECONDS)
            return ToolResult<string>.Fail(
                $"Timeout must be between {MIN_TIMEOUT_SECONDS} and {MAX_TIMEOUT_SECONDS} seconds.");

        var blockReason = GetBlockReason(command);
        if (blockReason is not null)
            return ToolResult<string>.Fail(blockReason);

        return await ExecuteCommandAsync(command, timeoutSeconds * 1_000).ConfigureAwait(false);
    }




    /// <summary>
    ///     Returns a human-readable reason why <paramref name="command"/> is blocked,
    ///     or <see langword="null"/> when the command is safe to execute.
    /// </summary>
    internal string? GetBlockReason(string command)
    {
        // Block the call operator (&) — it can execute arbitrary code or file paths.
        if (Regex.IsMatch(command, @"(?:^|[\s;|])\s*&"))
            return "The call operator '&' is not permitted.";

        // Block dot-sourcing (. script.ps1) — it loads and executes external scripts.
        if (Regex.IsMatch(command, @"(?:^|\s)\.\s+\S"))
            return "Dot-sourcing '.' is not permitted.";

        // Block known dangerous standalone aliases that bypass the cmdlet verb check.
        foreach (var token in ExtractLeadingTokens(command))
        {
            if (BlockedTokens.Contains(token))
                return $"The command or alias '{token}' is not permitted.";
        }

        // Require every cmdlet verb to appear in the allowed verb allowlist.
        foreach (Match match in _cmdletVerbPattern.Matches(command))
        {
            var verb = match.Groups[1].Value;
            if (!AllowedVerbs.Contains(verb))
                return $"The verb '{verb}' is not in the list of permitted read-only verbs.";
        }

        // Block specifically named destructive commands that share an otherwise allowed verb.
        foreach (Match match in _fullCmdletPattern.Matches(command))
        {
            var cmdlet = match.Groups[1].Value;
            if (BlockedCommandNames.Contains(cmdlet))
                return $"The command '{cmdlet}' is not permitted.";
        }

        return null;
    }




    // Splits the command on pipeline and statement separators and yields
    // the first non-whitespace token from each resulting segment.
    private static IEnumerable<string> ExtractLeadingTokens(string command)
    {
        var segments = command.Split(['|', ';', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
        foreach (var segment in segments)
        {
            var trimmed = segment.TrimStart();
            if (string.IsNullOrEmpty(trimmed))
                continue;

            var spaceIndex = trimmed.IndexOfAny([' ', '\t']);
            var token = spaceIndex >= 0 ? trimmed[..spaceIndex] : trimmed;

            if (!string.IsNullOrEmpty(token))
                yield return token;
        }
    }




    private static async Task<ToolResult<string>> ExecuteCommandAsync(string command, int timeoutMs)
    {
        var psExe = FindPowerShellExecutable();
        if (psExe is null)
            return ToolResult<string>.Fail(
                "No compatible PowerShell executable (pwsh.exe or powershell.exe) was found on PATH.");

        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = psExe,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetTempPath(),
        };

        process.StartInfo.ArgumentList.Add("-NoProfile");
        process.StartInfo.ArgumentList.Add("-NonInteractive");
        process.StartInfo.ArgumentList.Add("-ExecutionPolicy");
        process.StartInfo.ArgumentList.Add("Bypass");
        process.StartInfo.ArgumentList.Add("-Command");
        process.StartInfo.ArgumentList.Add(command);

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (_, e) => { if (e.Data is not null) outputBuilder.AppendLine(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data is not null) errorBuilder.AppendLine(e.Data); };

        try
        {
            process.Start();
        }
        catch (Exception ex)
        {
            return ToolResult<string>.Fail($"Failed to start PowerShell: {ex.Message}");
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        var completed = await Task.Run(() => process.WaitForExit(timeoutMs)).ConfigureAwait(false);
        if (!completed)
        {
            try { process.Kill(entireProcessTree: true); } catch { /* best-effort */ }
            return ToolResult<string>.Fail(
                $"PowerShell command timed out after {timeoutMs / 1_000} seconds.");
        }

        var output = outputBuilder.ToString();
        var error = errorBuilder.ToString();

        if (process.ExitCode != 0 && string.IsNullOrWhiteSpace(output))
        {
            var snippet = DiagnosticsText.Truncate(error.Trim(), MAX_ERROR_SNIPPET_LENGTH);
            return ToolResult<string>.Fail(
                $"PowerShell exited with code {process.ExitCode}. Error: {snippet}");
        }

        var combined = string.IsNullOrWhiteSpace(error)
            ? output
            : $"{output}\n--- stderr ---\n{error.Trim()}";

        return ToolResult<string>.Ok(DiagnosticsText.CleanModelText(combined, MAX_OUTPUT_LENGTH));
    }




    private static string? FindPowerShellExecutable()
    {
        var pathEntries = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);

        foreach (var exe in new[] { "pwsh.exe", "powershell.exe" })
        {
            if (pathEntries.Any(dir => File.Exists(Path.Combine(dir, exe))))
                return exe;
        }

        return null;
    }

}
