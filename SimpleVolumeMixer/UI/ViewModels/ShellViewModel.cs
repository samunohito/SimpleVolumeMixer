using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MahApps.Metro.Controls;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using SimpleVolumeMixer.Properties;
using SimpleVolumeMixer.UI.Constants;
using SimpleVolumeMixer.UI.Views.Controls;

namespace SimpleVolumeMixer.UI.ViewModels;

// You can show pages in different ways (update main view, navigate, right pane, new windows or dialog)
// using the NavigationService, RightPaneService and WindowManagerService.
// Read more about MenuBar project type here:
// https://github.com/Microsoft/WindowsTemplateStudio/blob/release/docs/WPF/projectTypes/menubar.md
public class ShellViewModel : BindableBase
{
    private readonly IRegionManager _regionManager;
    private DelegateCommand _goBackCommand;
    private ICommand _loadedCommand;
    private ICommand _menuItemInvokedCommand;
    private IRegionNavigationService _navigationService;
    private ICommand _optionsMenuItemInvokedCommand;
    private HamburgerMenuItem _selectedMenuItem;
    private HamburgerMenuItem _selectedOptionsMenuItem;
    private ICommand _unloadedCommand;

    public ShellViewModel(IRegionManager regionManager)
    {
        _regionManager = regionManager;
    }

    public HamburgerMenuItem SelectedMenuItem
    {
        get => _selectedMenuItem;
        set => SetProperty(ref _selectedMenuItem, value);
    }

    public HamburgerMenuItem SelectedOptionsMenuItem
    {
        get => _selectedOptionsMenuItem;
        set => SetProperty(ref _selectedOptionsMenuItem, value);
    }

    // TODO WTS: Change the icons and titles for all HamburgerMenuItems here.
    public ObservableCollection<HamburgerMenuItem> MenuItems { get; } = new()
    {
        new HamburgerMenuPackIconItem { Label = Resources.ShellAudioSessionsPage, Kind = PackIconKind.VolumeHigh, Tag = PageKeys.AudioSessions },
        new HamburgerMenuPackIconItem { Label = Resources.ShellAudioDevicesPage, Kind = PackIconKind.SpeakerMultiple, Tag = PageKeys.AudioDevices },
    };

    public ObservableCollection<HamburgerMenuItem> OptionMenuItems { get; } = new()
    {
        new HamburgerMenuPackIconItem { Label = Resources.ShellSettingsPage, Kind = PackIconKind.Cog, Tag = PageKeys.Settings }
    };

    public DelegateCommand GoBackCommand =>
        _goBackCommand ?? (_goBackCommand = new DelegateCommand(OnGoBack, CanGoBack));

    public ICommand LoadedCommand => _loadedCommand ?? (_loadedCommand = new DelegateCommand(OnLoaded));

    public ICommand UnloadedCommand => _unloadedCommand ?? (_unloadedCommand = new DelegateCommand(OnUnloaded));

    public ICommand MenuItemInvokedCommand =>
        _menuItemInvokedCommand ?? (_menuItemInvokedCommand = new DelegateCommand(OnMenuItemInvoked));

    public ICommand OptionsMenuItemInvokedCommand => _optionsMenuItemInvokedCommand ??
                                                     (_optionsMenuItemInvokedCommand =
                                                         new DelegateCommand(OnOptionsMenuItemInvoked));

    private void OnLoaded()
    {
        _navigationService = _regionManager.Regions[Regions.Main].NavigationService;
        _navigationService.Navigated += OnNavigated;
        SelectedMenuItem = MenuItems.First();
    }

    private void OnUnloaded()
    {
        _navigationService.Navigated -= OnNavigated;
        _regionManager.Regions.Remove(Regions.Main);
    }

    private bool CanGoBack()
    {
        return _navigationService != null && _navigationService.Journal.CanGoBack;
    }

    private void OnGoBack()
    {
        _navigationService.Journal.GoBack();
    }

    private void OnMenuItemInvoked()
    {
        RequestNavigate(SelectedMenuItem.Tag?.ToString());
    }

    private void OnOptionsMenuItemInvoked()
    {
        RequestNavigate(SelectedOptionsMenuItem.Tag?.ToString());
    }

    private void RequestNavigate(string target)
    {
        if (_navigationService.CanNavigate(target)) _navigationService.RequestNavigate(target);
    }

    private void OnNavigated(object sender, RegionNavigationEventArgs e)
    {
        var item = MenuItems
            .OfType<HamburgerMenuItem>()
            .FirstOrDefault(i => e.Uri.ToString() == i.Tag?.ToString());
        if (item != null)
            SelectedMenuItem = item;
        else
            SelectedOptionsMenuItem = OptionMenuItems
                .OfType<HamburgerMenuItem>()
                .FirstOrDefault(i => e.Uri.ToString() == i.Tag?.ToString());

        GoBackCommand.RaiseCanExecuteChanged();
    }
}