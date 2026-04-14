// Build Date: 2026/04/10
// Solution: RAGDataIngestionWPF
// Project:   AgentAILib
// File:         PathResolver.cs
// Author: GitHub Copilot

using System.IO;

namespace AgentAILib.ToolFunctions.Utils;

internal enum ResolvedPathKind
{
    File,
    Directory
}

internal sealed class ResolvedPath
{
    public required string AllowedRoot { get; init; }
    public required string FullPath { get; init; }
    public required string RequestedPath { get; init; }
}

internal static class PathResolver
{
    internal static ToolResult<IReadOnlyList<string>> GetExistingDirectoryRoots(IEnumerable<string>? allowedRoots)
    {
        ToolResult<IReadOnlyList<string>> normalizedRootsResult = NormalizeAllowedRoots(allowedRoots);
        if (!normalizedRootsResult.Success)
        {
            return normalizedRootsResult;
        }

        IReadOnlyList<string> existingRoots = normalizedRootsResult.Value!
                .Where(Directory.Exists)
                .ToList()
                .AsReadOnly();

        return ToolResult<IReadOnlyList<string>>.Ok(existingRoots);
    }

    internal static ToolResult<IReadOnlyList<string>> NormalizeAllowedRoots(IEnumerable<string>? allowedRoots)
    {
        if (allowedRoots == null)
        {
            return ToolResult<IReadOnlyList<string>>.Fail("No allowlisted roots are configured for this tool.");
        }

        HashSet<string> normalizedRoots = new(StringComparer.OrdinalIgnoreCase);

        foreach (var allowedRoot in allowedRoots)
        {
            if (TryNormalizeRoot(allowedRoot, out var normalizedRoot))
            {
                _ = normalizedRoots.Add(normalizedRoot!);
            }
        }

        return normalizedRoots.Count == 0 ? ToolResult<IReadOnlyList<string>>.Fail("No valid allowlisted roots are configured for this tool.") : ToolResult<IReadOnlyList<string>>.Ok(normalizedRoots.ToList().AsReadOnly());
    }

    internal static ToolResult<ResolvedPath> ResolveExistingDirectory(string path, IEnumerable<string>? allowedRoots)
    {
        return ResolveExistingPath(path, allowedRoots, ResolvedPathKind.Directory);
    }

    internal static ToolResult<ResolvedPath> ResolveExistingFile(string path, IEnumerable<string>? allowedRoots)
    {
        return ResolveExistingPath(path, allowedRoots, ResolvedPathKind.File);
    }

    internal static ToolResult<ResolvedPath> ResolveFilePathForWrite(string path, IEnumerable<string>? allowedRoots)
    {
        var sanitizedPath = SanitizePath(path);
        if (string.IsNullOrWhiteSpace(sanitizedPath))
        {
            return ToolResult<ResolvedPath>.Fail("Path cannot be empty.");
        }

        ToolResult<IReadOnlyList<string>> normalizedRootsResult = NormalizeAllowedRoots(allowedRoots);
        if (!normalizedRootsResult.Success)
        {
            return ToolResult<ResolvedPath>.Fail(normalizedRootsResult.Error!);
        }

        IReadOnlyList<string> normalizedRoots = normalizedRootsResult.Value!;
        ToolResult<string> candidatePathResult = Path.IsPathRooted(sanitizedPath) ? TryNormalizeFullPath(sanitizedPath) : TryNormalizeFullPath(Path.Combine(normalizedRoots[0], sanitizedPath));
        if (!candidatePathResult.Success)
        {
            return ToolResult<ResolvedPath>.Fail(candidatePathResult.Error!);
        }

        var candidatePath = candidatePathResult.Value!;
        var matchedRoot = normalizedRoots.FirstOrDefault(root => IsPathWithinRoot(candidatePath, root));
        if (matchedRoot == null)
        {
            return ToolResult<ResolvedPath>.Fail($"Access denied: '{sanitizedPath}' is outside the configured allowlisted roots.");
        }

        if (Path.EndsInDirectorySeparator(candidatePath))
        {
            return ToolResult<ResolvedPath>.Fail("Write path must include a file name.");
        }

        if (Directory.Exists(candidatePath))
        {
            return ToolResult<ResolvedPath>.Fail($"Write path resolves to a directory instead of a file: '{sanitizedPath}'.");
        }

        if (PathContainsReparsePoint(candidatePath, matchedRoot))
        {
            return ToolResult<ResolvedPath>.Fail($"Access denied: '{sanitizedPath}' uses an unsupported reparse point.");
        }

        var parentDirectory = Path.GetDirectoryName(candidatePath);
        if (string.IsNullOrWhiteSpace(parentDirectory) || !Directory.Exists(parentDirectory))
        {
            return ToolResult<ResolvedPath>.Fail($"Parent directory does not exist for '{sanitizedPath}'.");
        }

        return ToolResult<ResolvedPath>.Ok(new ResolvedPath
        {
            RequestedPath = sanitizedPath,
            FullPath = candidatePath,
            AllowedRoot = matchedRoot
        });
    }

