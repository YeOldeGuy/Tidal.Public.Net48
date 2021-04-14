using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using ControlzEx.Theming;
using Humanizer;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Tidal.Client.Models;
using Tidal.Core.Helpers;
using Tidal.Helpers;
using Tidal.Models.BrokerMessages;
using Tidal.Models.Messages;
using Tidal.Properties;
using Tidal.Services.Abstract;


namespace Tidal.ViewModels
{
    public class SettingsViewModel : BindableBase, INavigationAware
    {
        private List<IDisposable> disposables;
        private readonly ISettingsService settingsService;
        private readonly IGeoService geoService;
        private readonly IFileService fileService;
        private readonly IMessenger messenger;
        private bool _IsOpen;

        public const string SettingsParameter = "settings";

        public SettingsViewModel(ISettingsService settingsService,
                                 IFileService fileService,
                                 IMessenger messenger,
                                 IGeoService geoService)
        {
            this.settingsService = settingsService;
            this.geoService = geoService;
            this.fileService = fileService;
            this.messenger = messenger;
        }

        #region INavigationAware Methods
        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            IsOpen = true;

            Setter = new Session();
            disposables = new List<IDisposable>();

            var parameters = navigationContext.Parameters;
            if (parameters.ContainsKey(SettingsParameter))
                Setter.Assign((Session)parameters[SettingsParameter]);

            Setter.PropertyChanged += Setter_PropertyChanged;
            disposables.Add(messenger.Subscribe<SessionResponse>(OnSession));

            disposables.Add(messenger.Subscribe<HaltMessage>(m => IsOpen = false));
            disposables.Add(messenger.Subscribe<ResumeMessage>(m => IsOpen = true));

            messenger.Send(new SessionRequest());
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            Setter.PropertyChanged -= Setter_PropertyChanged;
            geoService.PropertyChanged -= WatchForDLComplete;

            foreach (var disposable in disposables)
                disposable.Dispose();
        }
        #endregion

        #region Message handlers
        private void OnSession(SessionResponse obj)
        {
            // While we're servicing the session response, turn off the event
            // monitoring until we're done

            Setter.PropertyChanged -= Setter_PropertyChanged;
            try
            {
                Setter.Assign(obj.Session);

                // Set local representations for the setting pane
                EncryptionIsPreferred = Setter.Encryption == EncryptionLevel.Preferred;
                EncryptionIsRequired = Setter.Encryption == EncryptionLevel.Required;
                EncryptionIsTolerated = Setter.Encryption == EncryptionLevel.Tolerated;

                IdleTime = new DateTime(2020, 1, 1, Setter.IdleSeedingLimit.Hours, Setter.IdleSeedingLimit.Minutes, 0);
                GetMMDBStatus();
            }
            finally
            {
                Setter.PropertyChanged += Setter_PropertyChanged;
            }
        }
        #endregion

