// 2026/03/05
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF
//  File:         MainPage.xaml.cs
//   Author: Kyle L. Crowder



using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

using RAGDataIngestionWPF.ViewModels;




namespace RAGDataIngestionWPF.Views;





public partial class MainPage : Page
{
    private ScrollViewer? _messagesScrollViewer;

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _messagesScrollViewer ??= FindVisualChild<ScrollViewer>(MessagesListBox);
        MessagesListBox.ItemContainerGenerator.ItemsChanged += OnMessagesItemsChanged;
        ScrollMessagesToBottom();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        MessagesListBox.ItemContainerGenerator.ItemsChanged -= OnMessagesItemsChanged;
    }

    private void OnMessagesItemsChanged(object? sender, ItemsChangedEventArgs e)
    {
        ScrollMessagesToBottom();
    }

    private void ScrollMessagesToBottom()
    {
        if (MessagesListBox.Items.Count == 0)
        {
            return;
        }

        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
        {
            object lastItem = MessagesListBox.Items[MessagesListBox.Items.Count - 1];
            MessagesListBox.ScrollIntoView(lastItem);
            _messagesScrollViewer?.ScrollToEnd();
        }));
    }

    private static T? FindVisualChild<T>(DependencyObject parent)
        where T : DependencyObject
    {
        int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int index = 0; index < childrenCount; index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, index);
            if (child is T typedChild)
            {
                return typedChild;
            }

            T? nestedChild = FindVisualChild<T>(child);
            if (nestedChild is not null)
            {
                return nestedChild;
            }
        }

        return null;
    }
}