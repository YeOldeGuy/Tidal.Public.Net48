namespace Tidal.Client.Constants
{
    /// <summary>
    /// These are the strings used in the remote procedure calls to the
    /// Transmission daemon.
    /// </summary>
    /// <remarks>
    /// There isn't anything regular about the strings, so even though most JSON
    /// libraries offer a way to massage property names into keys, they can't be
    /// used without difficulty. Some keys are camelCased, others deliniated
    /// with-dashes, and there's no rhyme or reason to it.
    /// </remarks>
    internal class RpcConstants
    {
        // RPC methods 
        public const string AddMagnet = "torrent-add";
        public const string AddTorrent = AddMagnet;
        public const string GetFreeSpace = "free-space";
        public const string GetSession = "session-get";
        public const string GetStats = "session-stats";
        public const string GetTorrents = "torrent-get";
        public const string SetSession = "session-set";
        public const string SetTorrent = "torrent-set";

        // All (almost) RPCs have these tags
        public const string Arguments = "arguments";
        public const string Method = "method";
        public const string Path = "path";
        public const string Tag = "tag";
        public const string Result = "result";

        // Other requests
        public const string Fields = "fields";

        // Responses
        public const string Torrents = "torrents";
        public const string FreeSpaceSize = "size-bytes";
        public const string TorrentAdded = "torrent-added";
        public const string TorrentDuped = "torrent-duplicate";

        // Related to AddTorrentArgs
        public const string DownloadDir = "download-dir";
        public const string FileName = "filename";
        public const string MetaInfo = "metainfo";
        public const string Paused = "paused";
        public const string Unwanted = "files-unwanted";

        // Related to Torrent-Action Requests
        public const string ForceStartTorrent = "torrent-start-now";
        public const string ReannounceTorrent = "torrent-reannounce";
        public const string StartTorrent = "torrent-start";
        public const string StopTorrent = "torrent-stop";
        public const string VerifyTorrent = "torrent-verify";

        // Various requests
        public const string DeleteData = "delete-local-data";
        public const string RemoveTorrent = "torrent-remove";

        // Torrent related
        public const string ActivityDate = "activityDate";
        public const string AddedDate = "addedDate";
        public const string DoneDate = "doneDate";
        public const string DownloadedEver = "downloadedEver";
        public const string DownloadLimit = "downloadLimit";
        public const string DownloadLimited = "downloadLimited";
        public const string Error = "error";
        public const string ErrorString = "errorString";
        public const string ETA = "eta";
        public const string Files = "files";
        public const string FileStats = "fileStats";
        public const string HashString = "hashString";
        public const string HonorsSessionLimits = "honorsSessionLimits";
        public const string Id = "id";
        public const string Ids = "ids";
        public const string IsFinished = "isFinished";
        public const string Name = "name";
        public const string PeerLimit = "peer-limit";
        public const string Peers = "peers";
        public const string PeersConnected = "peersConnected";
        public const string PercentDone = "percentDone";
        public const string Priorities = "priorities";
        public const string RateDownload = "rateDownload";
        public const string RateUpload = "rateUpload";
        public const string SecondsDownloading = "secondsDownloading";
        public const string SeedRatioMode = "seedRatioMode";
        public const string SeedRatioLimit = "seedRatioLimit";
        public const string SeedIdleMode = "seedIdleMode";
        public const string SeedIdleLimit = "seedIdleLimit";
        public const string Status = "status";
        public const string TotalSize = "totalSize";
        public const string TrackerStats = "trackerStats";
        public const string UploadedEver = "uploadedEver";
        public const string UploadLimit = "uploadLimit";
        public const string UploadLimited = "uploadLimited";
        public const string UploadRatio = "uploadRatio";

        // Mutator related
        public const string BandwidthPriority = "bandwidthPriority";
        public const string FilesWanted = "files-wanted";
        public const string FilesUnwanted = "files-unwanted";
        public const string Location = "location";
        public const string PriorityNormal = "priority-normal";
        public const string PriorityLow = "priority-low";
        public const string PriorityHigh = "priority-high";

        // Session Related
        public const string AltSpeedEnabled = "alt-speed-enabled";
        public const string AltSpeedDown = "alt-speed-down";
        public const string AltSpeedUp = "alt-speed-up";
        public const string AltSpeedBegin = "alt-speed-time-begin";
        public const string AltSpeedEnd = "alt-speed-time-end";
        public const string AltSpeedTimeEnabled = "alt-speed-time-enabled";
        public const string AltSpeedTimeDays = "alt-speed-time-day";
        public const string DownloadQueueSize = "download-queue-size";
        public const string DownloadQueueEnabled = "download-queue-enabled";
        public const string DownloadFreeSpace = "download-dir-free-space";
        public const string IncompleteDir = "incomplete-dir";
        public const string IncompleteDirEnabled = "incomplete-dir-enabled";
        public const string RenamePartialFiles = "rename-partial-files";
        public const string DHTEnabled = "dht-enabled";
        public const string LPDEnabled = "lpd-enabled";
        public const string PEXEnabled = "pex-enabled";
        public const string UTPEnabled = "utp-enabled";
        public const string Encryption = "encryption";
        public const string EncryptionRequired = "required";
        public const string EncryptionPreferred = "preferred";
        public const string EncryptionTolerated = "tolerated";
        public const string IdleSeedingLimit = "idle-seeding-limit";
        public const string IdleSeedingLimitEnabled = "idle-seeding-limit-enabled";
        public const string SeedRatioLimited = "seedRatioLimited";
        public const string PeerLimitGlobal = "peer-limit-global";
        public const string PeerLimitPerTorrent = "peer-limit-per-torrent";
        public const string PeerPort = "peer-port";
        public const string PeerPortRandomize = "peer-port-random-on-start";
        public const string SpeedLimitDown = "speed-limit-down";
        public const string SpeedLimitUp = "speed-limit-up";
        public const string SpeedLimitDownEnabled = "speed-limit-down-enabled";
        public const string SpeedLimitUpEnabled = "speed-limit-up-enabled";
        public const string Version = "version";

        // Temporal Stats Related
        public const string UploadedBytes = "uploadedBytes";
        public const string DownloadedBytes = "downloadedBytes";
        public const string FilesAdded = "filesAdded";
        public const string SessionCount = "sessionCount";
        public const string SecondsActive = "secondsActive";

        // Session Stats Related
        public const string ActiveTorrentCount = "activeTorrentCount";
        public const string DownloadSpeed = "downloadSpeed";
        public const string UploadSpeed = "uploadSpeed";
        public const string TorrentCount = "torrentCount";
        public const string PausedTorrentCount = "pausedTorrentCount";
        public const string CurrentStats = "current-stats";
        public const string CumulativeStats = "cumulative-stats";

        // Peer Related
        public const string Address = "address";
        public const string ClientName = "clientName";
        public const string FlagString = "flagStr";
        public const string HostName = "hostName";
        public const string IsEncrypted = "isEncrypted";
        public const string Port = "port";
        public const string Progress = "progress";
        public const string RateToClient = "rateToClient";
        public const string RateToPeer = "rateToPeer";

        // Tracker related
        public const string Host = "host";
        public const string IsBackup = "isBackup";
        public const string DownloadCount = "downloadCount";
        public const string LeecherCount = "leecherCount";
        public const string SeederCount = "seederCount";

        // File related
        public const string BytesCompleted = "bytesCompleted";
        public const string Wanted = "wanted";
        public const string Priority = "priority";
        public const string Length = "length";
    }
}
