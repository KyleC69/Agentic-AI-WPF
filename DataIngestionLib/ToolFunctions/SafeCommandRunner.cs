// 2026/03/05
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








    private string ExecuteAllowedCommand(string cmd, string args)
    {
        switch (cmd.ToLowerInvariant())
        {
            case "echo":
                return args;

            case "dir":
            case "ls":
                return string.Join("\n", Directory.GetFiles(_sandboxRoot).Select(Path.GetFileName));

            case "cat":
            case "type":
                string fullPath = Path.GetFullPath(Path.Combine(_sandboxRoot, args));
                if (!fullPath.StartsWith(_sandboxRoot, StringComparison.OrdinalIgnoreCase))
                {
                    return "Access denied.";
                }

                if (!File.Exists(fullPath))
                {
                    return "File not found.";
                }

                return File.ReadAllText(fullPath);

            default:
                return "Command not implemented.";
        }
    }








    public string Run(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return "No command provided.";
        }

        string[] parts = input.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        string cmd = parts[0];
        string args = parts.Length > 1 ? parts[1] : string.Empty;

        return !AllowedCommands.Contains(cmd) ? $"Command '{cmd}' is not allowed." : ExecuteAllowedCommand(cmd, args);

    }
}