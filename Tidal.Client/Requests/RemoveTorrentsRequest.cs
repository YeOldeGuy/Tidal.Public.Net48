using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Tidal.Client.Constants;
using Tidal.Client.Helpers;

namespace Tidal.Client.Requests
{
    public class RemoveTorrentsRequest : RequestBase
    {
        public RemoveTorrentsRequest(IEnumerable<int> ids, bool removeData)
        {
            Arguments = new RemoveTorrentsArgs
            {
                Ids = ids.ToList(),
                RemoveData = removeData,
            };
        }

        [DataMember(Name = RpcConstants.Arguments)]
        public RemoveTorrentsArgs Arguments { get; }

        public override string Serialize()
        {
            return Json.ToJSON(this);
        }

        protected override string GetMethodName() => RpcConstants.RemoveTorrent;
    }
}
