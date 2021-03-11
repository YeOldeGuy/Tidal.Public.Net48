using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tidal.Client;
using Tidal.Client.Models;

namespace Tidal.Models.BrokerMessages
{
    internal class ReannounceTorrentsRequest : BrokerRequestBase
    {
        public ReannounceTorrentsRequest(IEnumerable<Torrent> torrents)
        {
            Ids = torrents.Select(t => t.Id);
        }

        public IEnumerable<int> Ids { get; }

        public override async Task Invoke(IClient client)
        {
            var localClient = client;
            await InvokeWrapper(async () =>
            {
                await localClient.ReannounceTorrentsAsync(Ids);
            });
        }
    }
}
