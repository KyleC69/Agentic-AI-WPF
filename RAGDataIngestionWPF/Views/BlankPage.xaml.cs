using System.Windows.Controls;

using RAGDataIngestionWPF.ViewModels;

namespace RAGDataIngestionWPF.Views;

public partial class BlankPage : Page
{
    public BlankPage(BlankViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
