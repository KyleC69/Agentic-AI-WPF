// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//
//
//
//



using System.ComponentModel;
using System.IO;

using OllamaSharp;




namespace DataIngestionLib.ToolFunctions;






[Description("Reads files from the file system. Paths are resolved relative to the configured sandbox root.")]
public sealed class FileContentsReadingTool
{
    private static string? _sandboxRoot;








    public FileContentsReadingTool(string sandboxRoot)
    {
        if (string.IsNullOrWhiteSpace(sandboxRoot))
        {
            throw new ArgumentException("Sandbox root cannot be empty.", nameof(sandboxRoot));
        }

        _sandboxRoot = SandboxPathResolver.NormalizeRoot(sandboxRoot);
    }







    [OllamaTool]
    [Description("Read a file's text content. The path is relative to the sandbox root.")]
    public static ToolResult<string> ReadFileContents(string relativePath)
    {
        try
        {
            if (!SandboxPathResolver.TryResolveFilePath(_sandboxRoot, relativePath, out var fullPath, out var error))
            {
                return ToolResult<string>.Fail(error!);
            }

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