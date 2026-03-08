// 2026/03/07
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         SandboxFileReader.cs
//   Author: Kyle L. Crowder



using System.IO;




namespace DataIngestionLib.ToolFunctions;





public sealed class SandboxFileReader
{
    private readonly string _sandboxRoot;








    public SandboxFileReader(string sandboxRoot)
    {
        _sandboxRoot = Path.GetFullPath(sandboxRoot);
        Directory.CreateDirectory(_sandboxRoot);
    }








    public string ReadFile(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new ArgumentException("Path cannot be empty.");
        }

        var fullPath = Path.GetFullPath(Path.Combine(_sandboxRoot, relativePath));

        return !fullPath.StartsWith(_sandboxRoot, StringComparison.OrdinalIgnoreCase) ? throw new UnauthorizedAccessException("Access outside sandbox is not allowed.") : !File.Exists(fullPath) ? throw new FileNotFoundException("File not found.", fullPath) : File.ReadAllText(fullPath);

    }
}