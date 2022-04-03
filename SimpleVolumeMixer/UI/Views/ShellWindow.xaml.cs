using MahApps.Metro.Controls;
using Prism.Regions;
using SimpleVolumeMixer.UI.Constants;

namespace SimpleVolumeMixer.UI.Views;

public partial class ShellWindow : MetroWindow
{
    public ShellWindow(IRegionManager regionManager)
    {
        InitializeComponent();
        RegionManager.SetRegionName(HamburgerMenuContentControl, Regions.Main);
        RegionManager.SetRegionManager(HamburgerMenuContentControl, regionManager);
    }
}