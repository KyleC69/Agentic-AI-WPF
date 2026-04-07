// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Core
// File:         StreamExtensions.cs
// Author: Kyle L. Crowder
// Build Num: 212927



namespace RAGDataIngestionWPF.Core.Helpers;





public static class StreamExtensions
{
    public static string ToBase64String(this Stream stream)
    {
        using MemoryStream memoryStream = new();
        stream.CopyTo(memoryStream);
        return Convert.ToBase64String(memoryStream.ToArray());
    }
}