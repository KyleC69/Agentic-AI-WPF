// Build Date: 2026/03/29
// Solution: File
// Project:   DataIngestionLib
// File:         DiagnosticsText.cs
// Author: Kyle L. Crowder
// Build Num: 051941



namespace DataIngestionLib.ToolFunctions;





internal static class DiagnosticsText
{
    internal static string JoinBounded(IEnumerable<string?> values, int maxItems = 8, int maxLength = 256)
    {
        ArgumentNullException.ThrowIfNull(values);

        var filtered = values.Where(value => !string.IsNullOrWhiteSpace(value)).Take(maxItems).Select(value => value!.Trim());

        return Truncate(string.Join("; ", filtered), maxLength);
    }








    internal static string Truncate(string? value, int maxLength = 256)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength] + "...";
    }
}