using System.Runtime.Serialization;
using Tidal.Client.Constants;

namespace Tidal.Client.Requests
{
    /// <summary>
    /// The arguments supplied to the daemon to retrieve the free space
    /// available.
    /// </summary>
    public class FreeSpaceArgs
    {
        /// <summary>
        /// Specified on the RPC request; must be a valid directory path on the
        /// server.
        /// </summary>
        [DataMember(Name = RpcConstants.Path)]
        public string Path
        {
            get; set;
        }
    }
}
