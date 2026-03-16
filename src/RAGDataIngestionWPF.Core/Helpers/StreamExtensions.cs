// Build Date: 2026/03/16
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Core
// File:         StreamExtensions.cs
// Author: Kyle L. Crowder
// Build Num: 051916



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