using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ControlzEx.Theming;
using Humanizer;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Tidal.Client.Exceptions;
using Tidal.Client.Models;
using Tidal.Constants;
using Tidal.Core.Helpers;
using Tidal.Dialogs.ViewModels;
using Tidal.Models;
using Tidal.Models.BrokerMessages;
using Tidal.Models.Messages;
using Tidal.Services.Abstract;

namespace Tidal.ViewModels
{
    class ShellViewModel : BindableBase
    {
        private readonly IRegionManager regionManager;
        private readonly ISettingsService settingsService;
        private readonly IBrokerService brokerService;
        private readonly IMessenger messenger;
        private readonly ITaskService taskService;
        private readonly IDialogService dialogService;
        private readonly IHostService hostService;
        private IRegionNavigationService navigationService;


        public ShellViewModel(IRegionManager regionManager,
                              IMessenger messenger,
                              IBrokerService brokerService,
                              IDialogService dialogService,
                              ITaskService taskService,
                              IHostService hostService,
                              ISettingsService settingsService)
        {
            this.regionManager = regionManager;
            this.dialogService = dialogService;
            this.hostService = hostService;
            this.settingsService = settingsService;
            this.brokerService = brokerService;
            this.taskService = taskService;
            this.messenger = messenger;

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

            messenger.Subscribe<ShutdownMessage>((p) => { taskService.Stop(); brokerService.Stop(); });

            messenger.Subscribe<StartupMessage>(OnStartup);
            messenger.Subscribe<MouseNavMessage>(OnMouseNav);
        }

        private async void OnStartup(StartupMessage startupMessage)
        {
            if (startupMessage.IsFirstRun)
                await PerformStartup(startupMessage);
        }

        private async Task PerformStartup(StartupMessage startupMessage)
        {
            Host active = hostService.ActiveHost;

            if (active == null)
            {
                dialogService.ShowDialog(PageKeys.FirstHost, r =>
                {
                    if (r.Result != ButtonResult.OK)
                    {
                        // dialog explaining that we can't operate without a host
                        return;
                    }
                    active = r.Parameters.GetValue<Host>(FirstHostViewModel.HostParameter);
                    hostService.ReplaceAll(new[] { active });
                    hostService.ActiveHost = active;
                    hostService.Save();
                    settingsService.ActiveHost = active.Id;
                });
            }

            try
            {
                IsOpen = await brokerService.OpenAsync(active);
            }
            catch (ClientException)
            {
                // Something happened trying to open the host marked as active.
                // Navigate to the hosts page. The app will continue, but
                // nothing will be happening since IsOpen is false. When the
                // user puts in a new host or fixes the bad one, a
                // HostChangedMessage will be sent and things *should* start
                // working
                RequestNavigate(PageKeys.Hosts);
            }

            SetupPeriodicTasks();
            taskService.Start();
            brokerService.Start();

            if (IsOpen && startupMessage.Args != null && startupMessage.Args.Length > 0)
            {
                var arg0 = startupMessage.Args[0];
                if (MagnetUtils.IsValidMagnetUrl(arg0))
                {
                    var oldClip = Clipboard.GetText();
                    Clipboard.SetText(arg0);
                    //AddMagnetCommand.Execute();
                    Clipboard.SetText(oldClip);
                }
                else
                {
                    //DoAddTorrentDialog(arg0);
                }
            }
        }

        #region Scheduled Tasks
        private void SetupPeriodicTasks()
        {
            taskService.Add(nameof(RequestSessionTask), RequestSessionTask, TimeSpan.FromSeconds(2));
            taskService.Add(nameof(RequestStatsTask), RequestStatsTask, TimeSpan.FromSeconds(2.5));
            taskService.Add(nameof(RequestAllTorrents), RequestAllTorrents, TimeSpan.FromSeconds(3.0));

            taskService.Add(nameof(RequestGC), RequestGC, 30.Seconds());
        }

        private async Task RequestSessionTask()
        {
            if (IsOpen)
            {
                messenger.Send(new SessionRequest());
                await Task.CompletedTask;
            }
        }

        private async Task RequestStatsTask()
        {
            if (IsOpen)
            {
                messenger.Send(new SessionStatsRequest());
                await Task.CompletedTask;
            }
        }

        private async Task RequestAllTorrents()
        {
            if (IsOpen)
            {
                messenger.Send(new TorrentRequest());
                await Task.CompletedTask;
            }
        }

        private async Task RequestGC()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            await Task.CompletedTask;
        }
        #endregion


        #region Properties Visible to XAML
        #region Backing Store
        private string _Title;
        private bool _IsOpen;
        private SessionStats _SessionStats;
        private Session _Session;
        private long _FreeSpace;
        private string _AltModeLabel;
        private bool _IsAltModeEnabled;
        private string _AltModeGlyph;
        #endregion

        public string Title { get => _Title; set => SetProperty(ref _Title, value); }
        public bool IsOpen { get => _IsOpen; set => SetProperty(ref _IsOpen, value); }
        public Session Session { get => _Session; set => SetProperty(ref _Session, value); }
        public SessionStats SessionStats { get => _SessionStats; set => SetProperty(ref _SessionStats, value); }
        public long FreeSpace { get => _FreeSpace; set => SetProperty(ref _FreeSpace, value); }
        public string AltModeLabel { get => _AltModeLabel; set => SetProperty(ref _AltModeLabel, value); }
        public bool IsAltModeEnabled { get => _IsAltModeEnabled; set => SetProperty(ref _IsAltModeEnabled, value); }
        public string AltModeGlyph { get => _AltModeGlyph; set => SetProperty(ref _AltModeGlyph, value); }
        #endregion


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
