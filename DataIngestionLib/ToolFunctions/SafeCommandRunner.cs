// 2026/03/10
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         SafeCommandRunner.cs
//   Author: Kyle L. Crowder



using System.IO;




namespace DataIngestionLib.ToolFunctions;





public sealed class SafeCommandRunner
{
    private readonly string _sandboxRoot;

    private static readonly HashSet<string> AllowedCommands = new(StringComparer.OrdinalIgnoreCase)
    {
            "dir", "ls", "cat", "type", "echo"
    };








    public SafeCommandRunner(string sandboxRoot)
    {
        _sandboxRoot = Path.GetFullPath(sandboxRoot);
    }








    private ToolResult<string> ExecuteAllowedCommand(string cmd, string args)
    {
        switch (cmd.ToLowerInvariant())
        {
            case "echo":
                return ToolResult<string>.Ok(args);

            case "dir":
            case "ls":
                return ToolResult<string>.Ok(string.Join("\n", Directory.GetFiles(_sandboxRoot).Select(Path.GetFileName)));

            case "cat":
            case "type":
                var fullPath = Path.GetFullPath(Path.Combine(_sandboxRoot, args));
                if (!fullPath.StartsWith(_sandboxRoot, StringComparison.OrdinalIgnoreCase))
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

        return !AllowedCommands.Contains(cmd)
                ? ToolResult<string>.Fail($"Command '{cmd}' is not allowed.")
                : ExecuteAllowedCommand(cmd, args);

    }
}