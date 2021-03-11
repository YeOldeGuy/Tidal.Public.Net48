using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using Prism.Regions;
using Tidal.Constants;
using Tidal.ViewModels;
using Tidal.Core.Helpers;

namespace Tidal.Views
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : MetroWindow
    {
        private readonly ShellViewModel vm;

        public ShellView(IRegionManager regionManager)
        {
            InitializeComponent();
            RegionManager.SetRegionName(shellContentControl, Regions.Main);
            RegionManager.SetRegionManager(shellContentControl, regionManager);
            vm = DataContext as ShellViewModel;
        }

        private static void MenuOpened(ContextMenu menu,
                           Func<IEnumerable<SpeedMenuSelector>> getSpeeds,
                           RoutedEventHandler handler)
        {
            if (menu == null)
                return;

            foreach (var item in menu.Items)
            {
                if (item is MenuItem menuItem)
                    menuItem.Click -= handler; // try to prevent a small leak
            }
            menu.Items.Clear();

            // I used to have an ICommand in each of the items that would call a
            // RelayCommand in the vm. Porting this to .NETCore had the effect
            // of not calling the command when the item was selected. So now, 
            // intercept the Click event, which always works, and then
            // just call the vm method to set the speed.

            foreach (var selector in getSpeeds())
            {
                MenuItem item = selector.Speed == -1
                    ? new MenuItem
                    {
                        Header = "Unlimited",
                        IsChecked = selector.Selected,
                        Tag = -1L,
                    }
                    : new MenuItem
                    {
                        Header = (selector.Speed * 1000).HumanSpeed(1),
                        IsChecked = selector.Selected,
                        Tag = selector.Speed,
                    };

                item.Click += handler;
                menu.Items.Add(item);
                if (selector.Speed == -1)
                    menu.Items.Add(new Separator());
            }
        }

        private void UpItemClick(object sender, RoutedEventArgs e)
        {
            var tag = (sender as FrameworkElement).Tag;
            vm.SetUploadSpeed((long)tag);
        }

        private void UploadMenuOpened(object sender, RoutedEventArgs e)
        {
            MenuOpened(upLimitMenu, vm.GetUploadSpeeds, UpItemClick);
        }

        private void DownItemClick(object sender, RoutedEventArgs e)
        {
            var tag = (sender as FrameworkElement).Tag;
            vm.SetDownloadSpeed((long)tag);
        }

        private void DownloadMenuOpened(object sender, RoutedEventArgs e)
        {
            MenuOpened(downLimitMenu, vm.GetDownloadSpeeds, DownItemClick);
        }
    }
}
