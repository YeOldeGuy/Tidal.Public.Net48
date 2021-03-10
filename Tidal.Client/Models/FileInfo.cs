using System.Runtime.Serialization;
using Tidal.Client.Constants;

namespace Tidal.Client.Models
{
    /// <summary>
    /// Represents the "files" information as defined in §3.3 of
    /// the rpc-spec.
    /// </summary>
    public class FileInfo : Assignable<FileInfo>
    {
        private long _BytesCompleted;
        private long _Length;
        private string _Name;

        protected override void AssignInternal(FileInfo other)
        {
            BytesCompleted = other.BytesCompleted;
            Length = other.Length;
            Name = other.Name;
        }

        /// <summary>
        /// The number of bytes downloaded so far for this particular file.
        /// </summary>
        [DataMember(Name = RpcConstants.BytesCompleted)]
        public long BytesCompleted
        {
            get => _BytesCompleted;
            set
            {
                if (SetProperty(ref _BytesCompleted, value))
                    RaisePropertyChanged(nameof(PercentDone));
            }
        }


        /// <summary>
        /// The total size of this particular file.
        /// </summary>
        [DataMember(Name = RpcConstants.Length)]
        public long Length
        {
            get => _Length;
            set
            {
                if (SetProperty(ref _Length, value))
                    RaisePropertyChanged(nameof(PercentDone));
            }
        }


        /// <summary>
        /// The name of the file, including any directory paths.
        /// </summary>
        [DataMember(Name = RpcConstants.Name)]
        public string Name
        {
            get => _Name;
            set => SetProperty(ref _Name, value);
        }


        /// <summary>
        /// Simply the percent of the length the bytes completed is.
        /// </summary>
        public double PercentDone => (double)BytesCompleted / Length;
    }
}
