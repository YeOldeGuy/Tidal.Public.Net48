using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ControlzEx.Theming;
using Humanizer;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Tidal.Client.Exceptions;
using Tidal.Client.Models;
using Tidal.Constants;
using Tidal.Core.Helpers;
using Tidal.Dialogs.ViewModels;
using Tidal.Helpers;
using Tidal.Models;
using Tidal.Models.BrokerMessages;
using Tidal.Models.Messages;
using Tidal.Properties;
using Tidal.Services.Abstract;

namespace Tidal.ViewModels
{
    internal class ShellViewModel : BindableBase
    {
        private readonly IRegionManager regionManager;
        private readonly ISettingsService settingsService;
        private readonly IBrokerService brokerService;
        private readonly IMessenger messenger;
        private readonly ITaskService taskService;
        private readonly IDialogService dialogService;
        private readonly IHostService hostService;
        private readonly ITorrentStatusService torrentStatusService;
        private readonly INotificationService notificationService;
        private readonly InfoDisplayStack displayStack;
        private IRegionNavigationService navigationService;
        private readonly SynchronizationContext context;
        private bool inFatality;

        public ShellViewModel(IRegionManager regionManager,
                              IMessenger messenger,
                              IBrokerService brokerService,
                              IDialogService dialogService,
                              ITaskService taskService,
                              IHostService hostService,
                              INotificationService notificationService,
                              ITorrentStatusService torrentStatusService,
                              ISettingsService settingsService)
        {
            this.regionManager = regionManager;
            this.dialogService = dialogService;
            this.hostService = hostService;
            this.settingsService = settingsService;
            this.brokerService = brokerService;
            this.taskService = taskService;
            this.torrentStatusService = torrentStatusService;
            this.notificationService = notificationService;
            this.messenger = messenger;

            context = SynchronizationContext.Current;
            displayStack = new InfoDisplayStack((s) => UiInvoke(() => StatusInfo = s));

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

            messenger.Subscribe<ShutdownMessage>((_) => { taskService.Stop(); brokerService.Stop(); });

            // Misc subscriptions
            messenger.Subscribe<StartupMessage>(OnStartup);
            messenger.Subscribe<MouseNavMessage>(OnMouseNav);
            messenger.Subscribe<HostChangedMessage>(OnHostChanged);

            // Error subscriptions
            messenger.Subscribe<FatalMessage>(OnFatalError);
            messenger.Subscribe<WarningMessage>(OnWarning);
            messenger.Subscribe<InfoMessage>(OnInfo);
            messenger.Subscribe<StatusInfoMessage>(OnStatusInfo);

            // Client (via the Broker) subscriptions
            messenger.Subscribe<SessionResponse>(OnSession);
            messenger.Subscribe<SessionStatsResponse>(OnSessionStats);
            messenger.Subscribe<TorrentResponse>(OnTorrents);
            messenger.Subscribe<FreeSpaceResponse>(OnFreeSpace);
            messenger.Subscribe<AddTorrentResponse>(OnTorrentAdded);
        }

        private void UiInvoke(Action action)
        {
            context.Post(_ => action.Invoke(), null);
        }

        #region Startup Stuff
        private async void OnStartup(StartupMessage startupMessage)
        {
            if (startupMessage.IsFirstRun)
                await PerformStartup(startupMessage);
            else
                ProcessCommandLine(startupMessage);
        }

        private void ProcessCommandLine(StartupMessage startupMessage)
        {
            // Okay, here's the thing. I've figured out that unless you actually
            // use the command line, you know, like type the command in, Windows
            // handles the activation of your app rather oddly in the case where
            // you've selected say five torrents, then right clicked "Open" in
            // the shell.
            //
            // At this point, Windows will simply activate your app five times,
            // each with a single argument on the "command line". Also, there's
            // a built-in limit (15, I think) items that will be allowed; beyond
            // that limit, I'm not sure. Does the shell simply drop the ones in
            // excess, or skip it altogether? ¯\_(ツ)_/¯
            //
            // I guess I can understand Windows trying to keep someone from
            // selecting 200 files, then clicking "open," bringing the system to
            // its knees, but it is weird. I always figured that the shell would
            // simply build a long command line and pass that to the app. That
            // there might be one duplicate activation, but not 15. Like I said,
            // weird.
            //
            // So anyway, here's Wonderwall.
            //
            if (startupMessage.Args != null && startupMessage.Args.Length > 0)
            {
                string arg0 = startupMessage.Args[0];
                if (MagnetUtils.IsValidMagnetUrl(arg0))
                {
                    var oldClip = Clipboard.GetText();
                    Clipboard.SetText(arg0);
                    AddMagnetCommand.Execute();
                    Clipboard.SetText(oldClip);
                }
                else
                {
                    DoAddTorrentDialog(startupMessage.Args);
                }
            }
        }

