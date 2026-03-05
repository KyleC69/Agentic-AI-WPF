using System.Windows.Controls;

namespace RAGDataIngestionWPF.Contracts.Services;

public interface IPageService
{
    Type GetPageType(string key);

    Page GetPage(string key);
}
