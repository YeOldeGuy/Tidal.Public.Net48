using System;
using System.Runtime.Serialization;
using Tidal.Client.Constants;

namespace Tidal.Client.Models
{
    /// <summary>
    /// Represents the client's status at a specific moment in time,
    /// giving the number of bytes up and downloaded, as well as the
    /// uptime and session count.
    /// </summary>
    public class TemporalStats : Assignable<TemporalStats>
    {
        #region Backing Store
        private long _UploadedBytes;
        private long _DownloadedBytes;
        private int _SessionCount;
        private int _FilesAdded;
        private long _SecondsActive;
        #endregion

        protected override void AssignInternal(TemporalStats other)
        {
            if (other == null)
                return;

            UploadedBytes = other.UploadedBytes;
            DownloadedBytes = other.DownloadedBytes;
            FilesAdded = other.FilesAdded;
            SessionCount = other.SessionCount;
            SecondsActive = other.SecondsActive;
        }


        /// <summary>
        /// Total number of bytes uploaded, across all torrents, current and
        /// removed.
        /// </summary>
        [DataMember(Name = RpcConstants.UploadedBytes)]
        public long UploadedBytes
        {
            get => _UploadedBytes;
            set => SetProperty(ref _UploadedBytes, value);
        }


        /// <summary>
        /// Total number of bytes downloaded, across all torrents, current and
        /// removed.
        /// </summary>
        [DataMember(Name = RpcConstants.DownloadedBytes)]
        public long DownloadedBytes
        {
            get => _DownloadedBytes;
            set => SetProperty(ref _DownloadedBytes, value);
        }


        /// <summary>
        /// Number of files that have been added to the client since either the
        /// client was installed, or since the last startup.
        /// </summary>
        [DataMember(Name = RpcConstants.FilesAdded)]
        public int FilesAdded
        {
            get => _FilesAdded;
            set => SetProperty(ref _FilesAdded, value);
        }


        /// <summary>
        /// Number of times the client has been started up.
        /// </summary>
        [DataMember(Name = RpcConstants.SessionCount)]
        public int SessionCount
        {
            get => _SessionCount;
            set => SetProperty(ref _SessionCount, value);
        }


        /// <summary>
        /// Amount of time, in seconds, that the client has been running.
        /// </summary>
        [DataMember(Name = RpcConstants.SecondsActive)]
        public long SecondsActive
        {
            get => _SecondsActive;
            set
            {
                if (SetProperty(ref _SecondsActive, value))
                    RaisePropertyChanged(nameof(TimeActive));
            }
        }


        /// <summary>
        /// A <see cref="TimeSpan"/> representation of <see
        /// cref="SecondsActive"/>.
        /// </summary>
        [IgnoreDataMember]
        public TimeSpan TimeActive
        {
            get => TimeSpan.FromSeconds(_SecondsActive);
        }
    }
}
