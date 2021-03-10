using System.Runtime.Serialization;
using Tidal.Client.Constants;

namespace Tidal.Client.Responses
{
    public class FSRespArgs
    {
        [DataMember(Name = RpcConstants.FreeSpaceSize)]
        public long FreeSpace { get; set; }
    }

    public class FreeSpaceResponse : ResponseBase
    {
        public override void InPlaceDeserialize(string json)
        {
            var resp = Deserialize<FreeSpaceResponse>(json);
            FreeSpace = resp.Args?.FreeSpace ?? 0;
        }

        [DataMember(Name = RpcConstants.Arguments)]
        public FSRespArgs Args { get; set; }


        [IgnoreDataMember]
        public long FreeSpace { get; set; }
    }
}
