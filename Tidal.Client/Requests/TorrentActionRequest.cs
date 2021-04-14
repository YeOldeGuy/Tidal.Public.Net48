using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Tidal.Client.Constants;
using Tidal.Client.Helpers;

namespace Tidal.Client.Requests
{
    public class TorrentActionRequest : RequestBase
    {
        private readonly TorrentAction action;

        [DataMember(Name = RpcConstants.Arguments)]
        public TorrentActionArgs Arguments
        {
            get; set;
        }

        public TorrentActionRequest(IEnumerable<int> ids, TorrentAction requestType)
        {
            action = requestType;
            Arguments = new TorrentActionArgs(ids);
        }


        public override string Serialize() => Json.ToJSON(this);

        protected override string GetMethodName() => action.GetAttributeOfType<DescriptionAttribute>().Description;
    }
}
