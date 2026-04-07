// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         FileSystemWriterTool.cs
// Author: Kyle L. Crowder
// Build Num: 212917



using System.ComponentModel;
using System.IO;




namespace DataIngestionLib.ToolFunctions;





public sealed class FileSystemWriterTool
{

    [Description("Write text content to a file. Path is relative to the sandbox root. Creates or overwrites the file.")]
    public static ToolResult<string> WriteText([Description("File path relative to sandbox root")] string path, [Description("Text content to write")] string content)
    {


        if (string.IsNullOrWhiteSpace(path))
        {
            return ToolResult<string>.Fail("Path cannot be null or whitespace.");
        }

        try
        {
            if (!PathResolver.TryResolvePath(path, out var fullPath, out var error))
            {
                return ToolResult<string>.Fail(error!);
            }

            if (fullPath == null)
            {
                return ToolResult<string>.Fail("Resolved file path was not available.");
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