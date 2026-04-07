// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ListFolderContentsTool.cs
// Author: Kyle L. Crowder
// Build Num: 212918



using System.ComponentModel;
using System.IO;




namespace DataIngestionLib.ToolFunctions;





[Description("Reads files from the file system. Paths are resolved relative to the configured sandbox root.")]
public sealed class ListFolderContentsTool
{

    [Description("Reads the contents of a folder. Can be used to list files and subfolders. Path can be relative or absolute.")]
    public static ToolResult<List<string>> ListFolderContents(string relativePath)
    {
        try
        {
            if (!PathResolver.TryResolvePath(relativePath, out var fullPath, out var error))
            {
                return ToolResult<List<string>>.Fail(error!);
            }

            if (!Directory.Exists(fullPath))
            {
                return ToolResult<List<string>>.Fail($"Directory not found: {relativePath}");
            }

            var content = Directory.GetFileSystemEntries(fullPath).Select(Path.GetFileName).Where(name => !string.IsNullOrWhiteSpace(name)).ToList()!;

            return ToolResult<List<string>>.Ok(content!);
        }
        catch (UnauthorizedAccessException ex)
        {
            return ToolResult<List<string>>.Fail(ex.Message);
        }
        catch (IOException ex)
        {
            return ToolResult<List<string>>.Fail(ex.Message);
        }
    }
}





//Resolves a path to an existing accessible absolute path from relative or absolute input.
internal static class PathResolver
{

    private static bool CanAccessPath(string resolvedPath, out string? error)
    {
        error = null;

        try
        {
            if (Directory.Exists(resolvedPath))
            {
                using var enumerator = Directory.EnumerateFileSystemEntries(resolvedPath).GetEnumerator();
                _ = enumerator.MoveNext();
                return true;
            }

            using FileStream stream = File.Open(resolvedPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            return true;
        }
        catch (UnauthorizedAccessException ex)
        {
            error = ex.Message;
            return false;
        }
        catch (IOException ex)
        {
            error = ex.Message;
            return false;
        }
    }








    internal static bool TryResolvePath(string path, out string? fullPath, out string? error)
    {
        fullPath = null;
        if (string.IsNullOrWhiteSpace(path))
        {
            error = "Path cannot be empty.";
            return false;
        }

        try
        {
            var candidatePath = Path.IsPathRooted(path) ? path : Path.Combine(Directory.GetCurrentDirectory(), path);
            var resolvedPath = Path.GetFullPath(candidatePath);

            if (!File.Exists(resolvedPath) && !Directory.Exists(resolvedPath))
            {
                error = $"Path not found: {path}";
                return false;
            }

            if (!CanAccessPath(resolvedPath, out error))
            {
                return false;
            }

            fullPath = resolvedPath;
            return true;
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            error = ex.Message;
            return false;
        }
    }
}