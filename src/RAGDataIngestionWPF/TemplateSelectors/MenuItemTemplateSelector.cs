// Build Date: 2026/03/30
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         MenuItemTemplateSelector.cs
// Author: Kyle L. Crowder
// Build Num: 233201



using System.Windows;
using System.Windows.Controls;

using MahApps.Metro.Controls;




namespace RAGDataIngestionWPF.TemplateSelectors;





public sealed class MenuItemTemplateSelector : DataTemplateSelector
{

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        return item is HamburgerMenuGlyphItem ? GlyphDataTemplate : item is HamburgerMenuImageItem ? ImageDataTemplate : base.SelectTemplate(item, container);
    }








    public DataTemplate GlyphDataTemplate { get; init; }

    public DataTemplate ImageDataTemplate { get; init; }
}