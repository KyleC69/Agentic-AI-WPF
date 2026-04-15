// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         FileContentsReadingTool.cs
// Author: Kyle L. Crowder
// Build Num: 194507



using System.ComponentModel;
using System.IO;

using AgentAILib.ToolFunctions.Utils;




namespace AgentAILib.ToolFunctions.FileSystemReaders;





[Description("Reads text files from configured allowlisted roots and returns the resolved file path with the file content.")]
public sealed class FileContentsReadingTool
{
    private readonly IReadOnlyList<string> _allowedRoots;








    public FileContentsReadingTool(string[]? allowedRoots)
    {
        _allowedRoots = allowedRoots?.ToArray() ?? [];
    }








    [Description("Read a file's text content from an allowlisted path.")]
    public ToolResult<FileReadResult> ReadFileContents([Description("Relative or absolute file path constrained to the configured allowlisted roots.")] string path)
    {
        try
        {
            var resolvedPathResult = PathResolver.ResolveExistingFile(path, _allowedRoots);
            if (!resolvedPathResult.Success)
            {
                return ToolResult<FileReadResult>.Fail(resolvedPathResult.Error!);
            }

            ResolvedPath resolvedPath = resolvedPathResult.Value!;
            var content = File.ReadAllText(resolvedPath.FullPath);

            return ToolResult<FileReadResult>.Ok(new FileReadResult { AllowedRoot = resolvedPath.AllowedRoot, FullPath = resolvedPath.FullPath, Content = content });
        }
        catch (Exception ex)
        {
            return ToolResult<FileReadResult>.Fail($"File read failed: {ex.Message}");
        }
    }








    public sealed class FileReadResult
    {
        public required string AllowedRoot { get; init; }
        public required string Content { get; init; }
        public required string FullPath { get; init; }
    }
}