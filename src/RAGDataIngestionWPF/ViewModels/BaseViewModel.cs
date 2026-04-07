// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         BaseViewModel.cs
// Author: Kyle L. Crowder
// Build Num: 212938



using System.Collections.Specialized;
using System.ComponentModel;

using CommunityToolkit.Mvvm.ComponentModel;




namespace RAGDataIngestionWPF.ViewModels;





internal class BaseViewModel : ObservableObject, INotifyPropertyChanged, INotifyPropertyChanging, INotifyCollectionChanged
{
    /// <inheritdoc />
    public event NotifyCollectionChangedEventHandler CollectionChanged
    {
        add { }
        remove { }
    }
}