using System.Threading.Tasks;
using Tidal.Client;
using Tidal.Client.Models;

namespace Tidal.Models.BrokerMessages
{
    internal class SetSessionRequest : BrokerRequestBase
    {
        public SetSessionRequest(SessionMutator mutator)
        {
            Mutator = mutator;
        }

        /// <summary>
        /// Create a <see cref="SetSessionRequest"/> with a new
        /// <see cref="Mutator"/> that has the given property set to
        /// the specified value.
        /// </summary>
        /// <remarks>
        /// The normal mode of operation is to set a single value of
        /// the mutator, then send that to the host. This constructor
        /// makes that easier.
        /// </remarks>
        /// <param name="propertyName">The name of the property to set.</param>
        /// <param name="value">The value of the property.</param>
        public SetSessionRequest(string propertyName, object value)
        {
            Mutator = new SessionMutator(propertyName, value);
        }

        public SessionMutator Mutator { get; }

        public override async Task Invoke(IClient client)
        {
            var localClient = client;
            await InvokeWrapper(async () =>
            {
                await localClient.SetSessionAsync(Mutator);
            });
        }
    }
}
