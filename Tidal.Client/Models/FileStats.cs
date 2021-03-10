using System.Runtime.Serialization;
using Tidal.Client.Constants;

namespace Tidal.Client.Models
{
    /// <summary>
    /// Represents the "fileStats" information as defined in §3.3 of
    /// the rpc-spec.
    /// </summary>
    public class FileStats : Assignable<FileStats>
    {
        private long _BytesCompleted;
        private bool _Wanted;
        private int _PriorityRaw;

        protected override void AssignInternal(FileStats other)
        {
            Wanted = other.Wanted;
            PriorityRaw = other.PriorityRaw;
            BytesCompleted = other.BytesCompleted;
        }

        /// <summary>
        /// The number of bytes downloaded so far for this particular file. A
        /// duplicate of the info in <see cref="FileInfo.BytesCompleted"/>.
        /// </summary>
        [DataMember(Name = RpcConstants.BytesCompleted)]
        public long BytesCompleted
        {
            get => _BytesCompleted;
            set => SetProperty(ref _BytesCompleted, value);
        }


        /// <summary>
        /// If <see langword="true"/>, then this file will be downloaded. No effect
        /// after the download is accomplished.
        /// </summary>
        [DataMember(Name = RpcConstants.Wanted)]
        public bool Wanted
        {
            get => _Wanted;
            set => SetProperty(ref _Wanted, value);
        }


        [DataMember(Name = RpcConstants.Priority)]
        public int PriorityRaw
        {
            get => _PriorityRaw;
            set
            {
                if (SetProperty(ref _PriorityRaw, value))
                    RaisePropertyChanged(nameof(Priority));
            }
        }

        /// <summary>
        /// One of the <see cref="BandwidthPriority"/> values.
        /// </summary>
        public BandwidthPriority Priority => (BandwidthPriority)PriorityRaw;
    }
}
