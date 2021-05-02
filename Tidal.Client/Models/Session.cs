using System;
using System.Runtime.Serialization;
using Tidal.Client.Constants;

namespace Tidal.Client.Models
{
    public class Session : Assignable<Session>
    {
        #region Backing Store
        private bool _AltSpeedEnabled;
        private int _AltSpeedDown;
        private long _AltSpeedUp;
        private bool _AltScheduleEnabled;
        private int _AltSpeedEndMinutes;
        private int _AltSpeedBeginMinutes;
        private int _AltScheduleDays;
        private string _DownloadDirectory;
        private int _DownloadQueueSize;
        private bool _DownloadQueueEnabled;
        private bool _IncompleteDirectoryEnabled;
        private string _IncompleteDirectory;
        private bool _RenamePartialFiles;
        private bool _DHTEnabled;
        private bool _LPDEnabled;
        private bool _PEXEnabled;
        private bool _UTPEnabled;
        private string _EncryptionLevelString;
        private long _IdleSeedingLimitMinutes;
        private bool _IdleSeedingLimitEnabled;
        private double _SeedRatioLimit;
        private bool _SeedRatioLimited;
        private int _PeerLimitGlobal;
        private int _PeerLimitPerTorrent;
        private int _PeerPort;
        private bool _PeerPortRandomize;
        private long _SpeedLimitDown;
        private long _SpeedLimitUp;
        private bool _SpeedLimitDownEnabled;
        private bool _SpeedLimitUpEnabled;
        private string _Version;
        #endregion Backing Store

        #region Assignable<T>
        protected override void AssignInternal(Session other)
        {
            AltSpeedEnabled = other.AltSpeedEnabled;
            AltScheduleEnabled = other.AltScheduleEnabled;
            AltSpeedDown = other.AltSpeedDown;
            AltSpeedUp = other.AltSpeedUp;
            AltScheduleEndMinutes = other.AltScheduleEndMinutes;
            AltScheduleBeginMinutes = other.AltScheduleBeginMinutes;
            AltScheduleDays = other.AltScheduleDays;
            DownloadDirectory = other.DownloadDirectory;
            DownloadQueueSize = other.DownloadQueueSize;
            DownloadQueueEnabled = other.DownloadQueueEnabled;
            IncompleteDirectoryEnabled = other.IncompleteDirectoryEnabled;
            IncompleteDirectory = other.IncompleteDirectory;
            RenamePartialFiles = other.RenamePartialFiles;
            DHTEnabled = other.DHTEnabled;
            LPDEnabled = other.LPDEnabled;
            PEXEnabled = other.PEXEnabled;
            UTPEnabled = other.UTPEnabled;
            EncryptionLevelString = other.EncryptionLevelString;
            IdleSeedingLimitMinutes = other.IdleSeedingLimitMinutes;
            IdleSeedingLimitEnabled = other.IdleSeedingLimitEnabled;
            SeedRatioLimit = other.SeedRatioLimit;
            SeedRatioLimited = other.SeedRatioLimited;
            PeerLimitGlobal = other.PeerLimitGlobal;
            PeerLimitPerTorrent = other.PeerLimitPerTorrent;
            PeerPort = other.PeerPort;
            PeerPortRandomize = other.PeerPortRandomize;
            SpeedLimitDown = other.SpeedLimitDown;
            SpeedLimitDownEnabled = other.SpeedLimitDownEnabled;
            SpeedLimitUp = other.SpeedLimitUp;
            SpeedLimitUpEnabled = other.SpeedLimitUpEnabled;
            Version = other.Version;
        }
        #endregion Assignable<T>

        #region Properties
        /// <summary>
        /// Are the Alternate speed settings currently in use?
        /// </summary>
        [DataMember(Name = RpcConstants.AltSpeedEnabled)]
        public bool AltSpeedEnabled
        {
            get => _AltSpeedEnabled;
            set => SetProperty(ref _AltSpeedEnabled, value);
        }

