using Prism.Mvvm;
using Tidal.Core.Models;

namespace Tidal.Models
{
    public class TorrentFileWanted : BindableBase
    {
        #region property backing store
        private int _Index;
        private string _Name;
        private bool _Wanted;
        private long _Size;
        #endregion


        /// <summary>
        /// Create an instance, using the info from a <see
        /// cref="TorrentMetadata.TorFileInfo.File"/>. This is used with
        /// torrents that have multiple files.
        /// </summary>
        /// <param name="index">The offset index number of the subfile.</param>
        /// <param name="file">The subfile information.</param>
        public TorrentFileWanted(int index, TorrentMetadata.TorFileInfo.File file)
        {
            Name = file.FullPath;
            Size = file.Length;
            Index = index;
            Wanted = true;
        }

        /// <summary>
        /// Create an instance, using the info from a single-file torrent,
        /// where there are no subfiles.
        /// </summary>
        /// <param name="name">The name of the file that'll be downloaded.</param>
        /// <param name="size">The reported size in bytes of the file.</param>
        public TorrentFileWanted(string name, long size)
        {
            Name = name;
            Size = size;
            Index = 0;
            Wanted = true;
        }

        public int Index
        {
            get => _Index;
            set => SetProperty(ref _Index, value);
        }

        public string Name
        {
            get => _Name;
            set => SetProperty(ref _Name, value);
        }

        public bool Wanted
        {
            get => _Wanted;
            set => SetProperty(ref _Wanted, value);
        }

        public long Size
        {
            get => _Size;
            set => SetProperty(ref _Size, value);
        }
    }
}
