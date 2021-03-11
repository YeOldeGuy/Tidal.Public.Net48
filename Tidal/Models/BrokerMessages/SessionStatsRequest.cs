using System.Threading.Tasks;
using Tidal.Client;

namespace Tidal.Models.BrokerMessages
{
    internal class SessionStatsRequest : BrokerRequestBase
    {
        public override async Task Invoke(IClient client)
        {
            var local = client;
            await InvokeWrapper(async () =>
            {
                var stats = await local.GetStatsAsync();
                Messenger.Send(new SessionStatsResponse(stats));
            });
        }
    }
}
