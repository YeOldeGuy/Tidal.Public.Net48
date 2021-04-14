using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Tidal.Client.Constants;
using Tidal.Core.Helpers;

namespace Tidal.Client.Models
{
    /// <summary>
    /// Represents a torrent being maintained by the Transmission client.
    /// </summary>
    public class Torrent : Assignable<Torrent>, IEquatable<Torrent>
    {
        #region Backing Store
        // These MovingAverate values are only allocated on demand. The torrents
        // that are being retrieved from the client won't need them. One other
        // instance will be the one that is being displayed somewhere; that one
        // gets values assigned to it via Assign(), and will get the averages
        // created there.
        //
        private MovingAverage upAverage;
        private MovingAverage downAverage;

        private int _Id;
        private long _ActivityDateUnitTime;
        private long _AddedDateUnixTime;
        private long _DoneDateUnixTime;
        private long _ETASeconds;
        private int _Error;
        private string _ErrorString;
        private long _RateDownload;
        private long _RateUpload;
        private int _PeerLimit;
        private bool _IsFinished;
        private bool _HonorsSessionLimits;
        private int _SeedRatioModeInt;
        private int _SeedIdleModeInt;
        private double _SeedRatioLimit;
        private int _PeersConnected;
        private double _PercentDone;
        private int _StatusInt;
        private int _SeedIdleLimitMinutes;
        private long _SecondsDownloading;
        private long _DownloadedEver;
        private long _DownloadLimit;
        private bool _DownloadLimited;
        private string _HashString = string.Empty; // gets used in IEquatable methods
        private string _Name;
        private long _TotalSize;
        private IList<int> _Priorities;
        private long _UploadedEver;
        private long _UploadLimit;
        private bool _UploadLimited;
        private double _UploadRatio;
        //private IList<FileSummary> _FileSummaries;
        private long _AverageRateDownload;
        private long _AverageRateUpload;
        #endregion

        protected override void AssignInternal(Torrent other)
        {
            ActivityDateUnixTime = other.ActivityDateUnixTime;
            AddedDateUnixTime = other.AddedDateUnixTime;
            DoneDateUnixTime = other.DoneDateUnixTime;
            DownloadedEver = other.DownloadedEver;
            DownloadLimit = other.DownloadLimit;
            DownloadLimited = other.DownloadLimited;
            Error = other.Error;
            ErrorString = other.ErrorString;
            ETASeconds = other.ETASeconds;
            HashString = other.HashString;
            HonorsSessionLimits = other.HonorsSessionLimits;
            Id = other.Id;
            IsFinished = other.IsFinished;
            Name = other.Name;
            PeersConnected = other.PeersConnected;
            PeerLimit = other.PeerLimit;
            PercentDone = other.PercentDone;
            Priorities = other.Priorities;
            RateDownload = other.RateDownload;
            RateUpload = other.RateUpload;
            SecondsDownloading = other.SecondsDownloading;
            SeedIdleLimitMinutes = other.SeedIdleLimitMinutes;
            SeedIdleModeInt = other.SeedIdleModeInt;
            SeedRatioLimit = other.SeedRatioLimit;
            SeedRatioModeInt = other.SeedRatioModeInt;
            StatusInt = other.StatusInt;
            TotalSize = other.TotalSize;
            UploadedEver = other.UploadedEver;
            UploadLimit = other.UploadLimit;
            UploadLimited = other.UploadLimited;
            UploadRatio = other.UploadRatio;

            Files = other.Files;
            Stats = other.Stats;
            PeersRaw = other.PeersRaw;
            Trackers = other.Trackers;

            // These properties change as a function of the wall clock, so we
            // have to set them off manually:
            RaisePropertyChanged(nameof(TimeSinceActive));
            RaisePropertyChanged(nameof(SeedRatioProgress));

            // These properties are just too weird, so fire them off no matter
            // what.
            RaisePropertyChanged(nameof(SeederCount));
            RaisePropertyChanged(nameof(LeecherCount));

            // Averages:
            if (downAverage == null) downAverage = new MovingAverage();
            if (upAverage == null) upAverage = new MovingAverage();

            AverageRateUpload = (long)upAverage.Push(other.RateUpload);
            AverageRateDownload = (long)downAverage.Push(other.RateDownload);
        }

        #region overrides
        public override int GetHashCode() => HashString.GetHashCode();

        public override string ToString() => $"{Id}:{Name}";

        public bool Equals(Torrent other) => HashString.Equals(other.HashString);

        public override bool Equals(object obj)
        {
            return obj != null && obj is Torrent tor && Equals(tor);
        }
        #endregion


        [DataMember(Name = RpcConstants.DownloadedEver)]
        public long DownloadedEver { get => _DownloadedEver; set => SetProperty(ref _DownloadedEver, value); }

