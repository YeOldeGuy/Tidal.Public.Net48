using System.Runtime.Serialization;
using Tidal.Client.Constants;
using Tidal.Client.Models;
using Utf8Json;

namespace Tidal.Client.Responses
{
    /*
     * A session response from the Transmission host looks like:
     *
     * {
     *     "arguments": {
     *         "alt-speed-down": 50,
     *         "alt-speed-enabled": true,
     *         "alt-speed-time-begin": 300,
     *         ...
     *         "utp-enabled": true,
     *         "version": "2.92 (14714)"
     *     },
     *     "result": "success",
     *     "tag": 2
     * }
     * 
     */

    public class SessionResponse : ResponseBase
    {
        [DataMember(Name = RpcConstants.Arguments)]
        public Session Session
        {
            get; set;
        }

        public override void InPlaceDeserialize(string json)
        {
            try
            {
                var resp = Deserialize<SessionResponse>(json);
                Session = resp.Session;
            }
            catch (JsonParsingException)
            {
                Session = null;
            }
        }
    }
}
