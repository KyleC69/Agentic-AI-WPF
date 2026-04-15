// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         LogFileListingTool.cs
// Author: Kyle L. Crowder
// Build Num: 194508



using System.ComponentModel;
using System.IO;

using AgentAILib.ToolFunctions.Utils;




namespace AgentAILib.ToolFunctions.FileSystemReaders;





public sealed class LogFileListingTool
{
    private readonly IReadOnlyList<string> _allowedRoots;

    private static readonly string[] AllowedExtensions = { ".log", ".txt", ".etl.txt" };








    public LogFileListingTool(string[]? allowedRoots)
    {
        _allowedRoots = allowedRoots?.ToArray() ?? [];
    }








    [Description("List available log files beneath the configured allowlisted log roots. Use the LogFileReaderTool to read the contents of alog file.")]
    public ToolResult<IReadOnlyList<LogFileInfo>> GetLogFileList()
    {
        try
        {
            List<LogFileInfo> results = new();
            var rootsResult = PathResolver.GetExistingDirectoryRoots(_allowedRoots);
            if (!rootsResult.Success)
            {
                return ToolResult<IReadOnlyList<LogFileInfo>>.Fail(rootsResult.Error!);
            }

            foreach (var root in rootsResult.Value!)
            {
                IEnumerable<string> files;
                try
                {
                    files = Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories).Where(f => AllowedExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase));
                }
                catch (Exception)
                {
                    // Skip unreadable roots but continue enumeration
                    continue;
                }

                foreach (var file in files)
                    try
                    {
                        FileInfo info = new(file);
                        results.Add(new LogFileInfo
                        {
                                AllowedRoot = root,
                                FullPath = info.FullName,
                                RelativePath = Path.GetRelativePath(root, info.FullName),
                                SizeBytes = info.Length,
                                LastModifiedUtc = info.LastWriteTimeUtc
                        });
                    }
                    catch
                    {
                        // Skip unreadable files
                    }
            }

            return ToolResult<IReadOnlyList<LogFileInfo>>.Ok(results.OrderByDescending(f => f.LastModifiedUtc).ToList());
        }
        catch (Exception ex)
        {
            return ToolResult<IReadOnlyList<LogFileInfo>>.Fail($"Log enumeration failed: {ex.Message}");
        }
    }








    public sealed class LogFileInfo
    {
        public required string AllowedRoot { get; init; }
        public required string FullPath { get; init; }
        public required DateTime LastModifiedUtc { get; init; }
        public required string RelativePath { get; init; }
        public required long SizeBytes { get; init; }
    }
}