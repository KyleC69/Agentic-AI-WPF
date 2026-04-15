#nullable enable

using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

using AgenticAIWPF.Contracts.Services;
using AgenticAIWPF.Models;

namespace AgenticAIWPF.Controls;

public sealed partial class MarkdownCodeBlockControl : UserControl
{
    public static readonly DependencyProperty CodeProperty = DependencyProperty.Register(
        nameof(Code),
        typeof(string),
        typeof(MarkdownCodeBlockControl),
        new PropertyMetadata(string.Empty, OnCodeBlockPropertyChanged));

    public static readonly DependencyProperty CodeLanguageProperty = DependencyProperty.Register(
        nameof(CodeLanguage),
        typeof(string),
        typeof(MarkdownCodeBlockControl),
        new PropertyMetadata(string.Empty, OnCodeBlockPropertyChanged));

    public static readonly DependencyProperty RenderOptionsProperty = DependencyProperty.Register(
        nameof(RenderOptions),
        typeof(MarkdownRenderOptions),
        typeof(MarkdownCodeBlockControl),
        new PropertyMetadata(null, OnRenderOptionsChanged));

    private readonly DispatcherTimer _copyFeedbackTimer;
    private MarkdownRenderOptions? _subscribedOptions;

    public MarkdownCodeBlockControl()
    {
        InitializeComponent();
        _copyFeedbackTimer = new DispatcherTimer();
        _copyFeedbackTimer.Tick += OnCopyFeedbackTimerTick;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        RenderOptions = new MarkdownRenderOptions();
    }

    public string Code
    {
        get { return (string)GetValue(CodeProperty); }
        set { SetValue(CodeProperty, value); }
    }

    public string CodeLanguage
    {
        get { return (string)GetValue(CodeLanguageProperty); }
        set { SetValue(CodeLanguageProperty, value); }
    }

    public MarkdownRenderOptions RenderOptions
    {
        get { return (MarkdownRenderOptions)GetValue(RenderOptionsProperty); }
        set { SetValue(RenderOptionsProperty, value); }
    }

    private static void OnCodeBlockPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((MarkdownCodeBlockControl)d).ApplyState();
    }

    private static void OnRenderOptionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        MarkdownCodeBlockControl control = (MarkdownCodeBlockControl)d;
        control.UnsubscribeFromOptions(e.OldValue as MarkdownRenderOptions);
        control.SubscribeToOptions(e.NewValue as MarkdownRenderOptions);
        control.ApplyState();
    }

    private void ApplyState()
    {
        MarkdownRenderOptions options = RenderOptions ?? new MarkdownRenderOptions();

        RootBorder.Background = options.CodeSurfaceBrush;
        RootBorder.BorderBrush = options.CodeSurfaceBorderBrush;
        HeaderGrid.Background = options.InlineCodeBackgroundBrush;

        CodeTextBox.Text = Code ?? string.Empty;
        CodeTextBox.Foreground = options.CodeForegroundBrush;
        CodeTextBox.FontFamily = new FontFamily(options.CodeFontFamilyName);
        CodeTextBox.FontSize = options.CodeFontSize;

        string trimmedLanguage = string.IsNullOrWhiteSpace(CodeLanguage) ? string.Empty : CodeLanguage.Trim();
        LanguageLabel.Text = trimmedLanguage;
        LanguageLabel.Visibility = options.ShowCodeLanguage && !string.IsNullOrWhiteSpace(trimmedLanguage)
            ? Visibility.Visible
            : Visibility.Collapsed;

        CopyButton.Visibility = options.ShowCopyButton ? Visibility.Visible : Visibility.Collapsed;
        if (!_copyFeedbackTimer.IsEnabled)
        {
            CopyButton.Content = options.CopyButtonText;
        }
    }

    private void CopyButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(Code))
        {
            return;
        }

        Clipboard.SetText(Code);
        MarkdownRenderOptions options = RenderOptions ?? new MarkdownRenderOptions();
        CopyButton.Content = options.CopiedButtonText;
        _copyFeedbackTimer.Interval = options.CopyFeedbackDuration;
        _copyFeedbackTimer.Stop();
        _copyFeedbackTimer.Start();

        if (options.ShowCopyToast)
        {
            ShowCopyToast(options);
        }
    }

    private void OnCopyFeedbackTimerTick(object? sender, EventArgs e)
    {
        _copyFeedbackTimer.Stop();
        CopyButton.Content = (RenderOptions ?? new MarkdownRenderOptions()).CopyButtonText;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        SubscribeToOptions(RenderOptions);
        ApplyState();
    }

    private void OnOptionsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        ApplyState();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        UnsubscribeFromOptions(_subscribedOptions);
    }

    private void ShowCopyToast(MarkdownRenderOptions options)
    {
        if (Application.Current is not App app)
        {
            return;
        }

        IToastNotificationsService? toastService = app.Services.GetService(typeof(IToastNotificationsService)) as IToastNotificationsService;
        if (toastService is null)
        {
            return;
        }

        string codeLabel = string.IsNullOrWhiteSpace(CodeLanguage) ? "Code block" : $"{CodeLanguage} code";
        string message;
        try
        {
            message = string.Format(CultureInfo.CurrentCulture, options.CopyToastMessageFormat, codeLabel);
        }
        catch (FormatException)
        {
            message = options.CopyToastMessageFormat;
        }

        toastService.ShowToastNotification(options.CopyToastTitle, message);
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
}
