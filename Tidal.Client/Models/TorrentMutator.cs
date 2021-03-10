using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Tidal.Client.Constants;
using Tidal.Client.Contracts;

namespace Tidal.Client.Models
{
    /// <summary>
    /// Encapsulates items requested to change for a set of <see cref="Torrent"/>s
    /// maintained by the host.
    /// </summary>
    public class TorrentMutator : MutatorBase, IAssignable<TorrentMutator>, IAssignable<Torrent>, IIsChanged
    {
        #region Backing Store
        private bool _IsChanged;
        private BandwidthPriority? _BandwidthPriority;
        private long? _DownloadLimit;
        private bool? _DownloadLimited;
        private IList<int> _FilesWanted;
        private IList<int> _FilesUnwanted;
        private bool? _HonorsSessionLimits;
        private IList<int> _Ids;
        private string _Location;
        private int? _PeerLimit;
        private IList<int> _PriorityHigh;
        private IList<int> _PriorityNormal;
        private IList<int> _PriorityLow;
        private int? _SeedIdleMode;
        private int? _SeedRatioMode;
        private long? _SeedIdleLimit;
        private double? _SeedRatioLimit;
        private long? _UploadLimit;
        private bool? _UploadLimited;
        #endregion

        /// <summary>
        /// Create an all-null <see cref="TorrentMutator"/>, suitable for
        /// passing to the client in an RPC. Set values as appropriate.
        /// </summary>
        public TorrentMutator() { }

        /// <summary>
        /// A constructor that sets one property in the <see cref="TorrentMutator"/>.
        /// </summary>
        /// <param name="propertyName">The property name to set.</param>
        /// <param name="value">The value to set the property to.</param>
        public TorrentMutator(string propertyName, object value)
        {
            SetProperty(propertyName, value);
        }


        public void Assign(Torrent tor)
        {
            // These are the common properties between the mutator and the
            // torrent. There are more if you look in the rpc-spec.txt, but
            // these are the only ones I have defined right now, the limits
            // coming from the Torrent.

            DownloadLimit = tor.DownloadLimit;
            DownloadLimited = tor.DownloadLimited;
            HonorsSessionLimits = tor.HonorsSessionLimits;
            PeerLimit = tor.PeerLimit;
            SeedIdleLimit = tor.SeedIdleLimit;
            SeedIdleMode = tor.SeedIdleMode;
            SeedRatioLimit = tor.SeedRatioLimit;
            SeedRatioMode = tor.SeedRatioMode;
            UploadLimit = tor.UploadLimit;
            UploadLimited = tor.UploadLimited;

            IsChanged = false;
        }

        public void Assign(TorrentMutator other)
        {
            BandwidthPriority = other.BandwidthPriority;
            DownloadLimit = other.DownloadLimit;
            DownloadLimited = other.DownloadLimited;
            PeerLimit = other.PeerLimit;
            HonorsSessionLimits = other.HonorsSessionLimits;
            Location = other.Location;

            SeedIdleLimit = other.SeedIdleLimit;
            SeedRatioLimit = other.SeedRatioLimit;
            SeedIdleMode = other.SeedIdleMode;
            SeedRatioMode = other.SeedRatioMode;
            UploadLimit = other.UploadLimit;
            UploadLimited = other.UploadLimited;

            FilesWanted = other.FilesWanted;
            FilesUnwanted = other.FilesUnwanted;
            PriorityHigh = other.PriorityHigh;
            PriorityNormal = other.PriorityNormal;
            PriorityLow = other.PriorityLow;

            Ids = other.Ids;

            IsChanged = false;
        }

        [IgnoreDataMember]
        public bool IsChanged
        {
            get => _IsChanged;
            set => SetProperty(ref _IsChanged, value);
        }


        /// <summary>
        /// Set the <see cref="BandwidthPriority"/> of the <see cref="Torrent"/>.
        /// </summary>
        [DataMember(Name = RpcConstants.BandwidthPriority)]
        public BandwidthPriority? BandwidthPriority
        {
            get => _BandwidthPriority;
            set => SetProperty(ref _BandwidthPriority, value);
        }


        /// <summary>
        /// Change the maximum download speed a torrent can achieve.
        /// </summary>
        [DataMember(Name = RpcConstants.DownloadLimit)]
        public long? DownloadLimit
        {
            get => _DownloadLimit;
            set => SetProperty(ref _DownloadLimit, value);
        }


        /// <summary>
        /// Is the download speed of this <see cref="Torrent"/> limited?
        /// </summary>
        [DataMember(Name = RpcConstants.DownloadLimited)]
        public bool? DownloadLimited
        {
            get => _DownloadLimited;
            set => SetProperty(ref _DownloadLimited, value);
        }


        /// <summary>
        /// The index numbers of the files in the torrent that should be
        /// downloaded. Zero-based, naturally, and the index values are
        /// derived from the order listed in <see cref="Torrent.Files"/>.
        /// </summary>
        [DataMember(Name = RpcConstants.FilesWanted)]
        public IList<int> FilesWanted
        {
            get => _FilesWanted;
            set => SetProperty(ref _FilesWanted, value);
        }


        /// <summary>
        /// The index numbers of the files in the torrent that should <b>not</b>
        /// be downloaded. Zero-based, naturally, and the index values are
        /// derived from the order listed in <see cref="Torrent.Files"/>.
        /// </summary>
        [DataMember(Name = RpcConstants.FilesUnwanted)]
        public IList<int> FilesUnwanted
        {
            get => _FilesUnwanted;
            set => SetProperty(ref _FilesUnwanted, value);
        }


