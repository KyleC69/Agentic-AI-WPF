// Build Date: 2026/03/30
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         MainPage.xaml.cs
// Author: Kyle L. Crowder
// Build Num: 233208



using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using MdXaml;

using RAGDataIngestionWPF.ViewModels;




namespace RAGDataIngestionWPF.Views;





public sealed partial class MainPage
{
    private ScrollViewer _messagesScrollViewer;








    public MainPage(MainViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = viewModel;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        ReadMarkdownAndSetViewer();
    }



    public MainViewModel ViewModel { get; private set; }




    private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        ArgumentNullException.ThrowIfNull(parent);
        var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        for (var index = 0; index < childrenCount; index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, index);
            if (child is T typedChild)
            {
                return typedChild;
            }

            T nestedChild = FindVisualChild<T>(child);
            if (nestedChild is not null)
            {
                return nestedChild;
            }
        }

        return null;
    }








    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _messagesScrollViewer ??= FindVisualChild<ScrollViewer>(MessagesListBox);
        MessagesListBox.ItemContainerGenerator.ItemsChanged += OnMessagesItemsChanged;
        ScrollMessagesToBottom();
    }








    private void OnMessagesItemsChanged(object sender, ItemsChangedEventArgs e)
    {
        ScrollMessagesToBottom();
    }








    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        MessagesListBox.ItemContainerGenerator.ItemsChanged -= OnMessagesItemsChanged;
    }








    private void ReadMarkdownAndSetViewer()
    {
        Markdown.DoText("");
    }








    private void ScrollMessagesToBottom()
    {
        if (MessagesListBox.Items.Count == 0)
        {
            return;
        }

        DispatcherOperation unused = Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
        {
            var lastItem = MessagesListBox.Items[^1];
            MessagesListBox.ScrollIntoView(lastItem);
            _messagesScrollViewer?.ScrollToEnd();
        }));
    }








    private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            SendBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
    }








    private void UIElement_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {



        // Shift+Enter → allow newline
        if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Shift)
            return;

        // Enter alone → send
        if (e.Key == Key.Enter)
        {
            e.Handled = true; // prevent newline
            ViewModel.SendMessageCommand.Execute(null);
        }



    }
}