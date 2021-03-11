using System.Threading.Tasks;
using Tidal.Client;

namespace Tidal.Models.BrokerMessages
{
    internal class FreeSpaceRequest : BrokerRequestBase
    {
        public FreeSpaceRequest(string downloadDirectory)
        {
            DownloadDirectory = downloadDirectory;
        }

        public string DownloadDirectory { get; }


        public override async Task Invoke(IClient client)
        {
            var local = client;
            await InvokeWrapper(async () =>
            {
                var fs = await local.GetFreeSpaceAsync(DownloadDirectory);
                Messenger.Send(new FreeSpaceResponse(fs));
            });
        }
    }
}
