using System.Runtime.Serialization;
using Tidal.Client.Constants;

namespace Tidal.Client.Models
{
    /// <summary>
    /// Represents one of a torrent's trackers.
    /// </summary>
    public class Tracker : Assignable<Tracker>
    {
        private string _Host;
        private int _Id;
        private bool _IsBackup;
        private int _DownloadCount;
        private int _LeecherCount;
        private int _SeederCount;

        protected override void AssignInternal(Tracker other)
        {
            Host = other.Host;
            Id = other.Id;
            IsBackup = other.IsBackup;
            DownloadCount = other.DownloadCount;
            LeecherCount = other.LeecherCount;
            SeederCount = other.SeederCount;
        }


        /// <summary>
        /// The tracker's name, a host URI, normally.
        /// </summary>
        [DataMember(Name = RpcConstants.Host)]
        public string Host
        {
            get => _Host;
            set => SetProperty(ref _Host, value);
        }


        /// <summary>
        /// A client-assigned ID number for the tracker
        /// </summary>
        [DataMember(Name = RpcConstants.Id)]
        public int Id
        {
            get => _Id;
            set => SetProperty(ref _Id, value);
        }


        /// <summary>
        /// Is this a backup tracker?
        /// </summary>
        [DataMember(Name = RpcConstants.IsBackup)]
        public bool IsBackup
        {
            get => _IsBackup;
            set => SetProperty(ref _IsBackup, value);
        }


        /// <summary>
        /// Total number of downloads on this tracker
        /// </summary>
        [DataMember(Name = RpcConstants.DownloadCount)]
        public int DownloadCount
        {
            get => _DownloadCount;
            set => SetProperty(ref _DownloadCount, value);
        }


        /// <summary>
        /// Current number of incomplete downloads for the torrent that this
        /// tracker is for.
        /// </summary>
        [DataMember(Name = RpcConstants.LeecherCount)]
        public int LeecherCount
        {
            get => _LeecherCount;
            set => SetProperty(ref _LeecherCount, value);
        }


        /// <summary>
        /// Current number of seeding peers with completed downloads for this
        /// tracker's torrent.
        /// </summary>
        [DataMember(Name = RpcConstants.SeederCount)]
        public int SeederCount
        {
            get => _SeederCount;
            set => SetProperty(ref _SeederCount, value);
        }
    }
}
