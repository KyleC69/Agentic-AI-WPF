using System.Windows.Controls;

using RAGDataIngestionWPF.ViewModels;

namespace RAGDataIngestionWPF.Views;

public partial class ListDetailsPage : Page
{
    public ListDetailsPage(ListDetailsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