        /// <summary>
        /// If the alternative speed settings are in use, this is the maximum
        /// global speed of downloads, expressed as KBps (50 would be 50KBps)
        /// </summary>
        [DataMember(Name = RpcConstants.AltSpeedDown)]
        public int AltSpeedDown
        {
            get => _AltSpeedDown;
            set => SetProperty(ref _AltSpeedDown, value);
        }

        /// <summary>
        /// If the alternative speed settings are in use, this is the maximum
        /// global speed of uploads, expressed as KBps (50 would be 50KBps)
        /// </summary>
        [DataMember(Name = RpcConstants.AltSpeedUp)]
        public long AltSpeedUp
        {
            get => _AltSpeedUp;
            set => SetProperty(ref _AltSpeedUp, value);
        }

        /// <summary>
        /// If <see langword="true"/>, then the alternative speeds will be
        /// enabled and disabled according to a schedule.
        /// </summary>
        /// <remarks>
        /// This is <b>different</b> than the <see cref="AltSpeedEnabled"/>
        /// value. The other controls whether or not alternate speeds are
        /// enabled; this controls whether to apply a schedule.
        /// </remarks>
        [DataMember(Name = RpcConstants.AltSpeedTimeEnabled)]
        public bool AltScheduleEnabled
        {
            get => _AltScheduleEnabled;
            set => SetProperty(ref _AltScheduleEnabled, value);
        }

        /// <summary>
        /// If an automatic schedule is enabled for setting the alternate speed
        /// settings, this will be the time each day it is engaged.
        /// </summary>
        /// <remarks>
        /// This value is the unprocessed value from the client, expressed as
        /// the number of minutes after midnight.
        /// </remarks>
        [DataMember(Name = RpcConstants.AltSpeedBegin)]
        public int AltScheduleBeginMinutes
        {
            get => _AltSpeedBeginMinutes;
            set
            {
                if (SetProperty(ref _AltSpeedBeginMinutes, value))
                    RaisePropertyChanged(nameof(AltScheduleBegin));
            }
        }

        /// <summary>
        /// A processed representation of the <see cref="AltScheduleBeginMinutes"/>
        /// value, expressed as a time after the current date's midnight.
        /// </summary>
        [IgnoreDataMember]
        public DateTime AltScheduleBegin
        {
            get => DateTime.Today + TimeSpan.FromMinutes(AltScheduleBeginMinutes);
            set => AltScheduleBeginMinutes = (int)value.TimeOfDay.TotalMinutes;
        }

        /// <summary>
        /// If an automatic schedule is enabled for setting the alternate speed
        /// settings, this will be the time each day it is disabled and a return
        /// to normal up/down speeds is accomplished.
        /// </summary>
        /// <remarks>
        /// This value is the unprocessed value from the client, expressed as
        /// the number of minutes after midnight.
        /// </remarks>
        [DataMember(Name = RpcConstants.AltSpeedEnd)]
        public int AltScheduleEndMinutes
        {
            get => _AltSpeedEndMinutes;
            set
            {
                if (SetProperty(ref _AltSpeedEndMinutes, value))
                    RaisePropertyChanged(nameof(AltScheduleEnd));
            }
        }

        /// <summary>
        /// A processed representation of the <see cref="AltScheduleEndMinutes"/>
        /// value, expressed as a time after the current date's midnight.
        /// </summary>
        [IgnoreDataMember]
        public DateTime AltScheduleEnd
        {
            get => DateTime.Today + TimeSpan.FromMinutes(AltScheduleEndMinutes);
            set => AltScheduleEndMinutes = (int)value.TimeOfDay.TotalMinutes;
        }

