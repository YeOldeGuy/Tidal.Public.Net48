﻿using System;
using System.Runtime.Serialization;
using Tidal.Client.Constants;

namespace Tidal.Client.Models
{
    /// <summary>
    /// A class for changing <see cref="Session"/> values on the host. This is
    /// much the same as<see cref="Session"/>, but with nullable values and
    /// some elements elided.
    /// </summary>
    /// <remarks>
    /// To change a session value on the host, the app psses in a JSON encoded
    /// object consisting of just the value you want changed. An example of the
    /// JSON that a request to change the encryption setting on the host would
    /// look like:
    /// <code>
    /// {"arguments":{"encryption":"tolerated"},"method":"session-set","tag":24}
    /// </code>
    /// The only arguments passed in the JSON are the ones that are to be
    /// changed. It is <b>not</b> an error to change something to the same value
    /// it already has. So, in the example above, if the host already had the
    /// <c>tolerated</c> value for <c>encryption</c>, the host would silently
    /// ignore the request.
    /// </remarks>
    public class SessionMutator : MutatorBase
    {
        /// <summary>
        /// Create a <see cref="SessionMutator"/> with all properties
        /// set to <see langword="null"/>.
        /// </summary>
        public SessionMutator() { }

        /// <summary>
        /// Create a <see cref="SessionMutator"/>, with only the specified
        /// property set to the given value.
        /// </summary>
        /// <remarks>
        /// The normal mode of operation is to just set one value at a
        /// time on the host. This facilitates that.
        /// </remarks>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public SessionMutator(string propertyName, object value)
        {
            SetProperty(propertyName, value);
        }


        #region Backing Store
        private long? _AltScheduleBeginMinutes;
        private long? _AltScheduleEndMinutes;
        private bool? _AltSpeedEnabled;
        private long? _AltSpeedDown;
        private long? _AltSpeedUp;
        private bool? _AltSpeedTimeEnabled;
        private int? _AltSpeedTimeDays;
        private string _DownloadDirectory;
        private int? _DownloadQueueSize;
        private bool? _DownloadQueueEnabled;
        private string _IncompleteDirectory;
        private bool? _IncompleteDirectoryEnabled;
        private bool? _RenamePartialFiles;
        private bool? _DHTEnabled;
        private bool? _LPDEnabled;
        private bool? _PEXEnabled;
        private bool? _UTPEnabled;
        private bool? _IdleSeedingLimitEnabled;
        private double? _SeedRatioLimit;
        private bool? _SeedRatioLimitEnabled;
        private int? _PeerLimitGlobal;
        private int? _PeerLimitPerTorrent;
        private int? _PeerPort;
        private bool? _PeerPortRandomize;
        private long? _SpeedLimitDown;
        private long? _SpeedLimitUp;
        private bool? _SpeedLimitDownEnabled;
        private bool? _SpeedLimitUpEnabled;
        #endregion

        #region Alt Speed Properties
        /// <summary>
        /// Are the Alternate speed settings currently in use?
        /// </summary>
        [DataMember(Name = RpcConstants.AltSpeedEnabled)]
        public bool? AltSpeedEnabled
        {
            get => _AltSpeedEnabled;
            set => SetProperty(ref _AltSpeedEnabled, value);
        }


        /// <summary>
        /// If the alternative speed settings are in use, this is the maximum
        /// global speed of downloads, expressed as KBps (50 would be 50KBps)
        /// </summary>
        [DataMember(Name = RpcConstants.AltSpeedDown)]
        public long? AltSpeedDown
        {
            get => _AltSpeedDown;
            set => SetProperty(ref _AltSpeedDown, value);
        }

