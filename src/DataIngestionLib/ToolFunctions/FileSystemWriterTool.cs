// Build Date: 2026/03/19
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         FileSystemWriterTool.cs
// Author: Kyle L. Crowder
// Build Num: 044302



using System.ComponentModel;
using System.IO;




namespace DataIngestionLib.ToolFunctions;





public sealed class FileSystemWriterTool
{

    private readonly string _sandboxRoot;

    public FileSystemWriterTool(string sandboxRoot)
    {
        if (string.IsNullOrWhiteSpace(sandboxRoot))
        {
            throw new ArgumentException("Sandbox root cannot be empty.", nameof(sandboxRoot));
        }

        _sandboxRoot = Path.GetFullPath(sandboxRoot);
    }

    [Description("Write text content to a file. Path is relative to the sandbox root. Creates or overwrites the file.")]
    public ToolResult<string> WriteText([Description("File path relative to sandbox root")] string path,
        [Description("Text content to write")] string content)
    {


        if (string.IsNullOrWhiteSpace(path))
        {
            return ToolResult<string>.Fail("Path cannot be null or whitespace.");
        }

        try
        {
            var fullPath = Path.GetFullPath(Path.Combine(_sandboxRoot, path));
            if (!fullPath.StartsWith(_sandboxRoot, StringComparison.OrdinalIgnoreCase))
            {
                return ToolResult<string>.Fail("Access denied: path is outside the sandbox.");
            }

            File.WriteAllText(fullPath, content);
            return ToolResult<string>.Ok($"Wrote {fullPath}");
        }
        catch (UnauthorizedAccessException ex)
        {
            return ToolResult<string>.Fail($"Access denied writing file '{path}': {ex.Message}");
        }
        catch (IOException ex)
        {
            return ToolResult<string>.Fail($"I/O error writing file '{path}': {ex.Message}");
        }
    }
}