        private async Task PerformStartup(StartupMessage startupMessage)
        {
            Host active = hostService.ActiveHost;

            if (active is null)
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
                ProcessCommandLine(startupMessage);
            }
        }
        #endregion Startup Stuff

        #region Scheduled Tasks
        //
        // These methods are called by the TaskService. All they do is send
        // requests to the client via the BrokerService to fetch various
        // data.
        //
        private void SetupPeriodicTasks()
        {
            taskService.Add(nameof(RequestSessionTask), RequestSessionTask, TimeSpan.FromSeconds(2.9));
            taskService.Add(nameof(RequestStatsTask), RequestStatsTask, TimeSpan.FromSeconds(3.1));
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
        #endregion Scheduled Tasks

        #region Client Subscriptions
        //
        // All of these methods are called in response to subscription
        // notification. Recall that all of these subscriptions are invoked in
        // the UI thread, so they don't need a UI invocation wrapper.
        //
        private void OnSession(SessionResponse sessionResponse)
        {
            Session = Session ?? new Session();
            Session.Assign(sessionResponse.Session);
            messenger.Send(new FreeSpaceRequest(Session.DownloadDirectory));
            SetAltLabel(Session);
        }

        private void OnSessionStats(SessionStatsResponse statsResponse)
        {
            SessionStats = SessionStats ?? new SessionStats();
            SessionStats.Assign(statsResponse.SessionStats);
            SetTitle(SessionStats);
        }

        private void OnTorrents(TorrentResponse torrentResponse)
        {
            torrentStatusService.CheckStatus(torrentResponse.Torrents);
            torrentStatusService.CheckForConnection(torrentResponse.Torrents);
        }

        private void OnFreeSpace(FreeSpaceResponse freeSpaceResponse)
        {
            FreeSpace = freeSpaceResponse.FreeSpace;
        }

        private void OnTorrentAdded(AddTorrentResponse response)
        {
            if (response.IsDuplicate)
            {
                var msg = string.Format(Resources.TorrentExists_1, response.Name);
                var hdr = Resources.TorrentExists;
                notificationService.ShowInfo(msg, hdr, 10.Seconds());
            }
        }
        #endregion Client Subscriptions

        #region Error Subscriptions and handling
        //
        // Methods that are called in response to error handling subscriptions.
        // Some errors are minor, nothing more than an advisory that a torrent
        // file couldn't be parsed. Others are more serious, like a lack of comm
        // to the client.
        //
        private async void RestartAfterFailure()
        {
            inFatality = false;

            IsOpen = await brokerService.OpenAsync(hostService.ActiveHost);
            messenger.Send(new ResumeMessage());
        }

        private void OnFatalError(FatalMessage fatalMessage)
        {
            if (inFatality)
                return;

            inFatality = true;
            messenger.Send(new HaltMessage());

            IsOpen = false;

            // These assignments will force the display to zero out:
            SessionStats.AverageUploadSpeed = SessionStats.UploadSpeed = 0;
            SessionStats.AverageDownloadSpeed = SessionStats.DownloadSpeed = 0;
            Session.Version = Resources.ShellVM_UnknownHost;
            FreeSpace = 0;

            notificationService.ShowRetryCancel(Resources.ShellVM_Fatality,
                                                fatalMessage.Message,
                                                fatalMessage.Header,
                                                retryAction: () => RestartAfterFailure(),
                                                cancelAction: () => inFatality = false);
        }

        private void OnError(ErrorMessage errorMessage)
        {
            OnFatalError(new FatalMessage(errorMessage.Message, errorMessage.Header));
        }

        private void OnWarning(WarningMessage warningMessage)
        {
            notificationService.ShowWarning(warningMessage.Message, warningMessage.Header);
        }

        private void OnInfo(InfoMessage infoMessage)
        {
            notificationService.ShowInfo(infoMessage.Message, infoMessage.Header, infoMessage.Timeout);
        }

        private void OnStatusInfo(StatusInfoMessage info)
        {
            displayStack.AddMessage(info.Message, info.Timeout);
        }

        #endregion Error Subscriptions and handling

        #region Other Subscriptions
        private async void OnHostChanged(HostChangedMessage hostChangedMessage)
        {
            var host = hostService.GetHost(hostChangedMessage.ActiveId);
            if (host != null)
                IsOpen = await brokerService.OpenAsync(host);
        }

        #endregion Other Subscriptions

        #region Properties Visible to XAML
        #region Backing Store
        private string _Title = "Tidal";
        private bool _IsOpen;
        private SessionStats _SessionStats;
        private Session _Session;
        private long _FreeSpace;
        private string _AltModeLabel = "no connection";
        private bool _IsAltModeEnabled;
        private string _AltModeGlyph;
        private bool _CanGoBack;
        private string _StatusInfo;
        #endregion Backing Store

        public string Title
        {
            get => _Title;
            set => SetProperty(ref _Title, value);
        }

        public bool IsOpen
        {
            get => _IsOpen;
            set => SetProperty(ref _IsOpen, value);
        }

        public Session Session
        {
            get => _Session;
            set => SetProperty(ref _Session, value);
        }

        public SessionStats SessionStats
        {
            get => _SessionStats;
            set => SetProperty(ref _SessionStats, value);
        }

        public long FreeSpace
        {
            get => _FreeSpace;
            set => SetProperty(ref _FreeSpace, value);
        }

        public string AltModeLabel
        {
            get => _AltModeLabel;
            set => SetProperty(ref _AltModeLabel, value);
        }

        public bool IsAltModeEnabled
        {
            get => _IsAltModeEnabled;
            set => SetProperty(ref _IsAltModeEnabled, value);
        }

        public string AltModeGlyph
        {
            get => _AltModeGlyph;
            set => SetProperty(ref _AltModeGlyph, value);
        }

        public bool CanGoBack
        {
            get => _CanGoBack;
            set => SetProperty(ref _CanGoBack, value);
        }

        public string StatusInfo
        {
            get => _StatusInfo;
            set => SetProperty(ref _StatusInfo, value);
        }

        public string AssemblyVersion
        {
            get
            {
                Assembly assembly = typeof(ShellViewModel).Assembly;
                AssemblyName assemblyName = assembly.GetName();
                Version version = assemblyName.Version;

                return version.ToString();
            }
        }
        #endregion Properties Visible to XAML

        #region Helpers for Properties
        private void SetTitle(SessionStats stats)
        {
            if (stats == null)
            {
                Title = Resources.ShellViewMode_NoHostTitle;
                return;
            }

            long actual = stats.UploadSpeed + stats.DownloadSpeed;
            long avgUp = stats.AverageUploadSpeed;
            long avgDn = stats.AverageDownloadSpeed;

            // Need to short circuit the display of up/down speed. We show the
            // average, but that means that occasionally, the title will show an
            // up/down speed when there aren't any peers. That's a little bit
            // weird. It's like, *who* am I uploading at 15Kbps to? So, if the
            // actual up/down speed are both zero, go back to the title.

            Title = actual != 0 && avgUp + avgDn > 0
                ? string.Format("▲{0} ▼{1}", avgUp.HumanSpeed(), avgDn.HumanSpeed())
                : $"{Resources.ShellViewMode_NormalTitle}-{hostService.ActiveHost.Name}";
        }

        private void SetAltLabel(Session session)
        {
            if (session is null || !IsOpen)
            {
                AltModeLabel = Resources.ShellNoConnection;
                IsAltModeEnabled = false;
                AltModeGlyph = MDLConsts.LostComm;
            }
            else if (session.AltSpeedEnabled)
            {
                AltModeGlyph = MDLConsts.Play;
                long upLimit = session.AltSpeedUp * 1000;
                long downLimit = session.AltSpeedDown * 1000;
                AltModeLabel = string.Format("{0}/{1}", upLimit.HumanSpeed(1), downLimit.HumanSpeed(1));
                IsAltModeEnabled = true;
            }
            else
            {
                AltModeGlyph = MDLConsts.FastForward;
                string up = Resources.ShellUnlimited;
                string dn = Resources.ShellUnlimited;
                if (session.SpeedLimitUpEnabled)
                    up = (session.SpeedLimitUp * 1000).HumanSpeed(session.SpeedLimitUp > 1000 ? 2 : 0);

                if (session.SpeedLimitDownEnabled)
                    dn = (session.SpeedLimitDown * 1000).HumanSpeed(session.SpeedLimitDown > 1000 ? 2 : 0);

                AltModeLabel = string.Format("{0}/{1}", up, dn);
                IsAltModeEnabled = false;
            }
        }

        private void AltModePause()
        {
            AltModeGlyph = MDLConsts.Updating;
            AltModeLabel = Resources.ShellStandby;
        }
        #endregion Helpers for Properties

        #region upload speed limit menu stuff
        internal long UploadSpeedLimit => Session.SpeedLimitUpEnabled == false ? -1 : Session.SpeedLimitUp;

        private IEnumerable<long> PresetUploadSpeeds
        {
            get
            {
                foreach (string val in settingsService.UploadPresets.Split(','))
                {
                    if (long.TryParse(val, out var speed))
                        yield return speed;
                }
            }
        }

        private long selectedUpSpeed = 375;

        /// <summary>
        ///   Gets the preset upload speeds to display in a menu along with a
        ///   flag indicating whether the speed is currently selected.
        /// </summary>
        /// <returns>
        ///   An enumeration of tuples, each of a preset speed and a flag
        ///   indicating if that speed is selected.
        /// </returns>
        internal IEnumerable<SpeedMenuSelector> GetUploadSpeeds()
        {
            selectedUpSpeed = UploadSpeedLimit;

            // The -1 value is used as a flag to denote "unlimited"
            yield return new SpeedMenuSelector(-1, selectedUpSpeed == -1);

            var speeds = PresetUploadSpeeds.ToList();

            // There is a possibility that the Upload speed might have been set
            // to something odd, like 433, over in the settings, which is why we
            // append the currently set upload speed limit, then blend it into
            // the menu. Of course, it is most likely one of the same values as
            // the presets, which is why there's a Distinct() filter at the end
            // of the Linq statement.

            if (!speeds.Contains(selectedUpSpeed) && selectedUpSpeed != -1)
            {
                speeds.Add(selectedUpSpeed);
            }

            foreach (var speed in speeds.OrderByDescending(x => x).Distinct())
            {
                yield return new SpeedMenuSelector(speed, speed == selectedUpSpeed);
            }
        }

        internal void SetUploadSpeed(long bps)
        {
            var session = new SessionMutator { AltSpeedEnabled = false };
            if (bps == -1)
            {
                session.SpeedLimitUpEnabled = false;
            }
            else
            {
                session.SpeedLimitUpEnabled = true;
                session.SpeedLimitUp = bps;
            }
            selectedUpSpeed = bps;

            AltModePause();
            messenger.Send(new SetSessionRequest(session));
        }
        #endregion upload speed limit menu stuff

        #region download speed limit menu stuff
        internal long DownloadSpeedLimit => Session.SpeedLimitDownEnabled == false ? -1 : Session.SpeedLimitDown;

        private IEnumerable<long> PresetDownloadSpeeds
        {
            get
            {
                foreach (string val in settingsService.DownloadPresets.Split(new[] { ',' }))
                {
                    if (long.TryParse(val, out var speed))
                        yield return speed;
                }
            }
        }

        private long selectedDownSpeed = 1000;

        /// <summary>
        ///   Gets the preset download speeds to display in a menu along with a
        ///   flag indicating whether the speed is currently selected.
        /// </summary>
        /// <returns>
        ///   An enumeration of tuples, each of a preset speed and a flag
        ///   indicating if that speed is selected.
        /// </returns>
        internal IEnumerable<SpeedMenuSelector> GetDownloadSpeeds()
        {
            selectedDownSpeed = DownloadSpeedLimit;

            // The -1 value is used as a flag to denote "unlimited"
            yield return new SpeedMenuSelector(-1, selectedDownSpeed == -1);

            var speeds = PresetDownloadSpeeds.ToList();

            if (!speeds.Contains(selectedDownSpeed) && selectedDownSpeed != -1)
            {
                speeds.Add(selectedDownSpeed);
            }

            foreach (var speed in speeds.OrderByDescending(x => x).Distinct())
            {
                yield return new SpeedMenuSelector(speed, speed == selectedDownSpeed);
            }
        }

        internal void SetDownloadSpeed(long bps)
        {
            var session = new SessionMutator { AltSpeedEnabled = false };
            if (bps == -1)
                session.SpeedLimitDownEnabled = false;
            else
            {
                session.SpeedLimitDownEnabled = true;
                session.SpeedLimitDown = bps;
            }
            selectedDownSpeed = bps;

            AltModePause();
            messenger.Send(new SetSessionRequest(session));
        }
        #endregion download speed limit menu stuff

        #region Navigation Methods
        private void OnMouseNav(MouseNavMessage navMsg)
        {
            // Over in App.xaml.cs, there's a method that intercepts the
            // mouse clicks, and if the click is one of the back or forward
            // buttons that most modern mice have, sends out a message that
            // we act on here.

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
            CanGoBack = navigationService.Journal.CanGoBack;
        }
        #endregion Navigation Methods

        #region ICommands and helpers
        #region ICommand Backing
        private DelegateCommand _UnloadedCommand;
        private DelegateCommand _LoadedCommand;
        private DelegateCommand _GoBackCommand;
        private DelegateCommand _HostsCommand;
        private DelegateCommand _SettingsCommand;
        private DelegateCommand _AddTorrentCommand;
        private DelegateCommand _AddMagnetCommand;
        private DelegateCommand _ToggleAltMode;
        #endregion ICommand Backing

        /// <summary>
        /// Returns true if the currently displayed page is the one specified in
        /// <paramref name="pageName"/>.
        /// </summary>
        /// <param name="pageName">
        /// One of the constants in <see cref="PageKeys"/>
        /// </param>
        /// <returns></returns>
        private bool IsOnPage(string pageName)
        {
            return !(navigationService is null) && navigationService.Journal.CurrentEntry.Uri.OriginalString == pageName;
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
                // This never gets called, IRL. But we do the right thing
                // anyway.
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
                var parameters = new NavigationParameters
                {
                    { SettingsViewModel.SettingsParameter, Session },
                };
                RequestNavigate(PageKeys.Settings, parameters);
            }, () => !IsOnPage(PageKeys.Settings));