        [DataMember(Name = RpcConstants.DownloadLimit)]
        public long DownloadLimit { get => _DownloadLimit; set => SetProperty(ref _DownloadLimit, value); }

        [DataMember(Name = RpcConstants.DownloadLimited)]
        public bool DownloadLimited { get => _DownloadLimited; set => SetProperty(ref _DownloadLimited, value); }

        [DataMember(Name = RpcConstants.HashString)]
        public string HashString { get => _HashString; set => SetProperty(ref _HashString, value); }

        [DataMember(Name = RpcConstants.Name), Description("Torrent Name")]
        public string Name { get => _Name; set => SetProperty(ref _Name, value); }

        [DataMember(Name = RpcConstants.TotalSize), Description("Total Size")]
        public long TotalSize { get => _TotalSize; set => SetProperty(ref _TotalSize, value); }

        [DataMember(Name = RpcConstants.Priorities)]
        public IList<int> Priorities { get => _Priorities; set => SetProperty(ref _Priorities, value); }

        [DataMember(Name = RpcConstants.UploadedEver)]
        public long UploadedEver { get => _UploadedEver; set => SetProperty(ref _UploadedEver, value); }

        [DataMember(Name = RpcConstants.UploadLimit)]
        public long UploadLimit { get => _UploadLimit; set => SetProperty(ref _UploadLimit, value); }

        [DataMember(Name = RpcConstants.UploadLimited)]
        public bool UploadLimited { get => _UploadLimited; set => SetProperty(ref _UploadLimited, value); }

        [DataMember(Name = RpcConstants.UploadRatio), Description("Upload Ratio")]
        public double UploadRatio
        {
            get => _UploadRatio;
            set
            {
                if (SetProperty(ref _UploadRatio, value))
                {
                    RaisePropertyChanged(nameof(SeedRatioProgress));
                }
            }
        }

        [DataMember(Name = RpcConstants.Id)]
        public int Id { get => _Id; set => SetProperty(ref _Id, value); }

        private static DateTime RawToDate(long unixDate) =>
            DateTimeOffset.FromUnixTimeSeconds(unixDate).ToLocalTime().DateTime;

        [DataMember(Name = RpcConstants.ActivityDate)]
        public long ActivityDateUnixTime
        {
            get => _ActivityDateUnitTime;
            set { if (SetProperty(ref _ActivityDateUnitTime, value)) RaisePropertyChanged(nameof(ActivityDate)); }
        }
        [Description("Activity Date/Time")]
        public DateTime ActivityDate => RawToDate(ActivityDateUnixTime);

        [DataMember(Name = RpcConstants.AddedDate)]
        public long AddedDateUnixTime
        {
            get => _AddedDateUnixTime;
            set { if (SetProperty(ref _AddedDateUnixTime, value)) RaisePropertyChanged(nameof(AddedDate)); }
        }
        public DateTime AddedDate => RawToDate(AddedDateUnixTime);


        [DataMember(Name = RpcConstants.DoneDate)]
        public long DoneDateUnixTime
        {
            get => _DoneDateUnixTime;
            set { if (SetProperty(ref _DoneDateUnixTime, value)) RaisePropertyChanged(nameof(DoneDate)); }
        }
        [Description("Completed Date/Time")]
        public DateTime DoneDate => RawToDate(DoneDateUnixTime);


        [DataMember(Name = RpcConstants.ETA)]
        public long ETASeconds
        {
            get => _ETASeconds;
            set
            {
                if (SetProperty(ref _ETASeconds, value))
                    RaisePropertyChanged(nameof(ETA));
            }
        }
        [Description("Est. Time to Completion")]
        public TimeSpan ETA => TimeSpan.FromSeconds(ETASeconds);


        [DataMember(Name = RpcConstants.SeedIdleLimit)]
        public int SeedIdleLimitMinutes
        {
            get => _SeedIdleLimitMinutes;
            set
            {
                if (SetProperty(ref _SeedIdleLimitMinutes, value))
                    RaisePropertyChanged(nameof(SeedIdleLimit));
            }
        }
        public TimeSpan SeedIdleLimit => TimeSpan.FromMinutes(SeedIdleLimitMinutes);

        [DataMember(Name = RpcConstants.Error)]
        public int Error
        {
            get => _Error;
            set
            {
                if (SetProperty(ref _Error, value))
                {
                    RaisePropertyChanged(nameof(HasError));
                }
            }
        }

        [DataMember(Name = RpcConstants.ErrorString)]
        public string ErrorString
        {
            get => _ErrorString;
            set => SetProperty(ref _ErrorString, value);
        }


        [DataMember(Name = RpcConstants.HonorsSessionLimits), Description("Honors Session Limits")]
        public bool HonorsSessionLimits
        {
            get => _HonorsSessionLimits;
            set => SetProperty(ref _HonorsSessionLimits, value);
        }