        /// <summary>
        /// A bitmap of the days of the week that the alternate speed schedule
        /// should be applied, bit 0 = Sunday, bit 1 = Monday, bit 6 = Saturday.
        /// </summary>
        /// <remarks>
        /// A value of 62 (binary: 0011 1110) would represent Mon-Fri engaged
        /// and the weekend not. The eighth bit is ignored, natch.
        /// </remarks>
        [DataMember(Name = RpcConstants.AltSpeedTimeDays)]
        public int AltScheduleDays
        {
            get => _AltScheduleDays;
            set => SetProperty(ref _AltScheduleDays, value);
        }

        /// <summary>
        /// The directory that completed torrents are stored in <b>on the
        /// host computer</b>.
        /// </summary>
        [DataMember(Name = RpcConstants.DownloadDir)]
        public string DownloadDirectory
        {
            get => _DownloadDirectory;
            set => SetProperty(ref _DownloadDirectory, value);
        }

        /// <summary>
        /// The maximum number of torrents to download simultaneously, if the
        /// value of <see cref="DownloadQueueEnabled"/> is <see
        /// langword="true"/>.
        /// </summary>
        [DataMember(Name = RpcConstants.DownloadQueueSize)]
        public int DownloadQueueSize
        {
            get => _DownloadQueueSize;
            set => SetProperty(ref _DownloadQueueSize, value);
        }

        /// <summary>
        /// If <see langword="true"/>, there will be a maximum number of
        /// torrents that can be downloaded simultaneously.
        /// </summary>
        /// <seealso cref="DownloadQueueSize"/>
        [DataMember(Name = RpcConstants.DownloadQueueEnabled)]
        public bool DownloadQueueEnabled
        {
            get => _DownloadQueueEnabled;
            set => SetProperty(ref _DownloadQueueEnabled, value);
        }

        /// <summary>
        /// If <see cref="IncompleteDirectoryEnabled"/> is <see
        /// langword="true"/>, then partial downloads will be stored in this
        /// directory, then moved when finished.
        /// </summary>
        [DataMember(Name = RpcConstants.IncompleteDir)]
        public string IncompleteDirectory
        {
            get => _IncompleteDirectory;
            set => SetProperty(ref _IncompleteDirectory, value);
        }

        /// <summary>
        /// If <see langword="true"/>, partial torrent downloads will be
        /// stored in <see cref="IncompleteDirectory"/>.
        /// </summary>
        [DataMember(Name = RpcConstants.IncompleteDirEnabled)]
        public bool IncompleteDirectoryEnabled
        {
            get => _IncompleteDirectoryEnabled;
            set => SetProperty(ref _IncompleteDirectoryEnabled, value);
        }

        /// <summary>
        /// if <see langword="true"/>, then partial downloads will have a suffix
        /// of ".part" appended to the file name.
        /// </summary>
        /// <remarks>
        /// This doesn't make much sense when <see
        /// cref="IncompleteDirectoryEnabled"/> is <see langword="true"/>, but
        /// there you go.
        /// </remarks>
        [DataMember(Name = RpcConstants.RenamePartialFiles)]
        public bool RenamePartialFiles
        {
            get => _RenamePartialFiles;
            set => SetProperty(ref _RenamePartialFiles, value);
        }

        /// <summary>
        /// I haven't the foggiest what it does, but if it's <see
        /// langword="true"/>, then DHT will be allowed in public torrents.
        /// </summary>
        [DataMember(Name = RpcConstants.DHTEnabled)]
        public bool DHTEnabled
        {
            get => _DHTEnabled;
            set => SetProperty(ref _DHTEnabled, value);
        }

        /// <summary>
        /// Local Peer Discovery on/off.
        /// </summary>
        [DataMember(Name = RpcConstants.LPDEnabled)]
        public bool LPDEnabled
        {
            get => _LPDEnabled;
            set => SetProperty(ref _LPDEnabled, value);
        }

        /// <summary>
        /// I haven't the foggiest what it does, but if it's <see
        /// langword="true"/>, then PEX will be allowed in public torrents.
        /// </summary>
        [DataMember(Name = RpcConstants.PEXEnabled)]
        public bool PEXEnabled
        {
            get => _PEXEnabled;
            set => SetProperty(ref _PEXEnabled, value);
        }

