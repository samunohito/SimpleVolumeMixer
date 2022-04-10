using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MahApps.Metro.Controls;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using SimpleVolumeMixer.Properties;
using SimpleVolumeMixer.UI.Constants;
using SimpleVolumeMixer.UI.Views.Controls;

namespace SimpleVolumeMixer.UI.ViewModels;

public class ShellWindowViewModel : BindableBase
{
    private readonly IRegionManager _regionManager;
    private IRegionNavigationService? _navigationService;

    public ShellWindowViewModel(IRegionManager regionManager)
    {
        _regionManager = regionManager;
        SelectedMenuItem = new ReactivePropertySlim<HamburgerMenuItem?>();
        SelectedOptionsMenuItem = new ReactivePropertySlim<HamburgerMenuItem?>();
        LoadedCommand = new DelegateCommand(OnLoaded);
        UnloadedCommand = new DelegateCommand(OnUnloaded);
        MenuItemInvokedCommand = new DelegateCommand(OnMenuItemInvoked);
        OptionsMenuItemInvokedCommand = new DelegateCommand(OnOptionsMenuItemInvoked);
    }

    public ObservableCollection<HamburgerMenuItem> MenuItems { get; } = new()
    {
        new HamburgerMenuPackIconItem
        {
            Label = Resources.ShellAudioSessionsPage,
            Kind = PackIconKind.VolumeHigh,
            Tag = PageKeys.AudioSessions
        },
        new HamburgerMenuPackIconItem
        {
            Label = Resources.ShellAudioDevicesPage,
            Kind = PackIconKind.SpeakerMultiple,
            Tag = PageKeys.AudioDevices
        },
    };

    public ObservableCollection<HamburgerMenuItem> OptionMenuItems { get; } = new()
    {
        new HamburgerMenuPackIconItem
        {
            Label = Resources.ShellSettingsPage,
            Kind = PackIconKind.Cog,
            Tag = PageKeys.Settings
        }
    };

    public IReactiveProperty<HamburgerMenuItem?> SelectedMenuItem { get; }
    public IReactiveProperty<HamburgerMenuItem?> SelectedOptionsMenuItem { get; }

    public ICommand LoadedCommand { get; }
    public ICommand UnloadedCommand { get; }
    public ICommand MenuItemInvokedCommand { get; }
    public ICommand OptionsMenuItemInvokedCommand { get; }

    private void OnLoaded()
    {
        _navigationService = _regionManager.Regions[Regions.Main].NavigationService;
        _navigationService.Navigated += OnNavigated;
        SelectedMenuItem.Value = MenuItems.First();
    }

    private void OnUnloaded()
    {
        if (_navigationService != null)
        {
            _navigationService.Navigated -= OnNavigated;
        }

        _regionManager.Regions.Remove(Regions.Main);
    }

    private void OnMenuItemInvoked()
    {
        RequestNavigate(SelectedMenuItem.Value?.Tag?.ToString());
    }

    private void OnOptionsMenuItemInvoked()
    {
        RequestNavigate(SelectedOptionsMenuItem.Value?.Tag?.ToString());
    }

    private void RequestNavigate(string? target)
    {
        if (target != null && _navigationService?.CanNavigate(target) == true)
        {
            _navigationService.RequestNavigate(target);
        }
    }

    private void OnNavigated(object sender, RegionNavigationEventArgs e)
    {
        var item = Enumerable
            .OfType<HamburgerMenuItem>(MenuItems)
            .FirstOrDefault(i => e.Uri.ToString() == i.Tag?.ToString());
        if (item != null)
        {
            SelectedMenuItem.Value = item;
        }
        else
        {
            SelectedOptionsMenuItem.Value = Enumerable
                .OfType<HamburgerMenuItem>(OptionMenuItems)
                .FirstOrDefault(i => e.Uri.ToString() == i.Tag?.ToString());
        }
    }
}