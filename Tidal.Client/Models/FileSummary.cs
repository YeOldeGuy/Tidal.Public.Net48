using System;
using System.ComponentModel;

namespace Tidal.Client.Models
{
    /// <summary>
    /// An amalgam of the <see cref="FileInfo"/> and <see cref="FileStat"/>
    /// types, making it easier to deal with.
    /// </summary>
    public class FileSummary : Assignable<FileSummary>, IEquatable<FileSummary>
    {
        private int hashCode;
        private int _OwnerId;
        private long _BytesCompleted;
        private long _Length;
        private string _Name;
        private string _FullName;
        private bool _Wanted;
        private BandwidthPriority _Priority;
        private double _Progress;

        public FileSummary()
        {
        }

        /// <summary>
        /// Create a <see cref="FileSummary"/> instance from the data in
        /// <paramref name="info"/> and <paramref name="stat"/>.
        /// </summary>
        /// <param name="ownerId">The <see cref="Torrent.Id"/> of the owner.</param>
        /// <param name="index">The offset of this file in the array of files in the torrent.</param>
        /// <param name="info">The <see cref="FileInfo"/> to use.</param>
        /// <param name="stat">The <see cref="FileStat"/> data to use.</param>
        public FileSummary(int ownerId, int index, FileInfo info, FileStats stat)
        {
            Index = index;
            if (info != null)
            {
                BytesCompleted = info.BytesCompleted;
                Length = info.Length;
                FullName = info.Name;
                Name = info.Name.Substring(info.Name.LastIndexOf('/') + 1);
                Progress = info.PercentDone;
            }
            if (stat != null)
            {
                Wanted = stat.Wanted;
                Priority = stat.Priority;
            }

            OwnerId = ownerId;
            hashCode = $"{ownerId}:{Name}".GetHashCode();
        }

        protected override void AssignInternal(FileSummary other)
        {
            if (other == null)
                return;

            OwnerId = other.OwnerId;
            Name = other.Name;
            Index = other.Index;

            BytesCompleted = other.BytesCompleted;
            Wanted = other.Wanted;
            Priority = other.Priority;
            Length = other.Length;
            Progress = (double)other.BytesCompleted / other.Length;

            hashCode = other.hashCode;
        }

        #region IEquatable
        public bool Equals(FileSummary other) => GetHashCode().Equals(other.GetHashCode());

        public override bool Equals(object obj) => obj is null || (obj is FileSummary fs && Equals(fs));

        public override int GetHashCode() => hashCode;
        #endregion IEquatable

        /// <summary>
        /// The value of <see cref="Torrent.Id"/> of the owning torrent.
        /// </summary>
        public int OwnerId
        {
            get => _OwnerId; set => SetProperty(ref _OwnerId, value);
        }

        /// <summary>
        /// This file's offset into the array of <see cref="FileInfo"/> values.
        /// Needed for modifying priorities or wanted status.
        /// </summary>
        public int Index
        {
            get; set;
        }

        /// <summary>
        /// The number of bytes downloaded so far. Equal to <see cref="Length"/> if
        /// the file is complete.
        /// </summary>
        [Description("Bytes Completed")]
        public long BytesCompleted
        {
            get => _BytesCompleted;
            set
            {
                if (SetProperty(ref _BytesCompleted, value))
                    RaisePropertyChanged(nameof(Progress));
            }
        }

        /// <summary>
        /// The size of this file in bytes.
        /// </summary>
        [Description("Size in Bytes")]
        public long Length
        {
            get => _Length;
            set
            {
                if (SetProperty(ref _Length, value))
                    RaisePropertyChanged(nameof(Progress));
            }
        }

        /// <summary>
        /// Just the name of the file, without directory path info
        /// </summary>
        [Description("File Name")]
        public string Name
        {
            get => _Name;
            set => SetProperty(ref _Name, value);
        }

        /// <summary>
        /// The name of the file with directory path info, as presented from
        /// the <see cref="FileInfo.Name"/>.
        /// </summary>
        [Description("File Name")]
        public string FullName
        {
            get => _FullName;
            set => SetProperty(ref _FullName, value);
        }

        /// <summary>
        /// If <see langword="true"/>, then the file will be downloaded. Partial
        /// downloads will stop. Completed downloads are not affected.
        /// </summary>
        [Description("Download File?")]
        public bool Wanted
        {
            get => _Wanted;
            set => SetProperty(ref _Wanted, value);
        }

        /// <summary>
        /// One of the <see cref="BandwidthPriority"/> values.
        /// </summary>
        [Description("Bandwidth Priority")]
        public BandwidthPriority Priority
        {
            get => _Priority;
            set => SetProperty(ref _Priority, value);
        }

        /// <summary>
        /// Ratio of <see cref="BytesCompleted"/> to <see cref="Length"/>; i.e.,
        /// the percentage complete, expressed as a value from 0 to 1.
        /// </summary>
        [Description("Percent Completed")]
        public double Progress
        {
            get => _Progress;
            set => SetProperty(ref _Progress, value);
        }
    }
}