    private static string GetNotFoundMessage(string requestedPath, ResolvedPathKind expectedKind)
    {
        var expectedType = expectedKind == ResolvedPathKind.File ? "File" : "Directory";
        return $"{expectedType} not found within the configured allowlisted roots: '{requestedPath}'.";
    }

    private static string GetNearestExistingPath(string path)
    {
        var currentPath = path;

        while (!string.IsNullOrEmpty(currentPath) && !File.Exists(currentPath) && !Directory.Exists(currentPath))
        {
            currentPath = Path.GetDirectoryName(currentPath);
        }

        return currentPath ?? path;
    }

    private static bool IsExpectedKind(string fullPath, ResolvedPathKind expectedKind)
    {
        return expectedKind == ResolvedPathKind.File ? File.Exists(fullPath) : Directory.Exists(fullPath);
    }

    private static bool IsPathWithinRoot(string candidatePath, string normalizedRoot)
    {
        var relativePath = Path.GetRelativePath(normalizedRoot, candidatePath);

        return !relativePath.Equals("..", StringComparison.Ordinal)
                && !relativePath.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !relativePath.StartsWith($"..{Path.AltDirectorySeparatorChar}", StringComparison.Ordinal)
                && !Path.IsPathRooted(relativePath);
    }

    private static bool PathContainsReparsePoint(string candidatePath, string normalizedRoot)
    {
        var currentPath = GetNearestExistingPath(candidatePath);

        while (!string.IsNullOrEmpty(currentPath))
        {
            try
            {
                if ((File.GetAttributes(currentPath) & FileAttributes.ReparsePoint) != 0)
                {
                    return true;
                }
            }
            catch (IOException)
            {
                return true;
            }
            catch (UnauthorizedAccessException)
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

    private static ToolResult<ResolvedPath> ResolveAbsoluteExistingPath(string path, IReadOnlyList<string> normalizedRoots, ResolvedPathKind expectedKind)
    {
        ToolResult<string> candidatePathResult = TryNormalizeFullPath(path);
        if (!candidatePathResult.Success)
        {
            return ToolResult<ResolvedPath>.Fail(candidatePathResult.Error!);
        }

        var candidatePath = candidatePathResult.Value!;
        var sanitizedPath = SanitizePath(path);
        var matchedRoot = normalizedRoots.FirstOrDefault(root => IsPathWithinRoot(candidatePath, root));
        if (matchedRoot == null)
        {
            return ToolResult<ResolvedPath>.Fail($"Access denied: '{sanitizedPath}' is outside the configured allowlisted roots.");
        }

        if (PathContainsReparsePoint(candidatePath, matchedRoot))
        {
            return ToolResult<ResolvedPath>.Fail($"Access denied: '{sanitizedPath}' uses an unsupported reparse point.");
        }

        if (!IsExpectedKind(candidatePath, expectedKind))
        {
            if (File.Exists(candidatePath) || Directory.Exists(candidatePath))
            {
                var expectedType = expectedKind == ResolvedPathKind.File ? "file" : "directory";
                return ToolResult<ResolvedPath>.Fail($"Path '{sanitizedPath}' does not resolve to a {expectedType}.");
            }

            return ToolResult<ResolvedPath>.Fail(GetNotFoundMessage(sanitizedPath, expectedKind));
        }

        return ToolResult<ResolvedPath>.Ok(new ResolvedPath
        {
            RequestedPath = sanitizedPath,
            FullPath = candidatePath,
            AllowedRoot = matchedRoot
        });
    }

    private static ToolResult<ResolvedPath> ResolveExistingPath(string path, IEnumerable<string>? allowedRoots, ResolvedPathKind expectedKind)
    {
        var sanitizedPath = SanitizePath(path);
        if (string.IsNullOrWhiteSpace(sanitizedPath))
        {
            return ToolResult<ResolvedPath>.Fail("Path cannot be empty.");
        }

        ToolResult<IReadOnlyList<string>> normalizedRootsResult = NormalizeAllowedRoots(allowedRoots);
        if (!normalizedRootsResult.Success)
        {
            return ToolResult<ResolvedPath>.Fail(normalizedRootsResult.Error!);
        }

        IReadOnlyList<string> normalizedRoots = normalizedRootsResult.Value!;
        if (Path.IsPathRooted(sanitizedPath))
        {
            return ResolveAbsoluteExistingPath(sanitizedPath, normalizedRoots, expectedKind);
        }

        var attemptedOutsideBounds = false;
        var matchedWrongKind = false;

        foreach (var normalizedRoot in normalizedRoots)
        {
            ToolResult<string> candidatePathResult = TryNormalizeFullPath(Path.Combine(normalizedRoot, sanitizedPath));
            if (!candidatePathResult.Success)
            {
                continue;
            }

            var candidatePath = candidatePathResult.Value!;
            if (!IsPathWithinRoot(candidatePath, normalizedRoot))
            {
                attemptedOutsideBounds = true;
                continue;
            }

            if (PathContainsReparsePoint(candidatePath, normalizedRoot))
            {
                return ToolResult<ResolvedPath>.Fail($"Access denied: '{sanitizedPath}' uses an unsupported reparse point.");
            }

            if (IsExpectedKind(candidatePath, expectedKind))
            {
                return ToolResult<ResolvedPath>.Ok(new ResolvedPath
                {
                    RequestedPath = sanitizedPath,
                    FullPath = candidatePath,
                    AllowedRoot = normalizedRoot
                });
            }

            if (File.Exists(candidatePath) || Directory.Exists(candidatePath))
            {
                matchedWrongKind = true;
            }
        }

        if (attemptedOutsideBounds)
        {
            return ToolResult<ResolvedPath>.Fail($"Access denied: '{sanitizedPath}' is outside the configured allowlisted roots.");
        }

        if (matchedWrongKind)
        {
            var expectedType = expectedKind == ResolvedPathKind.File ? "file" : "directory";
            return ToolResult<ResolvedPath>.Fail($"Path '{sanitizedPath}' does not resolve to a {expectedType}.");
        }

        return ToolResult<ResolvedPath>.Fail(GetNotFoundMessage(sanitizedPath, expectedKind));
    }

    private static string SanitizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        return path.Trim().Trim('"');
    }

    private static ToolResult<string> TryNormalizeFullPath(string path)
    {
        try
        {
            return ToolResult<string>.Ok(Path.GetFullPath(path));
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return ToolResult<string>.Fail($"Path could not be resolved: {ex.Message}");
        }
    }

    private static bool TryNormalizeRoot(string? root, out string? normalizedRoot)
    {
        normalizedRoot = null;

        if (string.IsNullOrWhiteSpace(root))
        {
            return false;
        }

        ToolResult<string> fullPathResult = TryNormalizeFullPath(Environment.ExpandEnvironmentVariables(root.Trim()));
        if (!fullPathResult.Success)
        {
            return false;
        }

        normalizedRoot = TrimTrailingDirectorySeparator(fullPathResult.Value!);
        return !string.IsNullOrWhiteSpace(normalizedRoot);
    }

    private static string TrimTrailingDirectorySeparator(string path)
    {
        return path.Length <= 1 ? path : path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
}
