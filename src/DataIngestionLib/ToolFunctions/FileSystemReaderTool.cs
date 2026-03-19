// Build Date: 2026/03/19
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         FileSystemReaderTool.cs
// Author: Kyle L. Crowder
// Build Num: 044301



using System.ComponentModel;
using System.IO;

using OllamaSharp;




namespace DataIngestionLib.ToolFunctions;





[OllamaTool]
[Description("Reads files from the file system. The input is a path to a file, and the output is the contents of the file.")]
public sealed class FileSystemReaderTool
{

    public static ToolResult<string> ReadFile(string relativePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return ToolResult<string>.Fail("Path cannot be empty.");
            }

            var fullPath = Path.GetFullPath(relativePath);



            if (!File.Exists(fullPath))
            {
                return ToolResult<string>.Fail($"File not found: {relativePath}");
            }

            var content = File.ReadAllText(fullPath);

            return ToolResult<string>.Ok(content);
        }
        catch (Exception ex)
        {
            // Internal exception is captured and returned deterministically
            return ToolResult<string>.Fail(ex.Message);
        }
    }
}