using Tidal.Client.Constants;
using Tidal.Client.Helpers;

namespace Tidal.Client.Requests
{
    public class SessionRequest : RequestBase
    {
        protected override string GetMethodName() => RpcConstants.GetSession;

        public override string Serialize()
        {
            return Json.ToJSON(this);
        }
    }
}
