// 2026/03/05
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF
//  File:         MainPage.xaml.cs
//   Author: Kyle L. Crowder



using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

using RAGDataIngestionWPF.ViewModels;




namespace RAGDataIngestionWPF.Views;





public partial class MainPage : Page
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
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

        Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
        {
            object lastItem = MessagesListBox.Items[MessagesListBox.Items.Count - 1];
            MessagesListBox.ScrollIntoView(lastItem);
        }));
    }
}