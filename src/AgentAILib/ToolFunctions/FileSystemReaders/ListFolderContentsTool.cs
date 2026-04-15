// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         ListFolderContentsTool.cs
// Author: Kyle L. Crowder
// Build Num: 194508



using System.ComponentModel;
using System.IO;

using AgentAILib.ToolFunctions.Utils;




namespace AgentAILib.ToolFunctions.FileSystemReaders;





[Description("Lists files and subfolders from configured allowlisted roots.")]
public sealed class ListFolderContentsTool
{
    private readonly IReadOnlyList<string> _allowedRoots;








    public ListFolderContentsTool(string[]? allowedRoots)
    {
        _allowedRoots = allowedRoots?.ToArray() ?? [];
    }








    [Description("List the contents of an allowlisted folder path.")]
    public ToolResult<FolderContentsResult> ListFolderContents([Description("Relative or absolute directory path constrained to the configured allowlisted roots.")] string path)
    {
        try
        {
            var resolvedPathResult = PathResolver.ResolveExistingDirectory(path, _allowedRoots);
            if (!resolvedPathResult.Success)
            {
                return ToolResult<FolderContentsResult>.Fail(resolvedPathResult.Error!);
            }

            ResolvedPath resolvedPath = resolvedPathResult.Value!;
            IReadOnlyList<string> entries = Directory.GetFileSystemEntries(resolvedPath.FullPath).Select(Path.GetFileName).Where(name => !string.IsNullOrWhiteSpace(name)).Cast<string>().OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToList().AsReadOnly();

            return ToolResult<FolderContentsResult>.Ok(new FolderContentsResult { AllowedRoot = resolvedPath.AllowedRoot, FullPath = resolvedPath.FullPath, Entries = entries });
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            return ToolResult<FolderContentsResult>.Fail($"Directory listing failed: {ex.Message}");
        }
    }








    public sealed class FolderContentsResult
    {
        public required string AllowedRoot { get; init; }
        public required IReadOnlyList<string> Entries { get; init; }
        public required string FullPath { get; init; }
    }
}