// 2026/03/10
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         FileSystemPlugin.cs
//   Author: Kyle L. Crowder



using System.ComponentModel;
using System.IO;



// ReSharper disable UnusedMember.Global // Invoked via reflection by the agent framework


namespace DataIngestionLib.ToolFunctions;





public class FileSystemWriterTool
{

    [Description("Write text content to a file at the specified path. Creates the file if it does not exist, or overwrites it if it does.")]
    public static ToolResult<string> WriteText(
            [Description("File path")] string path,
            [Description("Text content to write")] string content)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return ToolResult<string>.Fail("Path cannot be null or whitespace.");
        }

        try
        {
            System.IO.File.WriteAllText(path, content ?? string.Empty);
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