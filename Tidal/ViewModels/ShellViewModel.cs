using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ControlzEx.Theming;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Tidal.Constants;
using Tidal.Services.Abstract;

namespace Tidal.ViewModels
{
    class ShellViewModel : BindableBase
    {
        private readonly IRegionManager regionManager;
        private IRegionNavigationService navigationService;


        public ShellViewModel(IRegionManager regionManager,
                              ISettingsService settingsService)
        {
            this.regionManager = regionManager;

            // Sync the theme with the system no matter the setting. This will
            // make the theme manager pick up the current color. Otherwise, the
            // app will start in the Cobalt default, which probably isn't what
            // is wanted.
            //
            // For more info on handling the theming engine, see:
            // https://github.com/ControlzEx/ControlzEx/blob/develop/Wiki/ThemeManager.md
            //
            if (settingsService.ThemeMode == ThemeMode.SystemDefault)
                ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncAll;

            ThemeManager.Current.SyncTheme();

            switch (settingsService.ThemeMode)
            {
                case ThemeMode.Light:
                    ThemeManager.Current.ChangeThemeBaseColor(Application.Current, ThemeManager.BaseColorLight);
                    ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAccent;
                    break;
                case ThemeMode.Dark:
                    ThemeManager.Current.ChangeThemeBaseColor(Application.Current, ThemeManager.BaseColorDark);
                    ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAccent;
                    break;
                default:
                    break;
            }
        }

        #region Navigation Methods
        private void RequestNavigate(string target, NavigationParameters parameters = null)
        {
            if (navigationService.CanNavigate(target))
                navigationService.RequestNavigate(target, parameters);
        }


        private void OnNavigated(object sender, RegionNavigationEventArgs e)
        {
        }
        #endregion

        #region ICommand Backing
        private DelegateCommand _UnloadedCommand;
        private DelegateCommand _LoadedCommand;
        #endregion

        public DelegateCommand LoadedCommand => 
            _LoadedCommand = _LoadedCommand ?? new DelegateCommand(() =>
            {
                navigationService = regionManager.Regions[Regions.Main].NavigationService;
                navigationService.Navigated += OnNavigated;
                RequestNavigate(PageKeys.Main);
            });


        public DelegateCommand UnloadedCommand => 
            _UnloadedCommand = _UnloadedCommand ?? new DelegateCommand(() =>
            {
                regionManager.Regions.Remove(Regions.Main);
                navigationService.Navigated -= OnNavigated;
            });
    }
}