        #region Property Helpers
        private void WatchForDLComplete(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IGeoService.IsDownloading))
            {
                FetchMMDBCommand.RaiseCanExecuteChanged();
                LoadMMDBCommand.RaiseCanExecuteChanged();
            }
        }
        private void Setter_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Setter.AltScheduleDays))
                RaisePropertyChanged(nameof(AltSpeedDays));

            if (sender is Session session)
            {
                var info = session.GetType().GetProperty(e.PropertyName);
                if (info is null)
                {
                    messenger.Send(new WarningMessage(string.Format(Resources.PropertySetFailWarning_1, e.PropertyName),
                                                      Resources.PropertySetFailHeader,
                                                      TimeSpan.FromSeconds(10)));
                }
                else
                {
                    var value = info.GetValue(session);
                    string desc = PropertyHelpers.GetDescription<SessionMutator>(e.PropertyName);

                    // Special case for properties that are booleans to show
                    // what is actually happening to the value

                    var verb = (value is bool b && !b) ? Resources.Clearing : Resources.Setting;
                    messenger.Send(new StatusInfoMessage($"{verb} {desc}", TimeSpan.FromSeconds(1)));

                    messenger.Send(new SetSessionRequest(e.PropertyName, value));
                }
            }
        }

        private void GetMMDBStatus()
        {
            if (geoService.IsDownloading)
            {
                MMDBStatusReport = string.Format(Resources.GeoDBFileDownloading_1, settingsService.GeoDbFileName);
            }
            else if (fileService.FileExists(settingsService.GeoDbFileName, StorageStrategy.Local))
            {
                var age = fileService.GetFileAge(settingsService.GeoDbFileName, StorageStrategy.Local);
                var hum = age.Humanize(2, minUnit: Humanizer.Localisation.TimeUnit.Minute);
                MMDBStatusReport = string.Format(Resources.GeoDbFileAge_2, settingsService.GeoDbFileName, hum);
            }
            else
            {
                MMDBStatusReport = string.Format(Resources.GeoDBFileNotFound_1, settingsService.GeoDbFileName);
            }
        }
        #endregion

        #region Visible Properties
        private string _MMDBStatusReport;
        private DateTime _IdleTime;
        private bool _EncryptionIsRequired;
        private bool _EncryptionIsPreferred;
        private bool _EncryptionIsTolerated;
        private ObservableCollection<bool> _AltSpeedDays;
        private DateTime _AltSpeedTimeBegin;
        private DateTime _AltSpeedTimeEnd;
        #region Backing Store
        #endregion

        public Session Setter { get; set; }

        public bool IsOpen { get => _IsOpen; set => SetProperty(ref _IsOpen, value); }

        public string MMDBStatusReport
        {
            get => _MMDBStatusReport;
            set => SetProperty(ref _MMDBStatusReport, value);
        }

        public bool AggressiveGC
        {
            get => settingsService.AggressiveGC;
            set => settingsService.AggressiveGC = value;
        }

        public int DeadHostTimeout
        {
            get => (int)settingsService.DeadHostTime.TotalMinutes;
            set => settingsService.DeadHostTime = TimeSpan.FromMinutes(value);
        }

        public int ThemeSettingIndex
        {
            get => (int)settingsService.ThemeMode;
            set
            {
                switch (value)
                {
                    case 0:
                        settingsService.ThemeMode = ThemeMode.Light;
                        ThemeManager.Current.ChangeThemeBaseColor(Application.Current, ThemeManager.BaseColorLight);
                        ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.DoNotSync;
                        break;
                    case 1:
                        settingsService.ThemeMode = ThemeMode.Dark;
                        ThemeManager.Current.ChangeThemeBaseColor(Application.Current, ThemeManager.BaseColorDark);
                        ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.DoNotSync;
                        break;
                    case 2:
                        settingsService.ThemeMode = ThemeMode.SystemDefault;
                        ThemeManager.Current.SyncTheme(ThemeSyncMode.SyncAll);
                        ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncAll;
                        break;
                }
            }
        }

        public DateTime IdleTime { get => _IdleTime; set => SetProperty(ref _IdleTime, value); }

        public bool EncryptionIsRequired
        {
            get => _EncryptionIsRequired;
            set
            {
                if (SetProperty(ref _EncryptionIsRequired, value) && value)
                    Setter.Encryption = EncryptionLevel.Required;
            }
        }

        public bool EncryptionIsPreferred
        {
            get => _EncryptionIsPreferred;
            set
            {
                if (SetProperty(ref _EncryptionIsPreferred, value) && value)
                    Setter.Encryption = EncryptionLevel.Preferred;
            }
        }

        public bool EncryptionIsTolerated
        {
            get => _EncryptionIsTolerated;
            set
            {
                if (SetProperty(ref _EncryptionIsTolerated, value) && value)
                    Setter.Encryption = EncryptionLevel.Tolerated;
            }
        }

        public DateTime AltSpeedTimeBegin
        {
            get => _AltSpeedTimeBegin;
            set
            {
                if (SetProperty(ref _AltSpeedTimeBegin, value))
                    Setter.AltScheduleBegin = value;
            }
        }

        public DateTime AltSpeedTimeEnd
        {
            get => _AltSpeedTimeEnd;
            set
            {
                if (SetProperty(ref _AltSpeedTimeEnd, value))
                    Setter.AltScheduleEnd = value;
            }
        }

        public ObservableCollection<bool> AltSpeedDays
        {
            get
            {
                if (Setter == null)
                    return null;

                if (_AltSpeedDays == null)
                {
                    _AltSpeedDays = new ObservableCollection<bool>(new DayMap(Setter.AltScheduleDays));
                    _AltSpeedDays.CollectionChanged += (s, e) =>
                    {
                        Setter.AltScheduleDays = new DayMap(_AltSpeedDays).AsInteger();
                    };
                }
                return _AltSpeedDays;
            }
        }

        public double PollingIntervalInSeconds
        {
            get => settingsService.PollingInterval.TotalSeconds;
            set
            {
                if (Equals(value, settingsService.PollingInterval.TotalSeconds))
                    return;
                settingsService.PollingInterval = TimeSpan.FromSeconds(value);
                RaisePropertyChanged();
            }
        }

        public string MaxMindUserName
        {
            get => settingsService.MaxMindUserName;
            set
            {
                if (Equals(value, settingsService.MaxMindUserName))
                    return;
                settingsService.MaxMindUserName = value;
                RaisePropertyChanged();
                FetchMMDBCommand.RaiseCanExecuteChanged();
            }
        }

        public string MaxMindPassword
        {
            get => settingsService.MaxMindPassword;
            set
            {
                if (Equals(value, settingsService.MaxMindPassword))
                    return;
                settingsService.MaxMindPassword = value;
                RaisePropertyChanged();
                FetchMMDBCommand.RaiseCanExecuteChanged();
            }
        }

        public string MaxMindLicenseKey
        {
            get => settingsService.MaxMindLicenseKey;
            set
            {
                if (Equals(value, settingsService.MaxMindLicenseKey))
                    return;
                settingsService.MaxMindLicenseKey = value;
                RaisePropertyChanged();
                FetchMMDBCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion

        #region Commands
        #region Backing Store
        private DelegateCommand _LoadMMDBCommand;
        private DelegateCommand _FetchMMDBCommand;
        private DelegateCommand<string> _VisitUrl;
        #endregion

        public DelegateCommand LoadMMDBCommand =>
            _LoadMMDBCommand = _LoadMMDBCommand ?? new DelegateCommand(async () =>
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = Resources.OpenMMDBFilePickerTitle,
                Filter = Resources.OpenMMDBFilePickerFilter,
                DefaultExt = ".mmdb"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                await geoService.LoadDbFileAsync(openFileDialog.FileName);
                GetMMDBStatus();
            }
        }, () => !geoService.IsDownloading);


        private bool HasMMInfo =>
            !string.IsNullOrEmpty(MaxMindUserName) &&
            !string.IsNullOrEmpty(MaxMindPassword) &&
            !string.IsNullOrEmpty(MaxMindLicenseKey);

        public DelegateCommand FetchMMDBCommand =>
            _FetchMMDBCommand = _FetchMMDBCommand ?? new DelegateCommand(async () =>
        {
            FetchMMDBCommand.RaiseCanExecuteChanged();
            await geoService.DownloadMaxMindAsync(MaxMindUserName,
                                                  MaxMindPassword,
                                                  MaxMindLicenseKey);
        }, () => !geoService.IsDownloading && HasMMInfo);


        public DelegateCommand<string> VisitUrl =>
            _VisitUrl = _VisitUrl ?? new DelegateCommand<string>((uri) =>
        {
            Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
        }, (s) => true);
        #endregion
    }
}
