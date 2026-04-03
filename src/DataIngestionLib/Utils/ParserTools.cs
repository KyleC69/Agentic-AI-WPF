// Build Date: 2026/04/03
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ParserTools.cs
// Author: Kyle L. Crowder
// Build Num: 095207



using System.Text;




namespace DataIngestionLib.Utils;





public class ParserTools
{

    public static string CleanRawText(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        StringBuilder sb = new(input.Length);

        foreach (var ch in input)
        {
            // Allow: tab, LF, CR, printable Unicode
            if (ch is '\t' or '\n' or '\r' or >= ' ')
            {
                _ = sb.Append(ch);
            }
            else
            {
                // Replace control chars with space
                _ = sb.Append(' ');
            }
        }

        var s = sb.ToString();

        // Normalize newline styles
        s = s.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\\r\\n", "");
        // Remove invisible Unicode
        s = s.Replace("\u00A0", " ") // non-breaking space
                .Replace("\u200B", "") // zero-width space
                .Replace("\u200C", "") // zero-width non-joiner
                .Replace("\u200D", "") // zero-width joiner
                .Replace("\u2028", "\n") // line separator
                .Replace("\u2029", "\n"); // paragraph separator

        // Collapse excessive blank lines
        while (s.Contains("\n\n\n"))
        {
            s = s.Replace("\n\n\n", "\n\n");
        }

        return s;
    }
}