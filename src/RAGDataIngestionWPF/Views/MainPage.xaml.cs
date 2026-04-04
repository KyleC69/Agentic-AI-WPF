// Build Date: 2026/04/03
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         MainPage.xaml.cs
// Author: Kyle L. Crowder
// Build Num: 095223



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





    private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is DependencyObject source &&
            FindParentScrollViewer(source) is ScrollViewer scrollViewer)
        {
            e.Handled = true; // Prevent the event from being handled by child elements

            // Adjust scroll offset based on wheel delta
            scrollViewer.ScrollToVerticalOffset(
                    scrollViewer.VerticalOffset - e.Delta / 3.0);
        }
    }

    private static ScrollViewer FindParentScrollViewer(DependencyObject child)
    {
        DependencyObject parent = VisualTreeHelper.GetParent(child);

        while (parent != null)
        {
            if (parent is ScrollViewer scrollViewer)
                return scrollViewer;

            parent = VisualTreeHelper.GetParent(parent);
        }

        return null;
    }



    public MainViewModel ViewModel { get; }








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