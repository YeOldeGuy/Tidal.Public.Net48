using System.Runtime.Serialization;
using Tidal.Client.Constants;
using Tidal.Client.Helpers;

namespace Tidal.Client.Requests
{
    public class FreeSpaceRequest : RequestBase
    {
        public FreeSpaceRequest(string downloadDirectory)
        {
            Arguments = new FreeSpaceArgs { Path = downloadDirectory };
        }

        [DataMember(Name = RpcConstants.Arguments)]
        public FreeSpaceArgs Arguments
        {
            get; set;
        }

        protected override string GetMethodName() => RpcConstants.GetFreeSpace;

        public override string Serialize() => Json.ToJSON(this);
    }
}
