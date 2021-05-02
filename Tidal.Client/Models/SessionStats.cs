using System.Runtime.Serialization;
using Tidal.Client.Constants;
using Tidal.Core.Helpers;

namespace Tidal.Client.Models
{
    public class SessionStats : Assignable<SessionStats>
    {
        private MovingAverage downAverage;
        private MovingAverage upAverage;
        private int _ActiveTorrentCount;
        private long _UploadSpeed;
        private long _DownloadSpeed;
        private int _TorrentCount;
        private int _PausedTorrentCount;
        private long _AverageDownloadSpeed;
        private long _AverageUploadSpeed;
        private TemporalStats _CurrentStats;
        private TemporalStats _CumulativeStats;

        #region assign
        protected override void AssignInternal(SessionStats other)
        {
            if (other == null)
                return;

            ActiveTorrentCount = other.ActiveTorrentCount;
            DownloadSpeed = other.DownloadSpeed;
            PausedTorrentCount = other.PausedTorrentCount;
            TorrentCount = other.TorrentCount;
            UploadSpeed = other.UploadSpeed;

            if (CurrentStats == null)
                CurrentStats = new TemporalStats();
            if (CumulativeStats == null)
                CumulativeStats = new TemporalStats();

            CurrentStats.Assign(other.CurrentStats);
            CumulativeStats.Assign(other.CumulativeStats);

            if (downAverage == null) downAverage = new MovingAverage();
            if (upAverage == null) upAverage = new MovingAverage();

            AverageDownloadSpeed = (long)downAverage.Push(other.DownloadSpeed);
            AverageUploadSpeed = (long)upAverage.Push(other.UploadSpeed);
        }
        #endregion assign

        /// <summary>
        /// A <see cref="MovingAverage"/> of the <see cref="DownloadSpeed"/>.
        /// </summary>
        [IgnoreDataMember]
        public long AverageDownloadSpeed
        {
            get => _AverageDownloadSpeed;
            set => SetProperty(ref _AverageDownloadSpeed, value);
        }

        /// <summary>
        /// A <see cref="MovingAverage"/> of the <see cref="UploadSpeed"/>.
        /// </summary>
        [IgnoreDataMember]
        public long AverageUploadSpeed
        {
            get => _AverageUploadSpeed;
            set => SetProperty(ref _AverageUploadSpeed, value);
        }

        /// <summary>
        /// Current number of torrents ready to up or download. Torrents
        /// that are stopped are not reported.
        /// </summary>
        [DataMember(Name = RpcConstants.ActiveTorrentCount)]
        public int ActiveTorrentCount
        {
            get => _ActiveTorrentCount;
            set => SetProperty(ref _ActiveTorrentCount, value);
        }

        /// <summary>
        /// Current overall download speed for the client.
        /// </summary>
        [DataMember(Name = RpcConstants.DownloadSpeed)]
        public long DownloadSpeed
        {
            get => _DownloadSpeed;
            set => SetProperty(ref _DownloadSpeed, value);
        }

        /// <summary>
        /// Current overall upload speed for the client.
        /// </summary>
        [DataMember(Name = RpcConstants.UploadSpeed)]
        public long UploadSpeed
        {
            get => _UploadSpeed;
            set => SetProperty(ref _UploadSpeed, value);
        }

        /// <summary>
        /// Current number of torrents being maintained by the client, active
        /// or not.
        /// </summary>
        [DataMember(Name = RpcConstants.TorrentCount)]
        public int TorrentCount
        {
            get => _TorrentCount;
            set => SetProperty(ref _TorrentCount, value);
        }

        /// <summary>
        /// Number of paused (stopped) torrents in the client.
        /// </summary>
        [DataMember(Name = RpcConstants.PausedTorrentCount)]
        public int PausedTorrentCount
        {
            get => _PausedTorrentCount;
            set => SetProperty(ref _PausedTorrentCount, value);
        }

        /// <summary>
        /// The <see cref="TemporalStats"/> of the client since the last
        /// startup.
        /// </summary>
        [DataMember(Name = RpcConstants.CurrentStats)]
        public TemporalStats CurrentStats
        {
            get => _CurrentStats;
            set => SetProperty(ref _CurrentStats, value);
        }

        /// <summary>
        /// The <see cref="TemporalStats"/> of the client since the client was
        /// installed.
        /// </summary>
        [DataMember(Name = RpcConstants.CumulativeStats)]
        public TemporalStats CumulativeStats
        {
            get => _CumulativeStats;
            set => SetProperty(ref _CumulativeStats, value);
        }
    }
}
