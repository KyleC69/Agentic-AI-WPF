// 2026/03/05
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








    public void WriteFile(string relativePath, string content)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new ArgumentException("Path cannot be empty.");
        }

        string fullPath = Path.GetFullPath(Path.Combine(_sandboxRoot, relativePath));

        if (!fullPath.StartsWith(_sandboxRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Access outside sandbox is not allowed.");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, content ?? string.Empty);
    }
}