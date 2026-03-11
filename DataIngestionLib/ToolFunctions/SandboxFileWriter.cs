// 2026/03/10
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         SandboxFileWriter.cs
//   Author: Kyle L. Crowder



using System.IO;




namespace DataIngestionLib.ToolFunctions;





public sealed class SandboxFileWriter
{
    private readonly string _sandboxRoot;








    public SandboxFileWriter(string sandboxRoot)
    {
        _sandboxRoot = Path.GetFullPath(sandboxRoot);
        Directory.CreateDirectory(_sandboxRoot);
    }








    public ToolResult<string> WriteFile(string relativePath, string content)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return ToolResult<string>.Fail("Path cannot be empty.");
        }

        try
        {
            var fullPath = Path.GetFullPath(Path.Combine(_sandboxRoot, relativePath));

            if (!fullPath.StartsWith(_sandboxRoot, StringComparison.OrdinalIgnoreCase))
            {
                return ToolResult<string>.Fail("Access outside sandbox is not allowed.");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath, content ?? string.Empty);
            return ToolResult<string>.Ok($"Wrote file {relativePath}");
        }
        catch (IOException ex)
        {
            return ToolResult<string>.Fail($"I/O error writing file: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            return ToolResult<string>.Fail($"Access denied writing file: {ex.Message}");
        }
    }
}