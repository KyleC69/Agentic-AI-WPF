using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace RAGDataIngestionWPF.Helpers;

internal static class MarkdownFlowDocumentFormatter
{
    private static readonly Thickness BlockMargin = new(0, 0, 0, 8);
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    internal static FlowDocument Format(string? markdown)
    {
        FlowDocument document = CreateDocument();
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return document;
        }

        MarkdownDocument parsedMarkdown = Markdown.Parse(markdown, Pipeline);
        foreach (Markdig.Syntax.Block block in parsedMarkdown)
        {
            AddBlock(document.Blocks, block);
        }

        return document;
    }

    private static void AddBlock(BlockCollection blocks, Markdig.Syntax.Block block)
    {
        switch (block)
        {
            case HeadingBlock headingBlock:
                blocks.Add(CreateHeading(headingBlock));
                return;

            case ParagraphBlock paragraphBlock:
                blocks.Add(CreateParagraph(paragraphBlock.Inline));
                return;

            case QuoteBlock quoteBlock:
                blocks.Add(CreateQuote(quoteBlock));
                return;

            case ListBlock listBlock:
                blocks.Add(CreateList(listBlock));
                return;

            case ThematicBreakBlock:
                blocks.Add(new Paragraph(new Run("────────")) { Margin = BlockMargin });
                return;

            case FencedCodeBlock fencedCodeBlock:
                blocks.Add(CreateCodeBlock(fencedCodeBlock));
                return;

            case CodeBlock codeBlock:
                blocks.Add(CreateCodeBlock(codeBlock));
                return;

            case ContainerBlock containerBlock:
                foreach (Markdig.Syntax.Block childBlock in containerBlock)
                {
                    AddBlock(blocks, childBlock);
                }

                return;

            case LeafBlock leafBlock when leafBlock.Inline is not null:
                blocks.Add(CreateParagraph(leafBlock.Inline));
                return;
        }
    }

    private static void AddInline(InlineCollection inlines, Markdig.Syntax.Inlines.Inline inline)
    {
        switch (inline)
        {
            case LiteralInline literalInline:
                inlines.Add(new Run(literalInline.Content.ToString()));
                return;

            case LineBreakInline:
                inlines.Add(new LineBreak());
                return;

            case CodeInline codeInline:
                inlines.Add(new Span(new Run(codeInline.Content))
                {
                    FontFamily = new FontFamily("Consolas"),
                    Background = CreateCodeBackgroundBrush()
                });
                return;

            case EmphasisInline emphasisInline:
                Span emphasisSpan = new();
                AddInlines(emphasisSpan.Inlines, emphasisInline);
                ApplyEmphasis(emphasisSpan, emphasisInline);
                inlines.Add(emphasisSpan);
                return;

            case LinkInline linkInline:
                inlines.Add(CreateLink(linkInline));
                return;

            case HtmlInline htmlInline:
                inlines.Add(new Run(htmlInline.Tag));
                return;

            case HtmlEntityInline htmlEntityInline:
                inlines.Add(new Run(htmlEntityInline.Transcoded.ToString()));
                return;

            case ContainerInline containerInline:
                Span containerSpan = new();
                AddInlines(containerSpan.Inlines, containerInline);
                inlines.Add(containerSpan);
                return;
        }
    }

    private static void AddInlines(InlineCollection inlines, ContainerInline? containerInline)
    {
        if (containerInline is null)
        {
            return;
        }

        for (Markdig.Syntax.Inlines.Inline? currentInline = containerInline.FirstChild; currentInline is not null; currentInline = currentInline.NextSibling)
        {
            AddInline(inlines, currentInline);
        }
    }

    private static void ApplyEmphasis(Span span, EmphasisInline emphasisInline)
    {
        if (emphasisInline.DelimiterChar == '~')
        {
            span.TextDecorations = TextDecorations.Strikethrough;
        }

        if (emphasisInline.DelimiterCount >= 2)
        {
            span.FontWeight = FontWeights.Bold;
        }

        if (emphasisInline.DelimiterCount % 2 == 1)
        {
            span.FontStyle = FontStyles.Italic;
        }
    }

    private static Brush CreateCodeBackgroundBrush()
    {
        return new SolidColorBrush(Color.FromArgb(32, 255, 255, 255));
    }

    private static BlockUIContainer CreateCodeBlock(LeafBlock codeBlock)
    {
        return new BlockUIContainer(new Border
        {
            Padding = new Thickness(8),
            Background = CreateCodeBackgroundBrush(),
            CornerRadius = new CornerRadius(4),
            Child = new TextBlock
            {
                Text = codeBlock.Lines.ToString(),
                FontFamily = new FontFamily("Consolas"),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0)
            }
        })
        {
            Margin = BlockMargin
        };
    }

    private static FlowDocument CreateDocument()
    {
        return new FlowDocument
        {
            PagePadding = new Thickness(0),
            ColumnWidth = double.PositiveInfinity,
            Background = Brushes.Transparent
        };
    }

    private static Paragraph CreateHeading(HeadingBlock headingBlock)
    {
        Paragraph heading = CreateParagraph(headingBlock.Inline);
        heading.FontWeight = FontWeights.SemiBold;
        heading.FontSize = headingBlock.Level switch
        {
            1 => 22,
            2 => 20,
            3 => 18,
            4 => 16,
            5 => 14,
            _ => 13
        };

        return heading;
    }

    private static Hyperlink CreateLink(LinkInline linkInline)
    {
        Hyperlink hyperlink = new()
        {
            Foreground = Brushes.DeepSkyBlue,
            TextDecorations = TextDecorations.Underline,
            ToolTip = linkInline.Url
        };

        if (!string.IsNullOrWhiteSpace(linkInline.Url) && Uri.TryCreate(linkInline.Url, UriKind.RelativeOrAbsolute, out Uri? navigateUri))
        {
            hyperlink.NavigateUri = navigateUri;
        }

        AddInlines(hyperlink.Inlines, linkInline);
        if (hyperlink.Inlines.FirstInline is null)
        {
            hyperlink.Inlines.Add(new Run(linkInline.Url ?? string.Empty));
        }

        return hyperlink;
    }

    private static System.Windows.Documents.List CreateList(ListBlock listBlock)
    {
        System.Windows.Documents.List list = new()
        {
            Margin = BlockMargin,
            MarkerStyle = listBlock.IsOrdered ? TextMarkerStyle.Decimal : TextMarkerStyle.Disc
        };

        foreach (Markdig.Syntax.Block block in listBlock)
        {
            if (block is not ListItemBlock listItemBlock)
            {
                continue;
            }

            ListItem item = new();
            foreach (Markdig.Syntax.Block childBlock in listItemBlock)
            {
                AddBlock(item.Blocks, childBlock);
            }

            list.ListItems.Add(item);
        }

        return list;
    }

    private static Paragraph CreateParagraph(ContainerInline? inline)
    {
        Paragraph paragraph = new()
        {
            Margin = BlockMargin
        };

        AddInlines(paragraph.Inlines, inline);
        return paragraph;
    }

    private static Section CreateQuote(QuoteBlock quoteBlock)
    {
        Section quoteSection = new()
        {
            Margin = BlockMargin,
            Padding = new Thickness(10, 0, 0, 0),
            BorderThickness = new Thickness(3, 0, 0, 0),
            BorderBrush = new SolidColorBrush(Color.FromArgb(96, 255, 255, 255))
        };

        foreach (Markdig.Syntax.Block childBlock in quoteBlock)
        {
            AddBlock(quoteSection.Blocks, childBlock);
        }

        return quoteSection;
    }
}
