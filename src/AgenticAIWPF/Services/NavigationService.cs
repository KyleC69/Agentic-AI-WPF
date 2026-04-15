// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         NavigationService.cs
// Author: Kyle L. Crowder
// Build Num: 194533



using System.Windows.Controls;
using System.Windows.Navigation;

using AgenticAIWPF.Contracts.Services;
using AgenticAIWPF.Contracts.ViewModels;
using AgenticAIWPF.Helpers;




namespace AgenticAIWPF.Services;





public sealed class NavigationService : INavigationService
{
    private Frame _frame;
    private object _lastParameterUsed;
    private readonly IPageService _pageService;








    public NavigationService(IPageService pageService)
    {
        _pageService = pageService;
    }








    public bool CanGoBack
    {
        get { return _frame.CanGoBack; }
    }








    public void CleanNavigation()
    {
        _frame.CleanNavigation();
    }








    public void GoBack()
    {
        if (_frame.CanGoBack)
        {
            var vmBeforeNavigation = _frame.GetDataContext();
            _frame.GoBack();
            if (vmBeforeNavigation is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedFrom();
            }
        }
    }








    public void Initialize(Frame shellFrame)
    {
        if (_frame == null)
        {
            _frame = shellFrame;
            _frame.Navigated += OnNavigated;
        }
    }








    public event EventHandler<string> Navigated;








    public bool NavigateTo(string pageKey, object parameter = null, bool clearNavigation = false)
    {
        Type pageType = _pageService.GetPageType(pageKey);

        if (_frame.Content?.GetType() != pageType || (parameter != null && !parameter.Equals(_lastParameterUsed)))
        {
            _frame.Tag = clearNavigation;
            Page page = _pageService.GetPage(pageKey);
            var navigated = _frame.Navigate(page, parameter);
            if (navigated)
            {
                _lastParameterUsed = parameter;
                var dataContext = _frame.GetDataContext();
                if (dataContext is INavigationAware navigationAware)
                {
                    navigationAware.OnNavigatedFrom();
                }
            }

            return navigated;
        }

        return false;
    }








    public void UnsubscribeNavigation()
    {
        _frame.Navigated -= OnNavigated;
        _frame = null;
    }








    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        if (sender is Frame frame)
        {
            var clearNavigation = (bool)frame.Tag;
            if (clearNavigation)
            {
                frame.CleanNavigation();
            }

            var dataContext = frame.GetDataContext();
            if (dataContext is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedTo(e.ExtraData);
            }

            Navigated?.Invoke(sender, dataContext?.GetType().FullName);
        }
    }
}