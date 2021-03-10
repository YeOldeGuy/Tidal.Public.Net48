using System.Collections.Generic;
using System.Runtime.Serialization;
using Tidal.Client.Constants;

namespace Tidal.Client.Requests
{
    public class RemoveTorrentsArgs
    {
        [DataMember(Name = RpcConstants.Ids)]
        public IList<int> Ids { get; set; }

        [DataMember(Name = RpcConstants.DeleteData)]
        public bool RemoveData { get; set; }
    }
}
