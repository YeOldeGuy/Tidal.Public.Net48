using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Tidal.Client.Constants;
using Tidal.Client.Helpers;

namespace Tidal.Client.Requests
{
    /// <summary>
    /// Represents a request to add a torrent to the client. The torrent
    /// file info in encoded by the caller to base64 prior to invoking.
    /// </summary>
    public class AddTorrentRequest : RequestBase
    {
        [DataMember(Name = RpcConstants.Arguments)]
        public AddTorrentArgs Args
        {
            get; set;
        }


        public AddTorrentRequest()
        {
        } // for testing


        /// <summary>
        /// Create a request to add a torrent via the <see cref="IClient"/>.
        /// </summary>
        /// <param name="buffer">
        ///   Base-64 encoded contents of a torrent file.
        /// </param>
        /// <param name="paused">
        ///   If true, add the torrent to the system but don't start it.
        /// </param>
        /// <param name="unwantedFiles">
        ///   A series of array offsets of the files to <b>not</b> download.
        /// </param>
        public AddTorrentRequest(string base64, bool paused, IEnumerable<int> unwantedFiles, string downloadDir = null)
        {
            Args = new AddTorrentArgs(base64, paused, unwantedFiles, downloadDir);
            if (unwantedFiles != null && unwantedFiles.Any())
                Args.Unwanted = unwantedFiles.ToList();
        }

        public override string Serialize() => Json.ToJSON(this);

        protected override string GetMethodName() => RpcConstants.AddTorrent;
    }
}
