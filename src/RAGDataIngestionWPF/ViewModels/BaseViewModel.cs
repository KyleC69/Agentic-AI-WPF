// Build Date: 2026/03/16
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         BaseViewModel.cs
// Author: Kyle L. Crowder
// Build Num: 051909



using System.Collections.Specialized;
using System.ComponentModel;

using CommunityToolkit.Mvvm.ComponentModel;




namespace RAGDataIngestionWPF.ViewModels;





internal class BaseViewModel : ObservableObject, INotifyPropertyChanged, INotifyPropertyChanging, INotifyCollectionChanged
{
    /// <inheritdoc />
    public event NotifyCollectionChangedEventHandler CollectionChanged;
}