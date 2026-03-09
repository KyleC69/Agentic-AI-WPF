// 2026/03/08
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
        _ = Directory.CreateDirectory(_sandboxRoot);
    }

    public ToolResult<string> ReadFile(string relativePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return ToolResult<string>.Fail("Path cannot be empty.");
            }

            string fullPath = Path.GetFullPath(Path.Combine(_sandboxRoot, relativePath));

            if (!fullPath.StartsWith(_sandboxRoot, StringComparison.OrdinalIgnoreCase))
            {
                return ToolResult<string>.Fail("Access outside sandbox is not allowed.");
            }

            if (!File.Exists(fullPath))
            {
                return ToolResult<string>.Fail($"File not found: {relativePath}");
            }

            string content = File.ReadAllText(fullPath);

            return ToolResult<string>.Ok(content);
        }
        catch (Exception ex)
        {
            // Internal exception is captured and returned deterministically
            return ToolResult<string>.Fail(ex.Message);
        }
    }
}
public sealed class ToolResult<T>
{
    public bool Success { get; init; }
    public T? Value { get; init; }
    public string? Error { get; init; }

    public static ToolResult<T> Ok(T value)
    {
        return new() { Success = true, Value = value };
    }

    public static ToolResult<T> Fail(string message)
    {
        return new() { Success = false, Error = message };
    }
}
