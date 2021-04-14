using System.Runtime.Serialization;
using Tidal.Client.Constants;
using Tidal.Client.Helpers;
using Tidal.Client.Models;

namespace Tidal.Client.Requests
{
    public class MutateTorrentRequest : RequestBase
    {
        public MutateTorrentRequest(TorrentMutator mutator)
        {
            Mutator = mutator;
        }

        [DataMember(Name = RpcConstants.Arguments)]
        public TorrentMutator Mutator
        {
            get;
        }

        public override string Serialize() => Json.ToJSON(this);

        protected override string GetMethodName() => RpcConstants.SetTorrent;
    }
}
