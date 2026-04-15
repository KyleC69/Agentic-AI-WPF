#nullable enable

using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

using AgenticAIWPF.Controls;
using AgenticAIWPF.Models;

using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Helpers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

using MdBlock = Markdig.Syntax.Block;
using MdInline = Markdig.Syntax.Inlines.Inline;
using MdTable = Markdig.Extensions.Tables.Table;
using WpfTable = System.Windows.Documents.Table;

namespace AgenticAIWPF.Converters;

public sealed class MarkdownToFlowDocumentConverter : IValueConverter
{
    private static readonly Regex HtmlAnchorRegex = new(@"<a\b(?<attributes>[^>]*)>(?<text>.*?)</a>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex HtmlHrefRegex = new(@"href\s*=\s*(?:""(?<url>[^""]*)""|'(?<url>[^']*)'|(?<url>[^\s>]+))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return ConvertToFlowDocument(value as string, parameter as MarkdownRenderOptions);
    }

    public static FlowDocument ConvertToFlowDocument(string? markdown, MarkdownRenderOptions? options = null)
    {
        MarkdownRenderOptions renderOptions = options ?? new MarkdownRenderOptions();
        FlowDocument document = CreateDocument(renderOptions);

        if (string.IsNullOrWhiteSpace(markdown))
        {
            return document;
        }

        MarkdownDocument parsedDocument = Markdown.Parse(markdown, Pipeline);
        foreach (MdBlock block in parsedDocument)
        {
            AddBlock(document.Blocks, block, renderOptions);
        }

        return document;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }

    private static void AddBlock(BlockCollection targetBlocks, MdBlock block, MarkdownRenderOptions options)
    {
        switch (block)
        {
            case HeadingBlock headingBlock:
                targetBlocks.Add(CreateHeadingParagraph(headingBlock, options));
                break;

            case ParagraphBlock paragraphBlock:
                targetBlocks.Add(CreateParagraph(paragraphBlock.Inline, options));
                break;

            case QuoteBlock quoteBlock:
                targetBlocks.Add(CreateQuoteSection(quoteBlock, options));
                break;

            case ListBlock listBlock:
                targetBlocks.Add(CreateList(listBlock, options));
                break;

            case FencedCodeBlock fencedCodeBlock:
                targetBlocks.Add(CreateCodeBlockContainer(fencedCodeBlock, GetCodeLanguage(fencedCodeBlock.Info), options));
                break;

            case CodeBlock codeBlock:
                targetBlocks.Add(CreateCodeBlockContainer(codeBlock, string.Empty, options));
                break;

            case ThematicBreakBlock:
                targetBlocks.Add(CreateRule(options));
                break;

            case MdTable table:
                targetBlocks.Add(CreateTable(table, options));
                break;

            case ContainerBlock containerBlock:
                foreach (MdBlock childBlock in containerBlock)
                {
                    AddBlock(targetBlocks, childBlock, options);
                }
                break;

            case LeafBlock leafBlock:
                string leafText = ExtractLeafText(leafBlock);
                if (!string.IsNullOrWhiteSpace(leafText))
                {
                    targetBlocks.Add(CreatePlainParagraph(leafText, options));
                }
                break;
        }
    }

    private static void AddInline(InlineCollection targetInlines, MdInline inline, MarkdownRenderOptions options)
    {
        switch (inline)
        {
            case LiteralInline literalInline:
                targetInlines.Add(new Run(literalInline.Content.ToString()));
                break;

            case LineBreakInline lineBreakInline:
                targetInlines.Add(new LineBreak());
                if (lineBreakInline.IsHard)
                {
                    targetInlines.Add(new LineBreak());
                }
                break;

            case CodeInline codeInline:
                targetInlines.Add(new Run(codeInline.Content)
                {
                    FontFamily = new FontFamily(options.CodeFontFamilyName),
                    Background = options.InlineCodeBackgroundBrush,
                    Foreground = options.CodeForegroundBrush
                });
                break;

            case EmphasisInline emphasisInline:
                targetInlines.Add(CreateEmphasisInline(emphasisInline, options));
                break;

            case LinkInline linkInline:
                targetInlines.Add(CreateLinkInline(linkInline, options));
                break;

            case HtmlInline htmlInline:
                AddHtmlInline(targetInlines, htmlInline, options);
                break;

            case ContainerInline containerInline:
                Span span = new Span();
                AppendInlineChildren(span.Inlines, containerInline, options);
                targetInlines.Add(span);
                break;

            default:
                string fallbackText = inline.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(fallbackText))
                {
                    targetInlines.Add(new Run(fallbackText));
                }
                break;
        }
    }

    private static void AppendInlineChildren(InlineCollection targetInlines, ContainerInline? containerInline, MarkdownRenderOptions options)
    {
        if (containerInline is null)
        {
            return;
        }

        MdInline? child = containerInline.FirstChild;
        while (child is not null)
        {
            AddInline(targetInlines, child, options);
            child = child.NextSibling;
        }
    }

    private static FlowDocument CreateDocument(MarkdownRenderOptions options)
    {
        return new FlowDocument
        {
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 14,
            Foreground = options.BodyForegroundBrush,
            PagePadding = new Thickness(0),
            TextAlignment = TextAlignment.Left
        };
    }

    private static BlockUIContainer CreateCodeBlockContainer(CodeBlock codeBlock, string language, MarkdownRenderOptions options)
    {
        return new BlockUIContainer(new MarkdownCodeBlockControl
        {
            Code = ExtractLeafText(codeBlock),
            CodeLanguage = language,
            RenderOptions = options
        });
    }

    private static Span CreateEmphasisInline(EmphasisInline emphasisInline, MarkdownRenderOptions options)
    {
        Span span = new Span();
        AppendInlineChildren(span.Inlines, emphasisInline, options);

        if (emphasisInline.DelimiterChar == '~')
        {
            span.TextDecorations = TextDecorations.Strikethrough;
            return span;
        }

        if (emphasisInline.DelimiterCount >= 2)
        {
            span.FontWeight = FontWeights.Bold;
        }
        else
        {
            span.FontStyle = FontStyles.Italic;
        }

        return span;
    }

    private static Paragraph CreateHeadingParagraph(HeadingBlock headingBlock, MarkdownRenderOptions options)
    {
        Paragraph paragraph = CreateParagraph(headingBlock.Inline, options);
        paragraph.Foreground = options.HeadingForegroundBrush;
        paragraph.Margin = headingBlock.Level switch
        {
            1 => new Thickness(0, 12, 0, 6),
            2 => new Thickness(0, 10, 0, 6),
            _ => new Thickness(0, 8, 0, 4)
        };
        paragraph.FontSize = headingBlock.Level switch
        {
            1 => 24,
            2 => 20,
            3 => 18,
            _ => 16
        };
        paragraph.FontWeight = FontWeights.SemiBold;
        return paragraph;
    }

    private static void AddHtmlInline(InlineCollection targetInlines, HtmlInline htmlInline, MarkdownRenderOptions options)
    {
        string html = htmlInline.Tag ?? string.Empty;
        if (string.IsNullOrWhiteSpace(html))
        {
            return;
        }

        if (html.StartsWith("<br", StringComparison.OrdinalIgnoreCase))
        {
            targetInlines.Add(new LineBreak());
            return;
        }

        if (TryCreateHtmlAnchor(html, options, out Hyperlink? hyperlink))
        {
            targetInlines.Add(hyperlink);
            return;
        }

        if (html.StartsWith("<a ", StringComparison.OrdinalIgnoreCase) ||
            html.StartsWith("</a", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        string plainText = WebUtility.HtmlDecode(StripHtmlTags(html));
        if (!string.IsNullOrWhiteSpace(plainText))
        {
            targetInlines.Add(new Run(plainText));
            return;
        }

        string rawHtml = WebUtility.HtmlDecode(html);
        if (!string.IsNullOrWhiteSpace(rawHtml))
        {
            targetInlines.Add(new Run(rawHtml));
        }
    }

    private static string GetCodeLanguage(string? info)
    {
        string language = info ?? string.Empty;
        if (string.IsNullOrWhiteSpace(language))
        {
            return string.Empty;
        }

        string[] parts = language.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 0 ? string.Empty : parts[0];
    }

    private static string StripHtmlTags(string html)
    {
        StringBuilder builder = new StringBuilder(html.Length);
        bool insideTag = false;

        foreach (char character in html)
        {
            if (character == '<')
            {
                insideTag = true;
                continue;
            }

            if (character == '>')
            {
                insideTag = false;
                continue;
            }

            if (!insideTag)
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }

    private static bool TryCreateHtmlAnchor(string html, MarkdownRenderOptions options, out Hyperlink? hyperlink)
    {
        Match anchorMatch = HtmlAnchorRegex.Match(html);
        if (!anchorMatch.Success)
        {
            hyperlink = null;
            return false;
        }

        string anchorText = WebUtility.HtmlDecode(StripHtmlTags(anchorMatch.Groups["text"].Value));
        string attributes = anchorMatch.Groups["attributes"].Value;
        Match hrefMatch = HtmlHrefRegex.Match(attributes);
        string url = hrefMatch.Success ? WebUtility.HtmlDecode(hrefMatch.Groups["url"].Value) : string.Empty;

        hyperlink = new Hyperlink
        {
            Foreground = options.HyperlinkBrush
        };

        string visibleText = !string.IsNullOrWhiteSpace(anchorText) ? anchorText : url;
        if (!string.IsNullOrWhiteSpace(visibleText))
        {
            hyperlink.Inlines.Add(new Run(visibleText));
        }

        if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
        {
            hyperlink.NavigateUri = uri;
            hyperlink.Click += (_, _) => OpenUri(uri);
        }

        return hyperlink.Inlines.FirstInline is not null;
    }

    private static Hyperlink CreateLinkInline(LinkInline linkInline, MarkdownRenderOptions options)
    {
        Hyperlink hyperlink = new Hyperlink
        {
            Foreground = options.HyperlinkBrush
        };

        AppendInlineChildren(hyperlink.Inlines, linkInline, options);

        if (hyperlink.Inlines.FirstInline is null && !string.IsNullOrWhiteSpace(linkInline.Url))
        {
            hyperlink.Inlines.Add(new Run(linkInline.Url));
        }

        if (Uri.TryCreate(linkInline.GetDynamicUrl != null ? linkInline.GetDynamicUrl() ?? linkInline.Url : linkInline.Url, UriKind.Absolute, out Uri? uri))
        {
            hyperlink.NavigateUri = uri;
            hyperlink.Click += (_, _) => OpenUri(uri);
        }

        return hyperlink;
    }

    private static List CreateList(ListBlock listBlock, MarkdownRenderOptions options)
    {
        int startIndex = 1;
        if (listBlock.IsOrdered && !int.TryParse(listBlock.OrderedStart, out startIndex))
        {
            startIndex = 1;
        }

        List list = new List
        {
            Margin = new Thickness(0, 4, 0, 8),
            MarkerStyle = listBlock.IsOrdered ? TextMarkerStyle.Decimal : TextMarkerStyle.Disc,
            StartIndex = startIndex
        };

        foreach (ListItemBlock listItemBlock in listBlock)
        {
            ListItem item = new ListItem();
            foreach (MdBlock childBlock in listItemBlock)
            {
                AddBlock(item.Blocks, childBlock, options);
            }

            if (item.Blocks.Count == 0)
            {
                item.Blocks.Add(CreatePlainParagraph(string.Empty, options));
            }

            list.ListItems.Add(item);
        }

        return list;
    }

    private static Paragraph CreateParagraph(ContainerInline? containerInline, MarkdownRenderOptions options)
    {
        Paragraph paragraph = new Paragraph
        {
            Margin = new Thickness(0, 2, 0, 8),
            Foreground = options.BodyForegroundBrush
        };
        AppendInlineChildren(paragraph.Inlines, containerInline, options);
        return paragraph;
    }

    private static Paragraph CreatePlainParagraph(string text, MarkdownRenderOptions options)
    {
        Paragraph paragraph = new Paragraph(new Run(text))
        {
            Margin = new Thickness(0, 2, 0, 8),
            Foreground = options.BodyForegroundBrush
        };
        return paragraph;
    }

    private static QuoteBlockSection CreateQuoteSection(QuoteBlock quoteBlock, MarkdownRenderOptions options)
    {
        QuoteBlockSection quoteSection = new QuoteBlockSection
        {
            Margin = new Thickness(0, 6, 0, 10),
            Padding = new Thickness(12, 8, 10, 8),
            BorderBrush = options.BlockquoteAccentBrush,
            BorderThickness = new Thickness(4, 0, 0, 0),
            Background = options.BlockquoteBackgroundBrush
        };

        foreach (MdBlock childBlock in quoteBlock)
        {
            AddBlock(quoteSection.Blocks, childBlock, options);
        }

        return quoteSection;
    }

    private static BlockUIContainer CreateRule(MarkdownRenderOptions options)
    {
        return new BlockUIContainer(new Border
        {
            Height = 1,
            Margin = new Thickness(0, 8, 0, 10),
            Background = options.CodeSurfaceBorderBrush
        });
    }

    private static WpfTable CreateTable(MdTable table, MarkdownRenderOptions options)
    {
        WpfTable flowTable = new WpfTable
        {
            CellSpacing = 0,
            BorderBrush = options.CodeSurfaceBorderBrush,
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 6, 0, 10)
        };

        int maxColumns = table.Count == 0 ? 0 : table.OfType<Markdig.Extensions.Tables.TableRow>().Select(static row => row.Count).DefaultIfEmpty(0).Max();
        for (int index = 0; index < maxColumns; index++)
        {
            flowTable.Columns.Add(new TableColumn());
        }

        System.Windows.Documents.TableRowGroup bodyGroup = new System.Windows.Documents.TableRowGroup();
        foreach (Markdig.Extensions.Tables.TableRow row in table)
        {
            System.Windows.Documents.TableRow flowRow = new System.Windows.Documents.TableRow();
            foreach (Markdig.Extensions.Tables.TableCell cell in row)
            {
                Paragraph paragraph = new Paragraph
                {
                    Margin = new Thickness(0),
                    Foreground = options.BodyForegroundBrush
                };

                foreach (MdBlock childBlock in cell)
                {
                    if (childBlock is ParagraphBlock paragraphBlock)
                    {
                        AppendInlineChildren(paragraph.Inlines, paragraphBlock.Inline, options);
                    }
                    else
                    {
                        string fallbackText = childBlock.ToString() ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(fallbackText))
                        {
                            paragraph.Inlines.Add(new Run(fallbackText));
                        }
                    }
                }

                System.Windows.Documents.TableCell flowCell = new System.Windows.Documents.TableCell(paragraph)
                {
                    BorderBrush = options.CodeSurfaceBorderBrush,
                    BorderThickness = new Thickness(0.5),
                    Padding = new Thickness(6, 4, 6, 4),
                    Background = row.IsHeader ? options.InlineCodeBackgroundBrush : Brushes.Transparent
                };
                flowRow.Cells.Add(flowCell);
            }

            bodyGroup.Rows.Add(flowRow);
        }

        flowTable.RowGroups.Add(bodyGroup);
        return flowTable;
    }

    private static string ExtractLeafText(LeafBlock leafBlock)
    {
        if (leafBlock.Lines.Count == 0)
        {
            return string.Empty;
        }

        StringBuilder builder = new StringBuilder();
        for (int index = 0; index < leafBlock.Lines.Count; index++)
        {
            StringLine line = leafBlock.Lines.Lines[index];
            builder.Append(line.ToString());
            if (index < leafBlock.Lines.Count - 1)
            {
                builder.AppendLine();
            }
        }

        return builder.ToString();
    }

    private static void OpenUri(Uri uri)
    {
        try
        {
            Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
        }
        catch (Exception)
        {
        }
    }

    private sealed class QuoteBlockSection : Section
    {
    }
}
