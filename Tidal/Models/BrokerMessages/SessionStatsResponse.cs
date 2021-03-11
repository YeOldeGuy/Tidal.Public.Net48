using Tidal.Client.Models;

namespace Tidal.Models.BrokerMessages
{
    internal class SessionStatsResponse
    {
        public SessionStatsResponse(SessionStats sessionStats)
        {
            SessionStats = sessionStats;
        }

        public SessionStats SessionStats { get; }
    }
}
