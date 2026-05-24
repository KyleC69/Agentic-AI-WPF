// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         BaseViewModel.cs
// Author: Kyle L. Crowder
// Build Date: 2026/05/24



using System.Collections.Specialized;
using System.ComponentModel;

using CommunityToolkit.Mvvm.ComponentModel;




namespace AgenticAIWPF.ViewModels;





internal class BaseViewModel : ObservableObject, INotifyPropertyChanged, INotifyPropertyChanging, INotifyCollectionChanged
{
    /// <inheritdoc />
    public event NotifyCollectionChangedEventHandler CollectionChanged
    {
        add { }
        remove { }
    }
}