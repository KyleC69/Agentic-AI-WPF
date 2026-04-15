#nullable enable

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

using AgenticAIWPF.Converters;
using AgenticAIWPF.Models;

namespace AgenticAIWPF.Controls;

public sealed partial class MarkdownRendererControl : UserControl
{
    public static readonly DependencyProperty MarkdownProperty = DependencyProperty.Register(
        nameof(Markdown),
        typeof(string),
        typeof(MarkdownRendererControl),
        new PropertyMetadata(string.Empty, OnMarkdownPropertyChanged));

    public static readonly DependencyProperty RenderOptionsProperty = DependencyProperty.Register(
        nameof(RenderOptions),
        typeof(MarkdownRenderOptions),
        typeof(MarkdownRendererControl),
        new PropertyMetadata(null, OnRenderOptionsChanged));

    private MarkdownRenderOptions? _subscribedOptions;

    public MarkdownRendererControl()
    {
        InitializeComponent();
        RenderOptions = new MarkdownRenderOptions();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public string Markdown
    {
        get { return (string)GetValue(MarkdownProperty); }
        set { SetValue(MarkdownProperty, value); }
    }

    public MarkdownRenderOptions RenderOptions
    {
        get { return (MarkdownRenderOptions)GetValue(RenderOptionsProperty); }
        set { SetValue(RenderOptionsProperty, value); }
    }

    private static void OnMarkdownPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((MarkdownRendererControl)d).UpdateDocument();
    }

    private static void OnRenderOptionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        MarkdownRendererControl control = (MarkdownRendererControl)d;
        control.UnsubscribeFromOptions(e.OldValue as MarkdownRenderOptions);
        control.SubscribeToOptions(e.NewValue as MarkdownRenderOptions);
        control.UpdateDocument();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        SubscribeToOptions(RenderOptions);
        UpdateDocument();
    }

    private void OnOptionsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdateDocument();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        UnsubscribeFromOptions(_subscribedOptions);
    }

    private void SubscribeToOptions(MarkdownRenderOptions? options)
    {
        if (options is null || ReferenceEquals(_subscribedOptions, options))
        {
            return;
        }

        options.PropertyChanged += OnOptionsPropertyChanged;
        _subscribedOptions = options;
    }

    private void UnsubscribeFromOptions(MarkdownRenderOptions? options)
    {
        if (options is null)
        {
            return;
        }

        options.PropertyChanged -= OnOptionsPropertyChanged;
        if (ReferenceEquals(_subscribedOptions, options))
        {
            _subscribedOptions = null;
        }
    }

    private void UpdateDocument()
    {
        if (DocumentViewer is null)
        {
            return;
        }

        DocumentViewer.Document = MarkdownToFlowDocumentConverter.ConvertToFlowDocument(Markdown, RenderOptions);
    }
}