        /// <summary>
        /// If the alternative speed settings are in use, this is the maximum
        /// global speed of uploads, expressed as KBps (50 would be 50KBps)
        /// </summary>
        [DataMember(Name = RpcConstants.AltSpeedUp)]
        public long? AltSpeedUp
        {
            get => _AltSpeedUp;
            set => SetProperty(ref _AltSpeedUp, value);
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
        public long? AltScheduleBeginMinutes
        {
            get => _AltScheduleBeginMinutes;
            set
            {
                if (SetProperty(ref _AltScheduleBeginMinutes, value))
                    RaisePropertyChanged(nameof(AltScheduleBegin));
            }
        }


        /// <summary>
        /// A processed representation of the <see cref="AltScheduleBeginMinutes"/>
        /// value, expressed as a <see cref="TimeSpan"/>.
        /// </summary>
        [IgnoreDataMember]
        public TimeSpan? AltScheduleBegin
        {
            get => TimeSpan.FromMinutes(AltScheduleBeginMinutes.GetValueOrDefault());
            set => AltScheduleBeginMinutes = (long)value.GetValueOrDefault().TotalMinutes;
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
        public long? AltScheduleEndMinutes
        {
            get => _AltScheduleEndMinutes;
            set
            {
                if (SetProperty(ref _AltScheduleEndMinutes, value))
                    RaisePropertyChanged(nameof(AltScheduleEnd));
            }
        }


        /// <summary>
        /// A processed representation of the <see cref="AltScheduleEndMinutes"/>
        /// value, expressed as a time after the current date's midnight.
        /// </summary>
        [IgnoreDataMember]
        public TimeSpan? AltScheduleEnd
        {
            get => TimeSpan.FromMinutes(AltScheduleEndMinutes.GetValueOrDefault());
            set => AltScheduleEndMinutes = (long)value.GetValueOrDefault().TotalMinutes;
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
        public bool? AltScheduleEnabled
        {
            get => _AltSpeedTimeEnabled;
            set => SetProperty(ref _AltSpeedTimeEnabled, value);
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
        public int? AltScheduleDays
        {
            get => _AltSpeedTimeDays;
            set => SetProperty(ref _AltSpeedTimeDays, value);
        }
        #endregion

        #region Download Properties
        /// <summary>
        /// The directory that completed torrents are stored in <b>on the host
        /// computer</b>.
        /// </summary>
        /// <remarks>
        /// Do not change this unless you know what the fuck you're doing.
        /// Setting an unreachable directory here will cause a cascade of
        /// errors.
        /// </remarks>
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
        public int? DownloadQueueSize
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
        public bool? DownloadQueueEnabled
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
        public bool? IncompleteDirectoryEnabled
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
        public bool? RenamePartialFiles
        {
            get => _RenamePartialFiles;
            set => SetProperty(ref _RenamePartialFiles, value);
        }
        #endregion

        #region Protocol Settings
        /// <summary>
        /// I haven't the foggiest what it does, but if it's <see
        /// langword="true"/>, then DHT will be allowed in public torrents.
        /// </summary>
        [DataMember(Name = RpcConstants.DHTEnabled)]
        public bool? DHTEnabled
        {
            get => _DHTEnabled;
            set => SetProperty(ref _DHTEnabled, value);
        }


        /// <summary>
        /// Local Peer Discovery on/off.
        /// </summary>
        [DataMember(Name = RpcConstants.LPDEnabled)]
        public bool? LPDEnabled
        {
            get => _LPDEnabled;
            set => SetProperty(ref _LPDEnabled, value);
        }


        /// <summary>
        /// I haven't the foggiest what it does, but if it's <see
        /// langword="true"/>, then PEX will be allowed in public torrents.
        /// </summary>
        [DataMember(Name = RpcConstants.PEXEnabled)]
        public bool? PEXEnabled
        {
            get => _PEXEnabled;
            set => SetProperty(ref _PEXEnabled, value);
        }


        /// <summary>
        /// I haven't the foggiest what it does, but if it's <see
        /// langword="true"/>, then UTP will be allowed in public torrents.
        /// </summary>
        [DataMember(Name = RpcConstants.UTPEnabled)]
        public bool? UTPEnabled
        {
            get => _UTPEnabled;
            set => SetProperty(ref _UTPEnabled, value);
        }

        #endregion

        #region Encryption
        private string _EncString;
        /// <summary>
        /// The string representing the encryption level as presented by
        /// the client.
        /// </summary>
        [DataMember(Name = RpcConstants.Encryption)]
        public string EncString
        {
            get => _EncString;
            set
            {
                if (SetProperty(ref _EncString, value))
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
                switch (EncString)
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
                        EncString = RpcConstants.EncryptionPreferred;
                        break;
                    case EncryptionLevel.Required:
                        EncString = RpcConstants.EncryptionRequired;
                        break;
                    default:
                        EncString = RpcConstants.EncryptionTolerated;
                        break;
                }
            }
        }
        #endregion

        #region Seeding Limits
        private long? _IdleSeedingLimitMinutes;
        /// <summary>
        /// The value that a seeding torrent can idle, not uploading to a peer
        /// before the seeding is stopped.
        /// </summary>
        /// <remarks>
        /// This is the value as presented by the host, in the number of
        /// minutes.
        /// </remarks>
        [DataMember(Name = RpcConstants.IdleSeedingLimit)]
        public long? IdleSeedingLimitMinutes
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
        public TimeSpan? IdleSeedingLimit
        {
            get => TimeSpan.FromMinutes(IdleSeedingLimitMinutes.GetValueOrDefault());
            set => IdleSeedingLimitMinutes = (long)value.GetValueOrDefault().TotalMinutes;
        }


        /// <summary>
        /// If <see langword="true"/>, then torrents will be monitored for idle
        /// time and shutdown if the <see cref="IdleSeedingLimit"/> is reached.
        /// </summary>
        [DataMember(Name = RpcConstants.IdleSeedingLimitEnabled)]
        public bool? IdleSeedingLimitEnabled
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
        public double? SeedRatioLimit
        {
            get => _SeedRatioLimit;
            set => SetProperty(ref _SeedRatioLimit, value);
        }


        /// <summary>
        /// If <see langword="true"/>, the <see cref="SeedRatioLimit"/> will be
        /// honored.
        /// </summary>
        [DataMember(Name = RpcConstants.SeedRatioLimited)]
        public bool? SeedRatioLimited
        {
            get => _SeedRatioLimitEnabled;
            set => SetProperty(ref _SeedRatioLimitEnabled, value);
        }
        #endregion

        #region Peer limits and ports
        /// <summary>
        /// The maximum number of peers the client will allow, spread across all
        /// active torrents.
        /// </summary>
        [DataMember(Name = RpcConstants.PeerLimitGlobal)]
        public int? PeerLimitGlobal
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
        public int? PeerLimitPerTorrent
        {
            get => _PeerLimitPerTorrent;
            set => SetProperty(ref _PeerLimitPerTorrent, value);
        }


        /// <summary>
        /// Gets the port number that peers may connect to the host on. This
        /// used to be 6881-6889, but those are blocked by many ISPs, and have
        /// fallen from favor.
        /// </summary>
        /// <remarks>
        /// Default value as this is written is 51413.
        /// </remarks>
        [DataMember(Name = RpcConstants.PeerPort)]
        public int? PeerPort
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
        public bool? PeerPortRandomize
        {
            get => _PeerPortRandomize;
            set => SetProperty(ref _PeerPortRandomize, value);
        }

        #endregion

        #region Speed Limits
        /// <summary>
        /// If <see cref="SpeedLimitDownEnabled"/> is <see langword="true"/>, then
        /// this represents the global, non-alt-mode download speed in KB/s.
        /// </summary>
        [DataMember(Name = RpcConstants.SpeedLimitDown)]
        public long? SpeedLimitDown
        {
            get => _SpeedLimitDown;
            set => SetProperty(ref _SpeedLimitDown, value);
        }


        /// <summary>
        /// If <see cref="SpeedLimitUpEnabled"/> is <see langword="true"/>, then
        /// this represents the global, non-alt-mode upload speed in KB/s.
        /// </summary>
        [DataMember(Name = RpcConstants.SpeedLimitUp)]
        public long? SpeedLimitUp
        {
            get => _SpeedLimitUp;
            set => SetProperty(ref _SpeedLimitUp, value);
        }


        /// <summary>
        /// If <see langword="true"/>, the download speed will be limited.
        /// </summary>
        [DataMember(Name = RpcConstants.SpeedLimitDownEnabled)]
        public bool? SpeedLimitDownEnabled
        {
            get => _SpeedLimitDownEnabled;
            set => SetProperty(ref _SpeedLimitDownEnabled, value);
        }


        /// <summary>
        /// If <see langword="true"/>, the upload speed will be limited.
        /// </summary>
        [DataMember(Name = RpcConstants.SpeedLimitUpEnabled)]
        public bool? SpeedLimitUpEnabled
        {
            get => _SpeedLimitUpEnabled;
            set => SetProperty(ref _SpeedLimitUpEnabled, value);
        }

        #endregion
    }
}
