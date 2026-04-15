// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         FileSystemWriterTool.cs
// Author: Kyle L. Crowder
// Build Num: 194509



using System.ComponentModel;
using System.IO;

using AgentAILib.ToolFunctions.Utils;




namespace AgentAILib.ToolFunctions.FileSystemWriters;





public sealed class FileSystemWriterTool
{
    private readonly IReadOnlyList<string> _allowedRoots;








    public FileSystemWriterTool(string[]? allowedRoots)
    {
        _allowedRoots = allowedRoots?.ToArray() ?? [];
    }








    [Description("Write text content to an allowlisted file path. Creates or overwrites the target file.")]
    public ToolResult<FileWriteResult> WriteText([Description("Relative or absolute file path constrained to the configured allowlisted roots.")] string path, [Description("Text content to write to the file.")] string content)
    {
        try
        {
            var resolvedPathResult = PathResolver.ResolveFilePathForWrite(path, _allowedRoots);
            if (!resolvedPathResult.Success)
            {
                return ToolResult<FileWriteResult>.Fail(resolvedPathResult.Error!);
            }

            ResolvedPath resolvedPath = resolvedPathResult.Value!;

            File.WriteAllText(resolvedPath.FullPath, content ?? string.Empty);
            return ToolResult<FileWriteResult>.Ok(new FileWriteResult { AllowedRoot = resolvedPath.AllowedRoot, FullPath = resolvedPath.FullPath, CharacterCount = content?.Length ?? 0 });
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            return ToolResult<FileWriteResult>.Fail($"File write failed for '{path}': {ex.Message}");
        }
    }








    public sealed class FileWriteResult
    {
        public required string AllowedRoot { get; init; }
        public int CharacterCount { get; init; }
        public required string FullPath { get; init; }
    }
}