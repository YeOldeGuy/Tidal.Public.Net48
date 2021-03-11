using System.Threading.Tasks;
using Tidal.Client;

namespace Tidal.Models.BrokerMessages
{
    /// <summary>
    /// A request for the latest <see cref="Session"/> information.
    /// </summary>
    internal class SessionRequest : BrokerRequestBase
    {
        public override async Task Invoke(IClient client)
        {
            var local = client;
            await InvokeWrapper(async () =>
            {
                var session = await local.GetSessionAsync();
                if (session != null)
                {
                    Messenger.Send(new SessionResponse(session));
                }
            });
        }
    }
}
