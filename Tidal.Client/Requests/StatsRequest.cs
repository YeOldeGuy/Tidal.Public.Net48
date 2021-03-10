using Tidal.Client.Constants;
using Tidal.Client.Helpers;

namespace Tidal.Client.Requests
{
    public class StatsRequest : RequestBase
    {
        protected override string GetMethodName() => RpcConstants.GetStats;

        public override string Serialize()
        {
            return Json.ToJSON(this);
        }
    }
}
