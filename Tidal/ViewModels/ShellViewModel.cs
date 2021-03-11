﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
using Tidal.Models;
using Tidal.Models.BrokerMessages;
using Tidal.Models.Messages;
using Tidal.Properties;
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
        private readonly ITorrentStatusService torrentStatusService;
        private readonly INotificationService notificationService;
        private readonly SynchronizationContext context;
        private IRegionNavigationService navigationService;
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

            // Misc subscriptions
            messenger.Subscribe<StartupMessage>(OnStartup);
            messenger.Subscribe<MouseNavMessage>(OnMouseNav);
            messenger.Subscribe<HostChangedMessage>(OnHostChanged);

            // Error subscriptions
            messenger.Subscribe<FatalMessage>(OnFatalError);
            messenger.Subscribe<WarningMessage>(OnWarning);
            messenger.Subscribe<InfoMessage>(OnInfo);

            // Client subscriptions
            messenger.Subscribe<SessionResponse>(OnSession);
            messenger.Subscribe<SessionStatsResponse>(OnSessionStats);
            messenger.Subscribe<TorrentResponse>(OnTorrents);
            messenger.Subscribe<FreeSpaceResponse>(OnFreeSpace);
        }

        private void UiInvoke(Action action)
        {
            context.Post(o => action.Invoke(), null);
        }

        #region Startup Stuff
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
        #endregion

        #region Scheduled Tasks
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
        #endregion

        #region Client Subscriptions
        private void OnSession(SessionResponse sessionResponse)
        {
            Session = Session ?? new Session();
            UiInvoke(() =>
            {
                Session.Assign(sessionResponse.Session);
                messenger.Send(new FreeSpaceRequest(Session.DownloadDirectory));
                //SetAltLabel(Session);
            });
        }

        private void OnSessionStats(SessionStatsResponse statsResponse)
        {
            SessionStats = SessionStats ?? new SessionStats();
            UiInvoke(() => SessionStats.Assign(statsResponse.SessionStats));
        }

        private void OnTorrents(TorrentResponse torrentResponse)
        {
            torrentStatusService.CheckStatus(torrentResponse.Torrents);
            torrentStatusService.CheckForConnection(torrentResponse.Torrents);
        }

        private void OnFreeSpace(FreeSpaceResponse freeSpace)
        {
            UiInvoke(() => FreeSpace = freeSpace.FreeSpace);
        }
        #endregion

        #region Error Subscriptions and handling
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
        #endregion

        #region Other Subscriptions
        private async void OnHostChanged(HostChangedMessage hostChangedMessage)
        {
            var host = hostService.GetHost(hostChangedMessage.ActiveId);
            if (host != null)
                IsOpen = await brokerService.OpenAsync(host);
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

        #region ICommands and helpers
        #region ICommand Backing
        private DelegateCommand _UnloadedCommand;
        private DelegateCommand _LoadedCommand;
        private DelegateCommand _GoBackCommand;
        private DelegateCommand _HostsCommand;
        private DelegateCommand _SettingsCommand;
        private DelegateCommand _AddTorrentCommand;
        private DelegateCommand _AddMagnetCommand;
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

        private void DoAddTorrentDialog(string filename)
        {
            if (TorrentReader.TryParse(filename, out var meta))
            {
                // Should I try to describe the Prism Dialog service here? Well,
                // here and in AddTorrentViewModel, too. Future Keith needs
                // this.
                //
                // The first thing to remember is that Prism creates its own
                // window and puts your dialog inside it; your dialog is just
                // another UserControl. Not a Page, nor a Window; just a
                // UserControl. Of course, that limits you to the system look
                // for the dialog window, but that can be remedied, too.
                //
                // To call the dialog, first create an instance of
                // IDialogParameters, like I do here (unless the dialog doesn't
                // need any information, but that'd be weird). This is a simple
                // extension of a KeyValuePair list, so feel free to put any
                // sort of value inside.
                var parms = new DialogParameters()
                {
                    // Naturally, the order of the parameters, since this is a
                    // key-value enumeration, order isn't important, but
                    // consistent use of the keys is. Don't use literals for the
                    // keys. You'll mistype something and everything will fall
                    // apart. You know you will.
                    //
                    // The data passed can be anything, too. Not just strings or
                    // simple values, but things like the MetaParameter, which
                    // is an instance of TorrentMetadata, are fine. They're just
                    // objects and not passed across any kind of marshaling 
                    // barrier, so feel free to use anything.

                    { AddTorrentViewModel.PathParameter, filename },
                    { AddTorrentViewModel.MetaParameter, meta },
                    { AddTorrentViewModel.IsValidParameter, true },
                };

                // Call the dialog service's ShowDialog method with the dialog
                // page key and the parameters just created.The third parameter
                // in this call is a callback, specified as an Action with an
                // IDialogResult parameter. The IDialogResult has the result of
                // the dialog, like OK, Cancel, or whatever. Normal dialog
                // results. It also has the dialog's own set of parameters to
                // give back any data necessary.
                dialogService.ShowDialog(PageKeys.AddTorrent, parms, (IDialogResult r) =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        // All of the data from the call to the dialog is
                        // returned via an instance of IDialogParameters, just
                        // like when you specified your data for the dialog.
                        //
                        // The only way to get at this data is through the
                        // GetValue<T> method, which needs a key to access the
                        // value, natch.

                        var action = r.Parameters.GetValue<AddTorrentDisposition>(AddTorrentViewModel.ActionParameter);
                        var path = r.Parameters.GetValue<string>(AddTorrentViewModel.PathParameter);
                        var unwanted = r.Parameters.GetValue<IEnumerable<int>>(AddTorrentViewModel.UnwantedParameter);
                        var paused = action == AddTorrentDisposition.Pause;

                        messenger.Send(new AddTorrentRequest(path, unwanted, paused));
                    }
                });
            }
            else
            {
                string message = string.Format(Resources.TorrentParsingError_1, Path.GetFileName(filename));
                string header = Resources.ParsingError;
                notificationService.ShowInfo(message, header, 10.Seconds());
            }
        }

        public DelegateCommand AddTorrentCommand =>
            _AddTorrentCommand = _AddTorrentCommand ?? new DelegateCommand(() =>
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                DefaultExt = ".torrent",
                Filter = "Torrent Files (*.torrent)|*.torrent"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                DoAddTorrentDialog(openFileDialog.FileName);
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

        #endregion
    }
}
