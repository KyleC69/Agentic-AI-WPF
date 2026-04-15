// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         ShellViewModel.cs
// Author: Kyle L. Crowder
// Build Num: 194544



using System.Collections.ObjectModel;
using System.Windows.Input;

using AgenticAIWPF.Contracts.Services;
using AgenticAIWPF.Properties;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MahApps.Metro.Controls;




namespace AgenticAIWPF.ViewModels;





public sealed partial class ShellViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IUserDataService _userDataService;

    [ObservableProperty] private HamburgerMenuItem selectedMenuItem;

    [ObservableProperty] private HamburgerMenuItem selectedOptionsMenuItem;








    public ShellViewModel(INavigationService navigationService, IUserDataService userDataService)
    {
        _navigationService = navigationService;
        _userDataService = userDataService;
    }








    public RelayCommand GoBackCommand
    {
        get { return field ??= new RelayCommand(OnGoBack, CanGoBack); }
    }

    public ICommand LoadedCommand
    {
        get { return field ??= new RelayCommand(OnLoaded); }
    }

    public ICommand MenuItemInvokedCommand
    {
        get { return field ??= new RelayCommand(OnMenuItemInvoked); }
    }

    // TODO: Change the icons and titles for all HamburgerMenuItems here.
    public ObservableCollection<HamburgerMenuItem> MenuItems { get; } =
    [
            new HamburgerMenuGlyphItem { Label = Resources.ShellMainPage, Glyph = "\uE804", TargetPageType = typeof(MainViewModel) },
            new HamburgerMenuGlyphItem { Label = Resources.ShellBlankPage, Glyph = "\uE82F", TargetPageType = typeof(BlankViewModel) },
            new HamburgerMenuGlyphItem { Label = Resources.ShellDataGridPage, Glyph = "\uE896", TargetPageType = typeof(DataGridViewModel) },
            new HamburgerMenuGlyphItem { Label = Resources.ShellWebViewPage, Glyph = "\uF6FA", TargetPageType = typeof(WebViewViewModel) }
    ];

    public ObservableCollection<HamburgerMenuItem> OptionMenuItems { get; } =
    [
            new HamburgerMenuGlyphItem { Label = Resources.ShellSettingsPage, Glyph = "\uE713", TargetPageType = typeof(SettingsViewModel) }
    ];

    public ICommand OptionsMenuItemInvokedCommand
    {
        get { return field ??= new RelayCommand(OnOptionsMenuItemInvoked); }
    }

    public ICommand UnloadedCommand
    {
        get { return field ??= new RelayCommand(OnUnloaded); }
    }








    private bool CanGoBack()
    {
        return _navigationService.CanGoBack;
    }








    private void NavigateTo(Type targetViewModel)
    {
        if (targetViewModel != null)
        {
            _ = _navigationService.NavigateTo(targetViewModel.FullName);
        }
    }








    private void OnGoBack()
    {
        _navigationService.GoBack();
    }








    private void OnLoaded()
    {
        _navigationService.Navigated += OnNavigated;
        _userDataService.UserDataUpdated += OnUserDataUpdated;
        UserViewModel user = _userDataService.GetUser();
        HamburgerMenuImageItem userMenuItem = new() { Thumbnail = user.Photo, Label = user.Name, Command = new RelayCommand(OnUserItemSelected) };

        OptionMenuItems.Insert(0, userMenuItem);
    }








    private void OnMenuItemInvoked()
    {
        NavigateTo(SelectedMenuItem.TargetPageType);
    }








    private void OnNavigated(object sender, string viewModelName)
    {
        HamburgerMenuItem item = MenuItems.FirstOrDefault(i => viewModelName == i.TargetPageType?.FullName);
        if (item != null)
        {
            SelectedMenuItem = item;
        }
        else
        {
            SelectedOptionsMenuItem = OptionMenuItems.FirstOrDefault(i => viewModelName == i.TargetPageType?.FullName);
        }

        GoBackCommand.NotifyCanExecuteChanged();
    }








    private void OnOptionsMenuItemInvoked()
    {
        NavigateTo(SelectedOptionsMenuItem.TargetPageType);
    }








    private void OnUnloaded()
    {
        _navigationService.Navigated -= OnNavigated;
        _userDataService.UserDataUpdated -= OnUserDataUpdated;
        HamburgerMenuImageItem userMenuItem = OptionMenuItems.OfType<HamburgerMenuImageItem>().FirstOrDefault();
        if (userMenuItem != null)
        {
            var unused = OptionMenuItems.Remove(userMenuItem);
        }
    }








    private void OnUserDataUpdated(object sender, UserViewModel user)
    {
        HamburgerMenuImageItem userMenuItem = OptionMenuItems.OfType<HamburgerMenuImageItem>().FirstOrDefault();
        if (userMenuItem != null)
        {
            userMenuItem.Label = user.Name;
            userMenuItem.Thumbnail = user.Photo;
        }
    }








    private void OnUserItemSelected()
    {
        NavigateTo(typeof(SettingsViewModel));
    }
}