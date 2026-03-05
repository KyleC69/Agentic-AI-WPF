using System.Windows.Controls;

using RAGDataIngestionWPF.ViewModels;

namespace RAGDataIngestionWPF.Views;

public partial class SettingsPage : Page
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
