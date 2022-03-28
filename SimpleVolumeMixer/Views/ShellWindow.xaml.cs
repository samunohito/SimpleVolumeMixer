using MahApps.Metro.Controls;

using Prism.Regions;

using SimpleVolumeMixer.Constants;
using SimpleVolumeMixer.Contracts.Services;

namespace SimpleVolumeMixer.Views
{
    public partial class ShellWindow : MetroWindow
    {
        public ShellWindow(IRegionManager regionManager, IRightPaneService rightPaneService)
        {
            InitializeComponent();
            RegionManager.SetRegionName(menuContentControl, Regions.Main);
            RegionManager.SetRegionManager(menuContentControl, regionManager);
            rightPaneService.Initialize(splitView, rightPaneContentControl);
        }
    }
}
