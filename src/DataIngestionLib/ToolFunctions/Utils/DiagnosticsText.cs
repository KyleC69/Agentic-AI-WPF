// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         DiagnosticsText.cs
// Author: Kyle L. Crowder
// Build Num: 212916

namespace DataIngestionLib.ToolFunctions.Utils;

using System.Text;





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









    internal static string CleanModelText(string? value, int maxLength = 12000)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        StringBuilder builder = new(value.Length);
        var previousWasWhitespace = false;

        foreach (var character in value.Normalize(NormalizationForm.FormKC))
        {
            var sanitizedCharacter = char.IsControl(character) && character is not ('\r' or '\n' or '\t') ? ' ' : character;
            if (sanitizedCharacter == '\r')
            {
                continue;
            }

            if (char.IsWhiteSpace(sanitizedCharacter) && sanitizedCharacter != '\n')
            {
                if (previousWasWhitespace)
                {
                    continue;
                }

                builder.Append(' ');
                previousWasWhitespace = true;
                continue;
            }

            builder.Append(sanitizedCharacter);
            previousWasWhitespace = sanitizedCharacter != '\n' && char.IsWhiteSpace(sanitizedCharacter);
        }

        return Truncate(builder.ToString().Trim(), maxLength);
    }
}