using System.Runtime.Serialization;
using Tidal.Client.Constants;

namespace Tidal.Client.Contracts
{
    /// <summary>
    /// Represents the data that all responses from the transmission
    /// daemon will have.
    /// </summary>
    public interface IResponse
    {
        /// <summary>
        /// The result value from the transmission client RPC return
        /// value, usually "success".
        /// </summary>
        [DataMember(Name = RpcConstants.Result)]
        string Result
        {
            get;
        }

        /// <summary>
        /// Matches the <see cref="IRpcRequest.Tag"/> value set by the
        /// <see cref="IClient"/> instance. Will be null if a tag was
        /// not specified in the corresponding request.
        /// </summary>
        [DataMember(Name = RpcConstants.Tag)]
        long? Tag
        {
            get; set;
        }

        void InPlaceDeserialize(string json);
    }
}