        public DelegateCommand ToggleAltMode =>
            _ToggleAltMode = _ToggleAltMode ?? new DelegateCommand(() =>
        {
            IsAltModeEnabled = !IsAltModeEnabled;
            AltModePause();
            messenger.Send(new SetSessionRequest(nameof(SessionMutator.AltSpeedEnabled), IsAltModeEnabled));
        }, () => true);

        private IEnumerable<TorrentFileWanted> ParseTorrentFiles(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                if (TorrentReader.TryParse(file, out var meta))
                    yield return new TorrentFileWanted(file, meta);
            }
        }

        private bool isInAddTorrentDialog = false;

        private void DoAddTorrentDialog(IEnumerable<string> filenames)
        {
            if (isInAddTorrentDialog)
                return;
            isInAddTorrentDialog = true;

            var metadata = ParseTorrentFiles(filenames);
            if (metadata.Any())
            {
                var pararmeters = new DialogParameters()
                {
                    { AddTorrentViewModel.MetaParameter, metadata },
                };
                dialogService.ShowDialog(PageKeys.AddTorrent, pararmeters, (IDialogResult r) =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        var action = r.Parameters.GetValue<AddTorrentDisposition>(AddTorrentViewModel.ActionParameter);
                        var paused = action == AddTorrentDisposition.Pause;
                        foreach (var file in r.Parameters.GetValue<IEnumerable<AddTorrentInfo>>(AddTorrentViewModel.FilesParameter))
                        {
                            messenger.Send(new AddTorrentRequest(file.Path, file.UnwantedIndexes, paused));
                        }
                    }
                    isInAddTorrentDialog = false;
                });
            }
        }

        public DelegateCommand AddTorrentCommand =>
            _AddTorrentCommand = _AddTorrentCommand ?? new DelegateCommand(() =>
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                DefaultExt = ".torrent",
                Filter = "Torrent Files (*.torrent)|*.torrent",
                Multiselect = true,
            };
            if (openFileDialog.ShowDialog() == true)
            {
                DoAddTorrentDialog(openFileDialog.FileNames);
            }
            openFileDialog = null;
        }, () => true);

        public DelegateCommand AddMagnetCommand =>
            _AddMagnetCommand = _AddMagnetCommand ?? new DelegateCommand(() =>
        {
            dialogService.ShowDialog(PageKeys.AddMagnet, r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    // The response from the AddMagnet dialog should be two
                    // parameters, the first with the key "action", the second
                    // keyed with "link" The "action" key value will be "start",
                    // "pause", or "cancel". It shouldn't ever be "cancel" at
                    // this point since the Result won't be ButtonResult.OK

                    var action = r.Parameters.GetValue<AddTorrentDisposition>(AddMagnetViewModel.ActionParameter);
                    var link = r.Parameters.GetValue<string>(AddMagnetViewModel.LinkParameter);

                    messenger.Send(new AddMagnetRequest(link, action == AddTorrentDisposition.Pause));
                }
            });
        }, () => true);
        #endregion ICommands and helpers
    }
}
