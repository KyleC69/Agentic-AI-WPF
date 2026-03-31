// Build Date: 2026/03/30
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         SafeCommandRunner.cs
// Author: Kyle L. Crowder
// Build Num: 233144



using System.IO;




namespace DataIngestionLib.ToolFunctions;





public sealed class SafeCommandRunner(string sandboxRoot)
{
    private readonly string _sandboxRoot = SandboxPathResolver.NormalizeRoot(sandboxRoot);

    private static readonly HashSet<string> AllowedCommands = new(StringComparer.OrdinalIgnoreCase)
    {
            "dir",
            "ls",
            "type",
            "cat",
            "echo"
    };








    internal ToolResult<string> ExecuteAllowedCommand(string cmd, string args)
    {
        switch (cmd.ToUpperInvariant())
        {
            case "ECHO":
                return ToolResult<string>.Ok(args);

            case "DIR":
            case "LS":
                return ToolResult<string>.Ok(string.Join("\n", Directory.GetFiles(_sandboxRoot).Select(Path.GetFileName)));

            case "CAT":
            case "TYPE":
                if (!SandboxPathResolver.TryResolveFilePath(_sandboxRoot, args, out var fullPath, out _))
                {
                    return ToolResult<string>.Fail("Access denied.");
                }

                if (!File.Exists(fullPath))
                {
                    return ToolResult<string>.Fail("File not found.");
                }

                return ToolResult<string>.Ok(File.ReadAllText(fullPath));

            default:
                return ToolResult<string>.Fail("Command not implemented.");
        }
    }








    public ToolResult<string> Run(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return ToolResult<string>.Fail("No command provided.");
        }

        var parts = input.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var cmd = parts[0];
        var args = parts.Length > 1 ? parts[1] : string.Empty;

        return !AllowedCommands.Contains(cmd) ? ToolResult<string>.Fail($"Command '{cmd}' is not allowed.") : ExecuteAllowedCommand(cmd, args);

    }
}