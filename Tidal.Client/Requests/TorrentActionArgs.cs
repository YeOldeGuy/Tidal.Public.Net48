using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Tidal.Client.Constants;

namespace Tidal.Client.Requests
{
    public class TorrentActionArgs
    {
        public TorrentActionArgs(IEnumerable<int> ids)
        {
            Ids = ids.ToList();
        }

        [DataMember(Name = RpcConstants.Ids)]
        public IList<int> Ids { get; }
    }
}
