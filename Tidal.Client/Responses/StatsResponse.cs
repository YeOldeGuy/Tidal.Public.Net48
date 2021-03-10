using System.Runtime.Serialization;
using Tidal.Client.Constants;
using Tidal.Client.Models;
using Utf8Json;

namespace Tidal.Client.Responses
{
    /*
     * A session stats response from the host looks like (full thing):
     * 
     * {
     *     "arguments": {
     *         "activeTorrentCount": 8,
     *         "cumulative-stats": {
     *             "downloadedBytes": 40814833140,
     *             "filesAdded": 27,
     *             "secondsActive": 9603065,
     *             "sessionCount": 13,
     *             "uploadedBytes": 83690801373
     *         },
     *         "current-stats": {
     *             "downloadedBytes": 6904581967,
     *             "filesAdded": 2,
     *             "secondsActive": 498618,
     *             "sessionCount": 1,
     *             "uploadedBytes": 17792557492
     *         },
     *         "downloadSpeed": 0,
     *         "pausedTorrentCount": 2,
     *         "torrentCount": 10,
     *         "uploadSpeed": 1382
     *     },
     *     "result": "success",
     *     "tag": 4
     * }     
     */

    /// <summary>
    /// Represents a response from the host with the latest session stats.
    /// </summary>
    public class StatsResponse : ResponseBase
    {
        [DataMember(Name = RpcConstants.Arguments)]
        public SessionStats SessionStats { get; set; }

        public override void InPlaceDeserialize(string json)
        {
            try
            {
                var resp = Deserialize<StatsResponse>(json);
                SessionStats = resp.SessionStats;
            }
            catch (JsonParsingException)
            {
                SessionStats = null;
            }
        }
    }
}
