using System.Runtime.Serialization;
using Tidal.Client.Constants;
using Tidal.Client.Helpers;

namespace Tidal.Client.Requests
{
    /// <summary>
    /// Represents a request to add a magnet link torrent to the
    /// client.
    /// </summary>
    public class AddMagnetRequest : RequestBase
    {
        public AddMagnetRequest() { }


        /// <summary>
        /// Create a request to add a magnet link to the client. The response
        /// is an <see cref="AddTorrentResponse"/>.
        /// </summary>
        /// <remarks>
        /// The link you specifiy isn't checked in any way. If you pass in
        /// garbage, then the host will burp and you'll get an exception
        /// at a later time, when the request is processed.
        /// </remarks>
        /// <param name="magnetLink">A magnet link, unchecked for validity.</param>
        /// <param name="paused">If <see langword="true"/>, then the link is added
        /// but the torrent isn't started.</param>
        public AddMagnetRequest(string magnetLink, bool paused)
        {
            Args = new AddTorrentArgs(paused, magnetLink);
        }

        [DataMember(Name = RpcConstants.Arguments)]
        public AddTorrentArgs Args { get; set; }


        public override string Serialize()
        {
            return Json.ToJSON(this);
        }

        protected override string GetMethodName() => RpcConstants.AddMagnet;
    }
}