        /// <summary>
        /// I haven't the foggiest what it does, but if it's <see
        /// langword="true"/>, then UTP will be allowed in public torrents.
        /// </summary>
        [DataMember(Name = RpcConstants.UTPEnabled)]
        public bool UTPEnabled
        {
            get => _UTPEnabled;
            set => SetProperty(ref _UTPEnabled, value);
        }

        /// <summary>
        /// The string representing the encryption level as presented by
        /// the client.
        /// </summary>
        [DataMember(Name = RpcConstants.Encryption)]
        public string EncryptionLevelString
        {
            get => _EncryptionLevelString;
            set
            {
                if (SetProperty(ref _EncryptionLevelString, value))
                    RaisePropertyChanged(nameof(Encryption));
            }
        }

        /// <summary>
        /// The encryption level that the client enforces.
        /// </summary>
        [IgnoreDataMember]
        public EncryptionLevel Encryption
        {
            get
            {
                switch (EncryptionLevelString)
                {
                    case RpcConstants.EncryptionRequired:
                        return EncryptionLevel.Required;
                    case RpcConstants.EncryptionPreferred:
                        return EncryptionLevel.Preferred;
                    case RpcConstants.EncryptionTolerated:
                        return EncryptionLevel.Tolerated;
                    default:
                        return EncryptionLevel.Preferred;
                }
            }
            set
            {
                switch (value)
                {
                    case EncryptionLevel.Preferred:
                        EncryptionLevelString = RpcConstants.EncryptionPreferred;
                        break;
                    case EncryptionLevel.Required:
                        EncryptionLevelString = RpcConstants.EncryptionRequired;
                        break;
                    default:
                        EncryptionLevelString = RpcConstants.EncryptionTolerated;
                        break;
                }
            }
        }

        /// <summary>
        /// The value that a seeding torrent can idle, not uploading to a peer
        /// before the seeding is stopped.
        /// </summary>
        /// <remarks>
        /// This is the value as presented by the host, in the number of
        /// minutes.
        /// </remarks>
        [DataMember(Name = RpcConstants.IdleSeedingLimit)]
        public long IdleSeedingLimitMinutes
        {
            get => _IdleSeedingLimitMinutes;
            set
            {
                if (SetProperty(ref _IdleSeedingLimitMinutes, value))
                    RaisePropertyChanged(nameof(IdleSeedingLimit));
            }
        }

        /// <summary>
        /// The amount of time that a seeding torrent can idle, not uploading to
        /// a peer before the seeding is stopped.
        /// </summary>
        [IgnoreDataMember]
        public TimeSpan IdleSeedingLimit
        {
            get => TimeSpan.FromMinutes(IdleSeedingLimitMinutes);
            set => IdleSeedingLimitMinutes = (long)value.TotalMinutes;
        }

        /// <summary>
        /// If <see langword="true"/>, then torrents will be monitored for idle
        /// time and shutdown if the <see cref="IdleSeedingLimit"/> is reached.
        /// </summary>
        [DataMember(Name = RpcConstants.IdleSeedingLimitEnabled)]
        public bool IdleSeedingLimitEnabled
        {
            get => _IdleSeedingLimitEnabled;
            set => SetProperty(ref _IdleSeedingLimitEnabled, value);
        }

        /// <summary>
        /// If enabled, torrents will be monitored and once the ratio of amount
        /// uploaded to amount downloaded reaches this value, it will be
        /// stopped.
        /// </summary>
        [DataMember(Name = RpcConstants.SeedRatioLimit)]
        public double SeedRatioLimit
        {
            get => _SeedRatioLimit;
            set => SetProperty(ref _SeedRatioLimit, value);
        }

