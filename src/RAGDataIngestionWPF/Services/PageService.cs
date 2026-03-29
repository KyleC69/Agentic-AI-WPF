// Build Date: 2026/03/29
// Solution: File
// Project:   RAGDataIngestionWPF
// File:         PageService.cs
// Author: Kyle L. Crowder
// Build Num: 051955



using System.Windows.Controls;

using CommunityToolkit.Mvvm.ComponentModel;

using RAGDataIngestionWPF.Contracts.Services;
using RAGDataIngestionWPF.ViewModels;
using RAGDataIngestionWPF.Views;




namespace RAGDataIngestionWPF.Services;





public sealed class PageService : IPageService
{
    private readonly Dictionary<string, Type> _pages = [];
    private readonly IServiceProvider _serviceProvider;








    public PageService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Configure<MainViewModel, MainPage>();
        Configure<BlankViewModel, BlankPage>();
        Configure<ListDetailsViewModel, ListDetailsPage>();
        Configure<DataGridViewModel, DataGridPage>();
        Configure<WebViewViewModel, WebViewPage>();
        Configure<SettingsViewModel, SettingsPage>();
    }








    public Type GetPageType(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        Type pageType;
        lock (_pages)
        {
            if (!_pages.TryGetValue(key, out pageType))
            {
                throw new ArgumentException($"Page not found: {key}. Did you forget to call PageService.Configure?");
            }
        }

        return pageType;
    }








    public Page GetPage(string key)
    {
        Type pageType = GetPageType(key);
        Page page = _serviceProvider.GetService(pageType) as Page;
        return page ?? throw new InvalidOperationException($"Page service could not resolve page type {pageType.FullName}.");
    }








    private void Configure<TVm, TV>() where TVm : ObservableObject where TV : Page
    {
        lock (_pages)
        {
            var key = typeof(TVm).FullName ?? typeof(TVm).Name;
            if (_pages.ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} is already configured in PageService");
            }

            Type type = typeof(TV);
            if (_pages.Any(p => p.Value == type))
            {
                throw new ArgumentException($"This type is already configured with key {_pages.First(p => p.Value == type).Key}");
            }

            _pages.Add(key, type);
        }
    }
}