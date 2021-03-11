namespace Tidal.Models.BrokerMessages
{
    internal class FreeSpaceResponse
    {
        public FreeSpaceResponse(long fs)
        {
            FreeSpace = fs;
        }

        public long FreeSpace { get; }
    }
}