        /// <summary>
        /// If <see langword="true"/>, the <see cref="SeedRatioLimit"/> will be
        /// honored.
        /// </summary>
        [DataMember(Name = RpcConstants.SeedRatioLimited)]
        public bool SeedRatioLimited
        {
            get => _SeedRatioLimited;
            set => SetProperty(ref _SeedRatioLimited, value);
        }

        /// <summary>
        /// The maximum number of peers the client will allow, spread across all
        /// active torrents.
        /// </summary>
        [DataMember(Name = RpcConstants.PeerLimitGlobal)]
        public int PeerLimitGlobal
        {
            get => _PeerLimitGlobal;
            set => SetProperty(ref _PeerLimitGlobal, value);
        }

        /// <summary>
        /// The maximum number of peers a torrent can have. Note that the number
        /// may be exceeded on occasion and that each torrent also has its own
        /// changeable figure.
        /// </summary>
        [DataMember(Name = RpcConstants.PeerLimitPerTorrent)]
        public int PeerLimitPerTorrent
        {
            get => _PeerLimitPerTorrent;
            set => SetProperty(ref _PeerLimitPerTorrent, value);
        }

        /// <summary>
        /// Gets the port number that peers may connect to the host on. This
        /// used to be 6881-6889, but those are blocked by many ISPs, and have
        /// fallen from favor.
        /// </summary>
        [DataMember(Name = RpcConstants.PeerPort)]
        public int PeerPort
        {
            get => _PeerPort;
            set => SetProperty(ref _PeerPort, value);
        }

        /// <summary>
        /// If <see langword="true"/>, then when the host is restarted, it will
        /// choose a <see cref="PeerPort"/> value at random. This is kinda
        /// useless unless using a local instance of Transmission that you start
        /// and stop frequently. On a server, where the app will run for weeks
        /// or months between restarts, it isn't as useful.
        /// </summary>
        [DataMember(Name = RpcConstants.PeerPortRandomize)]
        public bool PeerPortRandomize
        {
            get => _PeerPortRandomize;
            set => SetProperty(ref _PeerPortRandomize, value);
        }

        /// <summary>
        /// If <see cref="SpeedLimitDownEnabled"/> is <see langword="true"/>, then
        /// this represents the global, non-alt-mode download speed in KB/s.
        /// </summary>
        [DataMember(Name = RpcConstants.SpeedLimitDown)]
        public long SpeedLimitDown
        {
            get => _SpeedLimitDown;
            set => SetProperty(ref _SpeedLimitDown, value);
        }

        /// <summary>
        /// If <see cref="SpeedLimitUpEnabled"/> is <see langword="true"/>, then
        /// this represents the global, non-alt-mode upload speed in KB/s.
        /// </summary>
        [DataMember(Name = RpcConstants.SpeedLimitUp)]
        public long SpeedLimitUp
        {
            get => _SpeedLimitUp;
            set => SetProperty(ref _SpeedLimitUp, value);
        }

        /// <summary>
        /// If <see langword="true"/>, the download speed will be limited.
        /// </summary>
        [DataMember(Name = RpcConstants.SpeedLimitDownEnabled)]
        public bool SpeedLimitDownEnabled
        {
            get => _SpeedLimitDownEnabled;
            set => SetProperty(ref _SpeedLimitDownEnabled, value);
        }

        /// <summary>
        /// If <see langword="true"/>, the upload speed will be limited.
        /// </summary>
        [DataMember(Name = RpcConstants.SpeedLimitUpEnabled)]
        public bool SpeedLimitUpEnabled
        {
            get => _SpeedLimitUpEnabled;
            set => SetProperty(ref _SpeedLimitUpEnabled, value);
        }

        /// <summary>
        /// Gets the current Transmission version number (build)
        /// </summary>
        [DataMember(Name = RpcConstants.Version)]
        public string Version
        {
            get => _Version;
            set => SetProperty(ref _Version, value);
        }

        #endregion Properties
    }
}
