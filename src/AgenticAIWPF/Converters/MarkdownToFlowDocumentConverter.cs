// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         MarkdownToFlowDocumentConverter.cs
// Author: Kyle L. Crowder
// Build Num: 194529



using System.Globalization;
using System.Windows.Data;
using System.Windows.Documents;

using AgenticAIWPF.Helpers;




namespace AgenticAIWPF.Converters;





/// <summary>
///     Converts chat message text into a formatted <see cref="FlowDocument" /> so markdown output is rendered consistently
///     in the UI.
/// </summary>
public sealed class MarkdownToFlowDocumentConverter : IValueConverter
{
    /// <summary>
    ///     Converts a chat message string into a markdown-rendered <see cref="FlowDocument" /> for display in the chat
    ///     transcript.
    /// </summary>
    /// <param name="value">The raw chat message text to render.</param>
    /// <param name="targetType">The binding target type requested by WPF.</param>
    /// <param name="parameter">An optional converter parameter. This converter does not use it.</param>
    /// <param name="culture">The culture supplied by the binding engine.</param>
    /// <returns>A formatted <see cref="FlowDocument" /> for the supplied markdown text.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return MarkdownFlowDocumentFormatter.Format(value as string);
    }








    /// <summary>
    ///     Returns <see cref="Binding.DoNothing" /> because chat transcript documents are display-only.
    /// </summary>
    /// <param name="value">The current target value.</param>
    /// <param name="targetType">The source type requested by WPF.</param>
    /// <param name="parameter">An optional converter parameter. This converter does not use it.</param>
    /// <param name="culture">The culture supplied by the binding engine.</param>
    /// <returns><see cref="Binding.DoNothing" /> in all cases.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}