using System.Runtime.Serialization;
using Tidal.Client.Constants;
using Tidal.Client.Contracts;
using Tidal.Client.Helpers;

namespace Tidal.Client.Responses
{
    /// <summary>
    /// All responses from the host derive from this. When the request
    /// doesn't have an expected response, like with set-session, you
    /// can use this class.
    /// </summary>
    public class ResponseBase : IResponse
    {
        [DataMember(Name = RpcConstants.Result)]
        public string Result
        {
            get; set;
        }

        [DataMember(Name = RpcConstants.Tag)]
        public long? Tag
        {
            get; set;
        }

        protected T Deserialize<T>(string json)
            where T : IResponse
        {
            var resp = Json.ToObject<T>(json);
            AssignCommonProperties(resp);
            return resp;
        }

        public virtual void InPlaceDeserialize(string json)
        {
            var _ = Deserialize<ResponseBase>(json);
        }

        protected void AssignCommonProperties(IResponse response)
        {
            if (response == null)
                return;

            Tag = response.Tag;
            Result = response.Result;
        }
    }
}
