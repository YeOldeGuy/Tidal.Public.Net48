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
using Tidal.Models.Messages;
using Tidal.Services.Abstract;

namespace Tidal.ViewModels
{
    class ShellViewModel : BindableBase
    {
        private readonly IRegionManager regionManager;
        private readonly IMessenger messenger;
        private IRegionNavigationService navigationService;


        public ShellViewModel(IRegionManager regionManager,
                              IMessenger messenger,
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

            this.messenger = messenger;

            messenger.Subscribe<MouseNavMessage>(OnMouseNav);
        }


        #region Navigation Methods
        private void OnMouseNav(MouseNavMessage navMsg)
        {
            switch (navMsg.Direction)
            {
                case MouseNavDirection.GoBack when navigationService.Journal.CanGoBack:
                    navigationService.Journal.GoBack();
                    break;
                case MouseNavDirection.GoForward when navigationService.Journal.CanGoForward:
                    navigationService.Journal.GoForward();
                    break;
            }
        }

        private void RequestNavigate(string target, NavigationParameters parameters = null)
        {
            if (navigationService.CanNavigate(target))
                navigationService.RequestNavigate(target, parameters);
        }


        private void OnNavigated(object sender, RegionNavigationEventArgs e)
        {
            GoBackCommand.RaiseCanExecuteChanged();
            HostsCommand.RaiseCanExecuteChanged();
            SettingsCommand.RaiseCanExecuteChanged();
        }
        #endregion

        #region ICommands
        #region ICommand Backing
        private DelegateCommand _UnloadedCommand;
        private DelegateCommand _LoadedCommand;
        private DelegateCommand _GoBackCommand;
        private DelegateCommand _HostsCommand;
        private DelegateCommand _SettingsCommand;
        #endregion

        /// <summary>
        /// Returns true if the currently displayed page is the one specified in
        /// <paramref name="pageName"/>.
        /// </summary>
        /// <param name="pageName">
        /// One of the constants in <see cref="Constants.PageKeys"/>
        /// </param>
        /// <returns></returns>
        private bool IsOnPage(string pageName)
        {
            if (navigationService is null)
                return false;
            return navigationService.Journal.CurrentEntry.Uri.OriginalString == pageName;
        }

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

        public DelegateCommand GoBackCommand =>
            _GoBackCommand = _GoBackCommand ?? new DelegateCommand(() =>
            {
                navigationService?.Journal.GoBack();
            }, 
            () => navigationService != null && navigationService.Journal.CanGoBack);

        public DelegateCommand HostsCommand =>
            _HostsCommand = _HostsCommand ?? new DelegateCommand(() =>
            {
                RequestNavigate(PageKeys.Hosts);
            }, () => !IsOnPage(PageKeys.Hosts));

        public DelegateCommand SettingsCommand =>
            _SettingsCommand = _SettingsCommand ?? new DelegateCommand(() =>
            {
                RequestNavigate(PageKeys.Settings);
            }, () => !IsOnPage(PageKeys.Settings));
        #endregion
    }
}