        /// <summary>
        /// If <see langword="true"/>, then the <see cref="Torrent"/> will pay
        /// attention to system-wide limits.
        /// </summary>
        /// <remarks>
        /// For example, setting a maximum download speed higher than the system
        /// download limit will be as if that value is capped. Set this to <see
        /// langword="false"/> and even the alternative mode will have no effect
        /// on the download speed.
        /// </remarks>
        [DataMember(Name = RpcConstants.HonorsSessionLimits)]
        public bool? HonorsSessionLimits
        {
            get => _HonorsSessionLimits;
            set => SetProperty(ref _HonorsSessionLimits, value);
        }


        /// <summary>
        /// A list of <see cref="Torrent.Id"/> values that the changes within
        /// this mutator will effect.
        /// </summary>
        [DataMember(Name = RpcConstants.Ids)]
        public IList<int> Ids
        {
            get => _Ids;
            set => SetProperty(ref _Ids, value);
        }


        /// <summary>
        /// The directory that the torrent will be placed in when completed.
        /// Modify this to your peril.
        /// </summary>
        [DataMember(Name = RpcConstants.Location)]
        public string Location
        {
            get => _Location;
            set => SetProperty(ref _Location, value);
        }


        /// <summary>
        /// Sets the maximum number of peers that can be connected to the
        /// <see cref="Torrent"/>. 
        /// </summary>
        /// <seealso cref="HonorsSessionLimits"/>
        [DataMember(Name = RpcConstants.PeerLimit)]
        public int? PeerLimit
        {
            get => _PeerLimit;
            set => SetProperty(ref _PeerLimit, value);
        }


        /// <summary>
        /// A list of <see cref="Torrent.Id"/> values to set to <see
        /// cref="BandwidthPriority.High"/>
        /// </summary>
        [DataMember(Name = RpcConstants.PriorityHigh)]
        public IList<int> PriorityHigh
        {
            get => _PriorityHigh;
            set => SetProperty(ref _PriorityHigh, value);
        }


        /// <summary>
        /// A list of <see cref="Torrent.Id"/> values to set to <see
        /// cref="BandwidthPriority.Normal"/>
        /// </summary>
        [DataMember(Name = RpcConstants.PriorityNormal)]
        public IList<int> PriorityNormal
        {
            get => _PriorityNormal;
            set => SetProperty(ref _PriorityNormal, value);
        }


        /// <summary>
        /// A list of <see cref="Torrent.Id"/> values to set to <see
        /// cref="BandwidthPriority.Low"/>
        /// </summary>
        [DataMember(Name = RpcConstants.PriorityLow)]
        public IList<int> PriorityLow
        {
            get => _PriorityLow;
            set => SetProperty(ref _PriorityLow, value);
        }


        /// <summary>
        /// The value used by the client to set the <see cref="SeedIdleMode"/>
        /// of the torrent, as it balks at enumerations.
        /// </summary>
        [DataMember(Name = RpcConstants.SeedIdleMode)]
        public int? SeedIdleModeRaw
        {
            get => _SeedIdleMode;
            set => SetProperty(ref _SeedIdleMode, value);
        }


        /// <summary>
        /// Convenience property for settings the <see cref="SeedLimitMode"/>
        /// of the <see cref="Torrent"/> for seed idle time.
        /// </summary>
        [IgnoreDataMember]
        public SeedLimitMode? SeedIdleMode
        {
            get => (SeedLimitMode)SeedIdleModeRaw.GetValueOrDefault();
            set => SeedIdleModeRaw = (int)value.GetValueOrDefault();
        }


        [DataMember(Name = RpcConstants.SeedRatioMode)]
        public int? SeedRatioModeRaw
        {
            get => _SeedRatioMode;
            set => SetProperty(ref _SeedRatioMode, value);
        }


        /// <summary>
        /// Set the <see cref="SeedLimitMode"/> for the seed ratio.
        /// </summary>
        [IgnoreDataMember]
        public SeedLimitMode? SeedRatioMode
        {
            get => (SeedLimitMode)SeedRatioModeRaw.GetValueOrDefault();
            set => SeedRatioModeRaw = (int)value.GetValueOrDefault();
        }


        [DataMember(Name = RpcConstants.SeedIdleLimit)]
        public long? SeedIdleLimitRaw
        {
            get => _SeedIdleLimit;
            set => SetProperty(ref _SeedIdleLimit, value);
        }


        /// <summary>
        /// The time that a seeding torrent can sit idle before being paused.
        /// </summary>
        [IgnoreDataMember]
        public TimeSpan? SeedIdleLimit
        {
            get => TimeSpan.FromMinutes(SeedIdleLimitRaw.GetValueOrDefault());
            set => SeedIdleLimitRaw = (long)value.GetValueOrDefault().TotalMinutes;
        }


        /// <summary>
        /// The seed ratio (amount uploaded / amount downloaded) that when reached
        /// will makke the torrent automatically pause.
        /// </summary>
        [DataMember(Name = RpcConstants.SeedRatioLimit)]
        public double? SeedRatioLimit
        {
            get => _SeedRatioLimit;
            set => SetProperty(ref _SeedRatioLimit, value);
        }


        /// <summary>
        /// Maximum upload speed.
        /// </summary>
        [DataMember(Name = RpcConstants.UploadLimit)]
        public long? UploadLimit
        {
            get => _UploadLimit;
            set => SetProperty(ref _UploadLimit, value);
        }


        /// <summary>
        /// If <see langword="true"/>, the torrent will not exceed the <see
        /// cref="UploadLimit"/> value.
        /// </summary>
        [DataMember(Name = RpcConstants.UploadLimited)]
        public bool? UploadLimited
        {
            get => _UploadLimited;
            set => SetProperty(ref _UploadLimited, value);
        }
    }
}
