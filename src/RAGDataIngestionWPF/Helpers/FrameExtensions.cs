// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         FrameExtensions.cs
// Author: Kyle L. Crowder
// Build Num: 212931



using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;




namespace RAGDataIngestionWPF.Helpers;





public static class FrameExtensions
{

    public static void CleanNavigation([NotNull] this Frame frame)
    {
        while (frame.CanGoBack)
        {
            _ = frame.RemoveBackEntry();
        }
    }








    [return: MaybeNull]
    public static object GetDataContext([NotNull] this Frame frame)
    {
        return frame.Content is FrameworkElement element ? element.DataContext : null;

    }
}