using System.ComponentModel;

namespace Tidal.Client.Constants
{
    public enum TorrentAction
    {
        [Description(RpcConstants.StartTorrent)]
        Start,

        [Description(RpcConstants.ForceStartTorrent)]
        StartNow,

        [Description(RpcConstants.StopTorrent)]
        Stop,

        [Description(RpcConstants.VerifyTorrent)]
        Verify,

        [Description(RpcConstants.ReannounceTorrent)]
        Reannounce,
    }
}