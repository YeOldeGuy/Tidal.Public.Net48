using System.Threading.Tasks;
using Tidal.Client;

namespace Tidal.Models.BrokerMessages
{
    /// <summary>
    /// Use this message to request the current collection of
    /// <see cref="Torrent"/>s from the host.
    /// </summary>
    internal class TorrentRequest : BrokerRequestBase
    {
        public override async Task Invoke(IClient client)
        {
            var localClient = client;
            await InvokeWrapper(async () =>
            {
                var tors = await localClient.GetTorrentsAsync();
                if (tors != null)
                    Messenger.Send(new TorrentResponse(tors));
            });
        }
    }
}
