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

    [Description("Write text content to a file at the specified path. Creates the file if it does not exist, or overwrites it if it does.")]
    public static ToolResult<string> WriteText([Description("File path")] string path, [Description("Text content to write")] string content)
    {


        if (string.IsNullOrWhiteSpace(path))
        {
            return ToolResult<string>.Fail("Path cannot be null or whitespace.");
        }

        try
        {
            File.WriteAllText(path, content);
            return ToolResult<string>.Ok($"Wrote file {path}");
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