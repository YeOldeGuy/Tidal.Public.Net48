using System.Runtime.Serialization;
using Tidal.Client.Constants;
using Tidal.Client.Helpers;
using Tidal.Client.Models;

namespace Tidal.Client.Requests
{
    /// <summary>
    /// Used to alter parameters of the current Transmission session.
    /// </summary>
    public class MutateSessionRequest : RequestBase
    {
        /// <summary>
        /// Create a request for altering run-time parameters of the
        /// current session.
        /// </summary>
        /// <remarks>
        /// See the <see cref="SessionMutator"/> for details of what can
        /// be changed, but take it almost everything that's in a
        /// <see cref="Session"/> can be set. There isn't any kind of
        /// sanity checking for values; if you set the download directory
        /// to something that doesn't exist, you're gonna have a bad
        /// time.
        /// <para/>
        /// Set only those properties that you want changed with everything
        /// else remaining unset or null.
        /// </remarks>
        /// <param name="mutator">The <see cref="SessionMutator"/> with 
        /// the changes requested.</param>
        public MutateSessionRequest(SessionMutator mutator)
        {
            Mutator = mutator;
        }

        [DataMember(Name = RpcConstants.Arguments)]
        public SessionMutator Mutator
        {
            get;
        }

        public override string Serialize() => Json.ToJSON(this);

        protected override string GetMethodName() => RpcConstants.SetSession;
    }
}
