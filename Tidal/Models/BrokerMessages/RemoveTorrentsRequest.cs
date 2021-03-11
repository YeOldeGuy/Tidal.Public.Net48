using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tidal.Client;
using Tidal.Client.Models;

namespace Tidal.Models.BrokerMessages
{
    internal class RemoveTorrentsRequest : BrokerRequestBase
    {
        public RemoveTorrentsRequest(IEnumerable<Torrent> torrents, bool deleteData = true)
        {
            Ids = torrents.Select(t => t.Id);
            DeleteData = deleteData;
        }

        public IEnumerable<int> Ids { get; }

        public bool DeleteData { get; }


        public override async Task Invoke(IClient client)
        {
            var localClient = client;
            await InvokeWrapper(async () =>
            {
                await localClient.RemoveTorrentsAsync(Ids, DeleteData);
            });
        }
    }
}
