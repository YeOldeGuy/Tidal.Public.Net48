using System.Threading.Tasks;
using Tidal.Client;
using Tidal.Client.Models;

namespace Tidal.Models.BrokerMessages
{
    internal class SetTorrentsRequest : BrokerRequestBase
    {
        public SetTorrentsRequest(TorrentMutator mutator)
        {
            Mutator = mutator;
        }


        public SetTorrentsRequest(int id, TorrentMutator mutator)
        {
            Mutator = mutator;
            Mutator.Ids = new[] { id };
        }

        public TorrentMutator Mutator { get; set; }


        public override async Task Invoke(IClient client)
        {
            var localClient = client;
            await InvokeWrapper(async () =>
            {
                await localClient.SetTorrentAsync(Mutator);
            });
        }
    }
}
