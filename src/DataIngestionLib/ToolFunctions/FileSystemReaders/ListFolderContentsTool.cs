// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//
//
//
//



using System.ComponentModel;
using System.IO;

using AgentAILib.ToolFunctions.Utils;

namespace AgentAILib.ToolFunctions.FileSystemReaders;

[Description("Lists files and subfolders from configured allowlisted roots.")]
public sealed class ListFolderContentsTool
{
    private readonly IReadOnlyList<string> _allowedRoots;

    public sealed class FolderContentsResult
    {
        public required string AllowedRoot { get; init; }
        public required IReadOnlyList<string> Entries { get; init; }
        public required string FullPath { get; init; }
    }



    public ListFolderContentsTool(string[]? allowedRoots)
    {
        _allowedRoots = allowedRoots?.ToArray() ?? [];
    }

    [Description("List the contents of an allowlisted folder path.")]
    public ToolResult<FolderContentsResult> ListFolderContents([Description("Relative or absolute directory path constrained to the configured allowlisted roots.")] string path)
    {
        try
        {
            ToolResult<ResolvedPath> resolvedPathResult = PathResolver.ResolveExistingDirectory(path, _allowedRoots);
            if (!resolvedPathResult.Success)
            {
                return ToolResult<FolderContentsResult>.Fail(resolvedPathResult.Error!);
            }

            ResolvedPath resolvedPath = resolvedPathResult.Value!;
            IReadOnlyList<string> entries = Directory.GetFileSystemEntries(resolvedPath.FullPath)
                    .Select(Path.GetFileName)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Cast<string>()
                    .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                    .ToList()
                    .AsReadOnly();

            return ToolResult<FolderContentsResult>.Ok(new FolderContentsResult
            {
                AllowedRoot = resolvedPath.AllowedRoot,
                FullPath = resolvedPath.FullPath,
                Entries = entries
            });
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            return ToolResult<FolderContentsResult>.Fail($"Directory listing failed: {ex.Message}");
        }
    }
}