        [DataMember(Name = RpcConstants.IsFinished)]
        public bool IsFinished
        {
            get => _IsFinished;
            set => SetProperty(ref _IsFinished, value);
        }


        [DataMember(Name = RpcConstants.PeerLimit), Description("# Peers Limit")]
        public int PeerLimit
        {
            get => _PeerLimit;
            set => SetProperty(ref _PeerLimit, value);
        }


        [DataMember(Name = RpcConstants.PeersConnected), Description("Peers Connected")]
        public int PeersConnected
        {
            get => _PeersConnected;
            set => SetProperty(ref _PeersConnected, value);
        }


        [DataMember(Name = RpcConstants.PercentDone), Description("Percent Completed")]
        public double PercentDone
        {
            get => _PercentDone;
            set => SetProperty(ref _PercentDone, value);
        }


        [DataMember(Name = RpcConstants.RateDownload), Description("Actual Download Rate")]
        public long RateDownload
        {
            get => _RateDownload;
            set => SetProperty(ref _RateDownload, value);
        }


        [DataMember(Name = RpcConstants.RateUpload), Description("Actual Upload Rate")]
        public long RateUpload
        {
            get => _RateUpload;
            set => SetProperty(ref _RateUpload, value);
        }


        /// <summary>
        /// The torrent status as an integer value, the way the client
        /// reports it.
        /// </summary>
        [DataMember(Name = RpcConstants.Status)]
        public int StatusInt
        {
            get => _StatusInt;
            set
            {
                if (SetProperty(ref _StatusInt, value))
                    RaisePropertyChanged(nameof(Status));
            }
        }

        /// <summary>
        /// The torrent status as a <see cref="TorrentStatus"/> value.
        /// </summary>
        public TorrentStatus Status => (TorrentStatus)StatusInt;


        /// <summary>
        /// The number of seconds a torrent has been downloading.
        /// </summary>
        [DataMember(Name = RpcConstants.SecondsDownloading)]
        public long SecondsDownloading
        {
            get => _SecondsDownloading;
            set => SetProperty(ref _SecondsDownloading, value);
        }

        /// <summary>
        /// The elapsed time a torrent has been downloading
        /// </summary>
        [Description("Time Downloading")]
        public TimeSpan TimeDownloading => TimeSpan.FromSeconds(SecondsDownloading);


        /// <summary>
        /// The seed ratio mode as an integer from 0-2.
        /// </summary>
        [DataMember(Name = RpcConstants.SeedRatioMode)]
        public int SeedRatioModeInt
        {
            get => _SeedRatioModeInt;
            set
            {
                if (SetProperty(ref _SeedRatioModeInt, value))
                {
                    RaisePropertyChanged(nameof(SeedRatioProgress));
                    RaisePropertyChanged(nameof(SeedRatioMode));
                }
            }
        }


        /// <summary>
        /// The seed ratio mode as a <see cref="SeedRatioMode"/> value.
        /// </summary>
        [IgnoreDataMember, Description("Seed Ratio Mode")]
        public SeedLimitMode SeedRatioMode
        {
            get => (SeedLimitMode)SeedRatioModeInt;
            set => SeedRatioModeInt = (int)value;
        }


        /// <summary>
        /// The upload/download ratio, that when reached, will cause the
        /// torrent to stop seeding.
        /// </summary>
        [DataMember(Name = RpcConstants.SeedRatioLimit), Description("Seed Ratio Limit")]
        public double SeedRatioLimit
        {
            get => _SeedRatioLimit;
            set
            {
                if (SetProperty(ref _SeedRatioLimit, value))
                    RaisePropertyChanged(nameof(SeedRatioProgress));
            }
        }


        /// <summary>
        /// The seed idle mode as an integer, the way the client provides it.
        /// </summary>
        [DataMember(Name = RpcConstants.SeedIdleMode)]
        public int SeedIdleModeInt
        {
            get => _SeedIdleModeInt;
            set
            {
                if (SetProperty(ref _SeedIdleModeInt, value))
                    RaisePropertyChanged(nameof(SeedIdleMode));
            }
        }


        [IgnoreDataMember, Description("Seed Idle Mode")]
        public SeedLimitMode SeedIdleMode
        {
            get => (SeedLimitMode)SeedIdleModeInt;
            set => SeedIdleModeInt = (int)value;
        }


        [DataMember(Name = RpcConstants.Files)]
        public IList<FileInfo> Files { get; set; }


        [DataMember(Name = RpcConstants.FileStats)]
        public IList<FileStats> Stats { get; set; }


        [DataMember(Name = RpcConstants.Peers)]
        public IList<Peer> PeersRaw { get; set; }


        public IEnumerable<Peer> Peers
        {
            get
            {
                foreach (var peer in PeersRaw)
                {
                    peer.OwnerId = Id;
                    peer.OwnerName = Name;
                }
                return PeersRaw;
            }
        }

