using Humanizer;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Tidal.Client.Helpers;
using Tidal.Helpers;
using Tidal.Models;
using Tidal.Models.Messages;
using Tidal.Services.Abstract;

namespace Tidal.Services.Actual
{
    internal class SettingsService : BindableBase, ISettingsService
    {
        private const string settingsFile = "settings.json";
        private const StorageStrategy storageStrategy = StorageStrategy.Local;

        private readonly IFileService fileService;
        private readonly IMessenger messenger;
        private readonly IDictionary<string, string> persist;

        public SettingsService(IFileService fileService, IMessenger messenger)
        {
            this.fileService = fileService;
            this.messenger = messenger;

            persist = new Dictionary<string, string>();
            messenger.Subscribe<ShutdownMessage>(OnShutdown, Prism.Events.ThreadOption.PublisherThread);

            if (fileService.FileExists(settingsFile, storageStrategy))
            {
                var json = fileService.ReadAllText(settingsFile, storageStrategy);
                if (json != null && json.Length > 0)
                    persist = Json.ToObject<Dictionary<string, string>>(json);
            }
            else
            {
                fileService.CreateDirectory(storageStrategy);
            }
        }

        private void OnShutdown(ShutdownMessage obj)
        {
            messenger.Send(new SaveSettingsMessage(this));
            Save();
        }

        public void Save()
        {
            var bytes = Json.ToJSONBytes(persist);
            fileService.WriteAllBytes(bytes, settingsFile, storageStrategy);
        }

        public async Task SaveAsync()
        {
            byte[] json = Json.ToJSONBytes(persist);
            await fileService.WriteAllBytesAsync(json, settingsFile, storageStrategy);
        }

        protected T Read<T>(T def = default, [CallerMemberName] string propertyName = null)
        {
            if (persist.ContainsKey(propertyName))
            {
                return Json.ToObject<T>(persist[propertyName]);
            }
            else
            {
                var json = Json.ToJSON(def);
                persist.Add(propertyName, json);
                return def;
            }
        }

        protected void Write<T>(T value, [CallerMemberName] string propertyName = null)
        {
            if (persist.ContainsKey(propertyName))
            {
                T obj = Json.ToObject<T>(persist[propertyName]);
                if (EqualityComparer<T>.Default.Equals(value, obj) == false)
                {
                    persist[propertyName] = Json.ToJSON(value);
                    RaisePropertyChanged(propertyName);
                }
            }
            else
            {
                persist.Add(propertyName, Json.ToJSON(value));
            }
        }

        public int Width { get => Read(900); set => Write(value); }
        public int Height { get => Read(720); set => Write(value); }
        public int Top { get => Read(150); set => Write(value); }
        public int Left { get => Read(300); set => Write(value); }

        public ThemeMode ThemeMode { get => Read(ThemeMode.SystemDefault); set => Write(value); }

        public Guid ActiveHost
        {
            get => Read(Guid.NewGuid());
            set => Write(value);
        }

        public List<Host> Hosts
        {
            get => Read(new List<Host>());
            set => Write(value);
        }

        public Uri SettingsPage
        {
            get => Read<Uri>();
            set => Write(value);
        }

        public TimeSpan DeadHostTime
        {
            get => Read(60.Minutes());
            set => Write(value);
        }

        public bool AggressiveGC
        {
            get => Read(true);
            set => Write(value);
        }

        public TimeSpan PollingInterval
        {
            get => Read(3.Seconds());
            set => Write(value);
        }

        public List<string> SelectedHashes
        {
            get => Read(new List<string>());
            set => Write(value);
        }

        public string MaxMindLicenseKey
        {
            get => Read<string>();
            set => Write(value);
        }

        public string MaxMindUserName
        {
            get => Read<string>();
            set => Write(value);
        }

        public string MaxMindPassword
        {
            get => Read<string>();
            set => Write(value);
        }

        public string MaxMindPermaLink
        {
            get => Read("https://download.maxmind.com/app/geoip_download?" +
                $"edition_id=GeoLite2-City&license_key=YOUR_LICENSE_KEY&suffix=tar.gz");
            set => Write(value);
        }

        public string GeoDbFileName
        {
            get => Read("GeoLite2-City.mmdb");
            set => Write(value);
        }

        public TimeSpan GeoIpRetrievalInterval
        {
            get => Read(TimeSpan.FromDays(7));
            set => Write(value);
        }

        public LayoutInfo MainPageLayout
        {
            get => Read<LayoutInfo>(default);
            set => Write(value);
        }

        public string TorrentGridInfo
        {
            get => Read<string>();
            set => Write(value);
        }

        public string PeerGridInfo
        {
            get => Read<string>();
            set => Write(value);
        }

        public string FileGridInfo
        {
            get => Read<string>();
            set => Write(value);
        }

        public string UploadPresets
        {
            get => Read("2000,1500,1000,750,500,375,250,175,100");
            set => Write(value);
        }

        public string DownloadPresets
        {
            get => Read("7500,5000,2000,1500,1000,750,500,375,250,175,100");
            set => Write(value);
        }
    }
}
