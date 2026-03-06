// 2026/03/05
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF
//  File:         MenuItemTemplateSelector.cs
//   Author: Kyle L. Crowder



using System.Windows;
using System.Windows.Controls;

using MahApps.Metro.Controls;




namespace RAGDataIngestionWPF.TemplateSelectors;





public class MenuItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate GlyphDataTemplate { get; set; }

    public DataTemplate ImageDataTemplate { get; set; }








    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        return item is HamburgerMenuGlyphItem
            ? GlyphDataTemplate
            : item is HamburgerMenuImageItem ? ImageDataTemplate : base.SelectTemplate(item, container);
    }
}