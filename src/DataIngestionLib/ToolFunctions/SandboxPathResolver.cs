// Build Date: 2026/03/29
// Solution: File
// Project:   DataIngestionLib
// File:         SandboxPathResolver.cs
// Author: Kyle L. Crowder
// Build Num: 051943



using System.IO;




namespace DataIngestionLib.ToolFunctions;





internal static class SandboxPathResolver
{

    private static string GetNearestExistingPath(string path)
    {
        var currentPath = path;

        while (!string.IsNullOrEmpty(currentPath) && !File.Exists(currentPath) && !Directory.Exists(currentPath))
        {
            currentPath = Path.GetDirectoryName(currentPath);
        }

        return currentPath ?? path;
    }








    internal static string NormalizeRoot(string sandboxRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sandboxRoot);

        return TrimTrailingDirectorySeparator(Path.GetFullPath(sandboxRoot));
    }








    private static bool PathContainsReparsePoint(string candidatePath, string normalizedRoot)
    {
        var currentPath = GetNearestExistingPath(candidatePath);

        while (!string.IsNullOrEmpty(currentPath))
        {
            if ((File.GetAttributes(currentPath) & FileAttributes.ReparsePoint) != 0)
            {
                return true;
            }

            if (string.Equals(currentPath, normalizedRoot, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            currentPath = Path.GetDirectoryName(currentPath);
        }

        return false;
    }








    private static string TrimTrailingDirectorySeparator(string path)
    {
        if (path.Length <= 1)
        {
            return path;
        }

        return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }








    internal static bool TryResolveFilePath(string sandboxRoot, string path, out string? fullPath, out string? error)
    {
        fullPath = null;
        error = null;

        if (string.IsNullOrWhiteSpace(path))
        {
            error = "Path cannot be empty.";
            return false;
        }

        if (Path.IsPathRooted(path))
        {
            error = "Access denied: path is outside the sandbox.";
            return false;
        }

        var normalizedRoot = NormalizeRoot(sandboxRoot);
        var candidatePath = Path.GetFullPath(Path.Combine(normalizedRoot, path));
        var relativePath = Path.GetRelativePath(normalizedRoot, candidatePath);

        if (relativePath.Equals("..", StringComparison.Ordinal) || relativePath.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal) || relativePath.StartsWith($"..{Path.AltDirectorySeparatorChar}", StringComparison.Ordinal) || Path.IsPathRooted(relativePath))
        {
            error = "Access denied: path is outside the sandbox.";
            return false;
        }

        if (PathContainsReparsePoint(candidatePath, normalizedRoot))
        {
            error = "Access denied: path uses an unsupported reparse point.";
            return false;
        }

        fullPath = candidatePath;
        return true;
    }
}