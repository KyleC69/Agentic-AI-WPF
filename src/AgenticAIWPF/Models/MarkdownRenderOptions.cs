#nullable enable

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace AgenticAIWPF.Models;

public sealed class MarkdownRenderOptions : INotifyPropertyChanged
{
    private Brush _blockquoteAccentBrush = new SolidColorBrush(Color.FromRgb(145, 110, 67));
    private Brush _blockquoteBackgroundBrush = new SolidColorBrush(Color.FromRgb(247, 241, 232));
    private Brush _bodyForegroundBrush = Brushes.DarkSlateGray;
    private string _codeFontFamilyName = "Cascadia Code";
    private double _codeFontSize = 13;
    private Brush _codeForegroundBrush = new SolidColorBrush(Color.FromRgb(39, 51, 64));
    private Brush _inlineCodeBackgroundBrush = new SolidColorBrush(Color.FromRgb(236, 226, 212));
    private Brush _codeSurfaceBrush = new SolidColorBrush(Color.FromRgb(249, 246, 240));
    private Brush _codeSurfaceBorderBrush = new SolidColorBrush(Color.FromRgb(191, 165, 127));
    private TimeSpan _copyFeedbackDuration = TimeSpan.FromSeconds(2);
    private string _copyToastMessageFormat = "{0} sent to clipboard.";
    private string _copyToastTitle = "Clipboard Updated";
    private string _copiedButtonText = "Copied";
    private string _copyButtonText = "Copy";
    private Brush _headingForegroundBrush = new SolidColorBrush(Color.FromRgb(44, 53, 66));
    private Brush _hyperlinkBrush = new SolidColorBrush(Color.FromRgb(27, 91, 153));
    private bool _showCopyToast;
    private bool _showCodeLanguage = true;
    private bool _showCopyButton = true;

    public event PropertyChangedEventHandler? PropertyChanged;

    public Brush BlockquoteAccentBrush
    {
        get { return _blockquoteAccentBrush; }
        set { SetProperty(ref _blockquoteAccentBrush, value); }
    }

    public Brush BlockquoteBackgroundBrush
    {
        get { return _blockquoteBackgroundBrush; }
        set { SetProperty(ref _blockquoteBackgroundBrush, value); }
    }

    public Brush BodyForegroundBrush
    {
        get { return _bodyForegroundBrush; }
        set { SetProperty(ref _bodyForegroundBrush, value); }
    }

    public string CodeFontFamilyName
    {
        get { return _codeFontFamilyName; }
        set { SetProperty(ref _codeFontFamilyName, value); }
    }

    public double CodeFontSize
    {
        get { return _codeFontSize; }
        set { SetProperty(ref _codeFontSize, value); }
    }

    public Brush CodeForegroundBrush
    {
        get { return _codeForegroundBrush; }
        set { SetProperty(ref _codeForegroundBrush, value); }
    }

    public Brush CodeSurfaceBorderBrush
    {
        get { return _codeSurfaceBorderBrush; }
        set { SetProperty(ref _codeSurfaceBorderBrush, value); }
    }

    public Brush CodeSurfaceBrush
    {
        get { return _codeSurfaceBrush; }
        set { SetProperty(ref _codeSurfaceBrush, value); }
    }

    public TimeSpan CopyFeedbackDuration
    {
        get { return _copyFeedbackDuration; }
        set { SetProperty(ref _copyFeedbackDuration, value); }
    }

    public string CopiedButtonText
    {
        get { return _copiedButtonText; }
        set { SetProperty(ref _copiedButtonText, value); }
    }

    public string CopyToastMessageFormat
    {
        get { return _copyToastMessageFormat; }
        set { SetProperty(ref _copyToastMessageFormat, value); }
    }

    public string CopyToastTitle
    {
        get { return _copyToastTitle; }
        set { SetProperty(ref _copyToastTitle, value); }
    }

    public string CopyButtonText
    {
        get { return _copyButtonText; }
        set { SetProperty(ref _copyButtonText, value); }
    }

    public Brush HeadingForegroundBrush
    {
        get { return _headingForegroundBrush; }
        set { SetProperty(ref _headingForegroundBrush, value); }
    }

    public Brush HyperlinkBrush
    {
        get { return _hyperlinkBrush; }
        set { SetProperty(ref _hyperlinkBrush, value); }
    }

    public Brush InlineCodeBackgroundBrush
    {
        get { return _inlineCodeBackgroundBrush; }
        set { SetProperty(ref _inlineCodeBackgroundBrush, value); }
    }

    public bool ShowCodeLanguage
    {
        get { return _showCodeLanguage; }
        set { SetProperty(ref _showCodeLanguage, value); }
    }

    public bool ShowCopyToast
    {
        get { return _showCopyToast; }
        set { SetProperty(ref _showCopyToast, value); }
    }

    public bool ShowCopyButton
    {
        get { return _showCopyButton; }
        set { SetProperty(ref _showCopyButton, value); }
    }

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
