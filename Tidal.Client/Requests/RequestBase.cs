using System;
using System.Runtime.Serialization;
using Tidal.Client.Constants;
using Tidal.Client.Contracts;

namespace Tidal.Client.Requests
{
    public abstract class RequestBase : IRequest
    {
        public RequestBase()
        {
            Tag = new Random().Next(0, 10000); // just in case...
        }

        /// <summary>
        /// Describes the RPC method to call, such as "session-get" or "torrent-set".
        /// </summary>
        [DataMember(Name = RpcConstants.Method)]
        public string Method => GetMethodName();


        /// <summary>
        /// An optional property, if set, the corresponding response will have
        /// the same value in it.
        /// </summary>
        /// <remarks>
        /// Normally, the user doesn't need to set this value; it is set in the
        /// <see cref="Client"/> and checked there.
        /// </remarks>
        [DataMember(Name = RpcConstants.Tag)]
        public long? Tag { get; set; }


        /// <summary>
        /// Convert the instance to a JSON-encoded string.
        /// </summary>
        /// <remarks>
        /// Due to the way Utf8Json works, this can't be virtualized. Doing so,
        /// then using the base method will get you the JSON for the base and
        /// not the derived class. Json.NET does this correctly, so maybe the
        /// way this is implemented in Utf8Json is for performance. IDK.
        /// </remarks>
        /// <returns>A JSON-encoded string.</returns>
        public abstract string Serialize();

        protected abstract string GetMethodName();
    }
}