        [DataMember(Name = RpcConstants.TrackerStats)]
        public IList<Tracker> Trackers { get; set; }


        #region synthetic properties
        public IEnumerable<FileSummary> FileSummaries
        {
            get
            {
                for (int i = 0; i < Files.Count; i++)
                    yield return new FileSummary(Id, i, Files[i], Stats[i]);
            }
        }

        public bool HasError => Error != 0;

        public TimeSpan TimeSinceActive => DateTime.Now - ActivityDate;

        public double SeedRatioProgress
        {
            get
            {
                double progress = 0.0;

                switch (SeedRatioMode)
                {
                    case SeedLimitMode.FollowGlobalSettings:
                        progress = UploadRatio / (SeedRatioLimit > 0 ? SeedRatioLimit : 1);
                        break;
                    case SeedLimitMode.Unlimited:
                        progress = 1.0;
                        break;
                    case SeedLimitMode.OverrideGlobalSettings:
                        progress = UploadRatio / (SeedRatioLimit > 0 ? SeedRatioLimit : 1);
                        break;
                }
                return progress < 0 ? 0 : progress;
            }
        }

        /// <summary>
        /// Gets the number of seeders being reported by the different trackers.
        /// </summary>
        public int SeederCount
        {
            get
            {
                if (Trackers == null || Trackers.Count <= 0)
                    return 0;

                int count = 0;
                foreach (var tracker in Trackers)
                    count += tracker.IsBackup ? 0 : tracker.SeederCount;
                return count < 0 ? 0 : count;
            }
        }

        /// <summary>
        /// Gets the number of leechers (people who don't have the 100% yet)
        /// as being reported by the trackers.
        /// </summary>
        public int LeecherCount
        {
            get
            {
                if (Trackers == null || Trackers.Count <= 0)
                    return 0;

                int count = 0;
                foreach (var tracker in Trackers)
                    count += tracker.IsBackup ? 0 : tracker.LeecherCount;
                return count < 0 ? 0 : count;
            }
        }

        /// <summary>
        /// Gets the number of uploaded bytes necessary to fulfill the seeding
        /// requirements. If the torrent is 250MB in size and the seed ratio
        /// limit is 2.0, then this will return 500MB.
        /// </summary>
        public long TargetDownloadAmount
        {
            get
            {
                switch (SeedRatioMode)
                {
                    case SeedLimitMode.FollowGlobalSettings:
                    case SeedLimitMode.OverrideGlobalSettings:
                        return (long)(TotalSize * (SeedRatioLimit > 0 ? SeedRatioLimit : 1));
                    default:
                        return TotalSize;
                }
            }
        }

        /// <summary>
        /// Gets the status of the torrent, but tries to be smart about
        /// it and figure out what's really happening, as opposed to the
        /// Torrent.Status property.
        /// </summary>
        public TorrentState TorrentAction
        {
            get
            {
                TorrentState t = 0;

                if (Error != 0)
                    t = TorrentState.Error;

                switch (Status)
                {
                    case TorrentStatus.Stopped:
                        if (IsFinished == false)
                            t |= TorrentState.Paused | TorrentState.Inactive;
                        else
                            t |= TorrentState.Stopped | TorrentState.Inactive | TorrentState.Completed;
                        break;
                    case TorrentStatus.Downloading:
                        if (PeersConnected > 0 && (RateDownload + RateUpload > 0))
                            t |= TorrentState.Active | TorrentState.Downloading;
                        else
                            t |= TorrentState.Inactive | TorrentState.Downloading;
                        break;
                    case TorrentStatus.Seeding:
                        if (PeersConnected > 0 && RateUpload > 0)
                            t |= TorrentState.Active | TorrentState.Seeding;
                        else
                            t |= TorrentState.Inactive | TorrentState.Seeding;
                        break;
                    case TorrentStatus.Checking:
                    case TorrentStatus.CheckWait:
                        t |= TorrentState.Active | TorrentState.Checking;
                        break;
                    case TorrentStatus.Queued:
                    case TorrentStatus.QueuedToSeed:
                        t |= TorrentState.Inactive | TorrentState.Waiting;
                        break;
                }
                return t;
            }
        }


        [IgnoreDataMember, Description("Average download rate")]
        public long AverageRateDownload
        {
            get => _AverageRateDownload == 0 && RateDownload > 0 ? RateDownload : _AverageRateDownload;
            set => SetProperty(ref _AverageRateDownload, value);
        }

        [IgnoreDataMember, Description("Average upload rate")]
        public long AverageRateUpload
        {
            get => _AverageRateUpload == 0 && RateUpload > 0 ? RateUpload : _AverageRateUpload;
            set => SetProperty(ref _AverageRateUpload, value);
        }

        #endregion

    }
}
