using System.Collections.Generic;
using System.Runtime.Serialization;
using Tidal.Client.Constants;
using Tidal.Client.Models;
using Utf8Json;

namespace Tidal.Client.Responses
{
    /*
     * A torrent response from the host looks like:
     * 
     * {
     *     "arguments": {
     *        "torrents": [
     *            {
     *               "activityDate": 1590172222,
     *               "addedDate": 1584909307,
     *                ...
     *               "uploadRatio": 1.8260,
     *               "uploadedEver": 4522647139
     *            }
     *        ]
     *     },
     *     "result": "success",
     *     "tag": 1
     *  }
     *
     */

    public class TorrentsResponseArgs
    {
        [DataMember(Name = RpcConstants.Torrents)]
        public IList<Torrent> Torrents
        {
            get; set;
        }
    }

    public class TorrentsResponse : ResponseBase
    {
        [DataMember(Name = RpcConstants.Arguments)]
        public TorrentsResponseArgs Arguments
        {
            get; set;
        }

        [IgnoreDataMember]
        public IList<Torrent> Torrents
        {
            get; private set;
        }

        public override void InPlaceDeserialize(string json)
        {
            try
            {
                var response = Deserialize<TorrentsResponse>(json);
                Arguments = response.Arguments;
                Torrents = response.Arguments.Torrents;
            }
            catch (JsonParsingException)
            {
                Arguments = null;
                Torrents = null;
            }
        }
    }
}
