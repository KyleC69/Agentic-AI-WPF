// 2026/03/05
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF
//  File:         FrameExtensions.cs
//   Author: Kyle L. Crowder



using System.Windows;
using System.Windows.Controls;

namespace RAGDataIngestionWPF.Helpers;





public static class FrameExtensions
{

    public static void CleanNavigation(this Frame frame)
    {
        while (frame.CanGoBack)
        {
            frame.RemoveBackEntry();
        }
    }








    public static object GetDataContext(this Frame frame)
    {
        return frame.Content is FrameworkElement element ? element.DataContext : null;

    }
}