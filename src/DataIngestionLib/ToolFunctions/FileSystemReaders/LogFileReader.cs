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
using System.Text;

using DataIngestionLib.ToolFunctions.Utils;

namespace DataIngestionLib.ToolFunctions.FileSystemReaders;

public sealed class LogFileReader
{
    private readonly IReadOnlyList<string> _allowedRoots;



    public LogFileReader(string[]? allowedRoots)
    {
        _allowedRoots = allowedRoots?.ToArray() ?? [];
    }

    public sealed class ReadResult
    {
        public required string AllowedRoot { get; init; }
        public required string FullPath { get; init; }
        public required long FileSizeBytes { get; init; }
        public required DateTime LastModifiedUtc { get; init; }
        public required string Content { get; init; }
    }

    [Description("Read the tail of a text based system log file. Use the LogFileListingTool to list available log files.")]
    public ToolResult<ReadResult> LogFileReaderTool([Description("Relative or absolute log file path constrained to the configured allowlisted roots.")] string path, [Description("Maximum number of bytes to read from the end of the file.")] int maxBytes = 65536)
    {
        try
        {
            ToolResult<ResolvedPath> resolvedPathResult = PathResolver.ResolveExistingFile(path, _allowedRoots);
            if (!resolvedPathResult.Success)
            {
                return ToolResult<ReadResult>.Fail(resolvedPathResult.Error!);
            }

            ResolvedPath resolvedPath = resolvedPathResult.Value!;
            FileInfo info = new(resolvedPath.FullPath);
            if (!info.Exists)
            {
                return ToolResult<ReadResult>.Fail($"File not found within the configured allowlisted roots: '{path}'.");
            }

            var size = info.Length;

            var boundedMaxBytes = Math.Clamp(maxBytes, 1, 262144);
            var bytesToRead = (int)Math.Min(boundedMaxBytes, size);
            var buffer = new byte[bytesToRead];

            using (FileStream fs = new(resolvedPath.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (bytesToRead > 0)
                {
                    _ = fs.Seek(-bytesToRead, SeekOrigin.End);
                    _ = fs.Read(buffer, 0, bytesToRead);
                }
            }

            ReadResult result = new()
            {
                AllowedRoot = resolvedPath.AllowedRoot,
                FullPath = info.FullName,
                FileSizeBytes = size,
                LastModifiedUtc = info.LastWriteTimeUtc,
                Content = DiagnosticsText.CleanModelText(Encoding.UTF8.GetString(buffer))
            };

            return ToolResult<ReadResult>.Ok(result);
        }
        catch (Exception ex)
        {
            return ToolResult<ReadResult>.Fail($"Log read failed: {ex.Message}");
        }
    }
}
