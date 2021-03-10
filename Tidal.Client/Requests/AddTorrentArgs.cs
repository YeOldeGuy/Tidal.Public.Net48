using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Tidal.Client.Constants;

namespace Tidal.Client.Requests
{
    /// <summary>
    /// Represents the data that this app might pass to the transmission client
    /// when adding a torrent.
    /// </summary>
    public class AddTorrentArgs
    {
        // For testing
        public AddTorrentArgs()
        {
            Paused = false;
        }

        /// <summary>
        /// This cons is for the <see cref="AddMagnetRequest"/>.
        /// </summary>
        /// <param name="paused"></param>
        public AddTorrentArgs(bool paused, string link)
        {
            Paused = paused;
            FileName = link;
        }


        /// <summary>
        /// Create the arguments section of the command.
        /// </summary>
        /// <param name="paused"></param>
        public AddTorrentArgs(string base64, bool paused, IEnumerable<int> unwantedFiles, string downloadDir = null)
        {
            MetaInfo = base64;
            Paused = paused;
            if (Unwanted != null)
                Unwanted = unwantedFiles.ToList();
            DownloadDir = downloadDir;
        }

        /// <summary>
        /// The directory to use for downloading, overriding the default.
        /// This app does not use this. Extremely Danger!
        /// </summary>
        [DataMember(Name = RpcConstants.DownloadDir)]
        public string DownloadDir { get; set; }

        /// <summary>
        /// Not really a filename, but a URI for a magnet link.
        /// </summary>
        /// <remarks>
        ///   You can pass in a file name, but the file has to be accessable on
        ///   the client's disk, which isn't practical for a daemon installation
        ///   normally, since you probably don't have access to the client's
        ///   file system.
        /// </remarks>
        [DataMember(Name = RpcConstants.FileName)]
        public string FileName { get; set; }

        /// <summary>
        /// The base64 encoded contents of a ".torrent" file.
        /// </summary>
        [DataMember(Name = RpcConstants.MetaInfo)]
        public string MetaInfo { get; set; }

        /// <summary>
        /// If set, then the torrent or magnet will be added to the system, but
        /// not started. It will show up as "Stopped" in the status field.
        /// </summary>
        [DataMember(Name = RpcConstants.Paused)]
        public bool Paused { get; set; }

        /// <summary>
        /// An array of file indices of the files in a multi-file torrent that
        /// should <b>not</b> be downloaded.
        /// </summary>
        [DataMember(Name = RpcConstants.Unwanted)]
        public IList<int> Unwanted { get; set; }
    }
}
