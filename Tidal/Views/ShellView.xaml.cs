using MahApps.Metro.Controls;
using Prism.Regions;
using Tidal.Constants;

namespace Tidal.Views
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : MetroWindow
    {
        //private readonly ShellViewModel vm;

        public ShellView(IRegionManager regionManager)
        {
            InitializeComponent();
            RegionManager.SetRegionName(shellContentControl, Regions.Main);
            RegionManager.SetRegionManager(shellContentControl, regionManager);
            //vm = DataContext as ShellViewModel;
        }
    }
}
