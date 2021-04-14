using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tidal.Client.Constants;
using Tidal.Client.Contracts;
using Tidal.Client.Exceptions;
using Tidal.Client.Models;
using Tidal.Client.Requests;
using Tidal.Client.Responses;

namespace Tidal.Client
{
    public class Client : IDisposable, IClient
    {
        private readonly HttpClient httpClient;
        private readonly string XTransSessIdHeader = "X-Transmission-Session-Id";
        private readonly SemaphoreSlim accessSemaphore = new SemaphoreSlim(1, 1);
        private int tagNumber = 0;

        public Client()
        {
            var media = new MediaTypeWithQualityHeaderValue("text/json");

            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add(XTransSessIdHeader, "Gets a CSRF reply later");
            httpClient.DefaultRequestHeaders.Accept.Add(media);
        }

        private Uri _Host;
        private bool disposedValue;

        public Uri Host
        {
            get => _Host;
            private set
            {
                if (_Host != value)
                {
                    IsOpen = false;
                    _Host = value;
                }
            }
        }

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

        public string IpAddress => Host?.Host ?? "";

        public int Port => Host?.Port ?? 9091;

        public bool IsOpen { get; set; } = false;

        public bool SecureConnection { get; private set; } = false;


        public void SetAuthorizationInfo(string username, string password)
        {
            if (username.Contains(":"))
                throw new ClientAuthorizationException(
                    $"No colons in {nameof(username)}",
                    new ArgumentException(null, nameof(username)));

            var header = new AuthenticationHeaderValue(
                "Basic",
                CreateAuthorizationString(username, password));

            ClearAuthorizationInfo();
            httpClient.DefaultRequestHeaders.Authorization = header;
        }

        public void ClearAuthorizationInfo() => httpClient.DefaultRequestHeaders.Authorization = null;

        private static string CreateAuthorizationString(string username, string password)
        {
            var auth = $"{username}:{password}";
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(auth));
        }

        public void SetHost(string address, int port, bool secure = false)
        {
            var protocol = secure ? "https" : "http";

            Host = new Uri($"{protocol}://{address}:{port}/transmission/rpc");
            SecureConnection = secure;
            if (secure)
            {
                // Don't be picky about what TLS protocol is used
                ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Tls12
                    | SecurityProtocolType.Tls11
                    | SecurityProtocolType.Tls;
            }
        }

        private async Task SendRequestAsync(IRequest req, IResponse resp)
        {
            // Each request has a unique tag number. It doesn't matter what it
            // is; it's just there to identify a response/request pair.
            req.Tag = Interlocked.Increment(ref tagNumber);

            // Make the request into some really cool JSON stuff
            var reqAsJson = req.Serialize();

            using (var cts = new CancellationTokenSource(Timeout))
            {
                await accessSemaphore.WaitAsync();
                try
                {
                    // Getting the CSRF established will take two attempts. One, to
                    // send a known-bad CSRF key, then the second to resend the
                    // request with the CSRF key received. If this fails after three
                    // tries, then all is lost and it's time to die.
                    for (var i = 0; i < 3; i++)
                    {
                        using (var content = new StringContent(reqAsJson, Encoding.UTF8, "text/json"))
                        using (var httpresp = await httpClient.PostAsync(Host, content, cts.Token))
                        {
#pragma warning disable IDE0010 // There are a gazillion codes here. I don't want to list them all
                            switch (httpresp.StatusCode)
#pragma warning restore IDE0010
                            {
                                case HttpStatusCode.OK:
                                    var respStream = await httpresp.Content.ReadAsStringAsync();
                                    resp.InPlaceDeserialize(respStream);
                                    if (req.Tag != resp.Tag)
                                        throw new ClientRPCException("Mismatched tags");
                                    if (resp.Result != "success")
                                        throw new ClientRPCException($"Bad result code: {resp.Result}");
                                    return;
                                case HttpStatusCode.Conflict:
                                    // This is where the CSRF stuff is done. On the very
                                    // first request, the key won't be right and the
                                    // host will send back the Conflict error. Read the
                                    // CSRF token from the headers, save it back to the
                                    // headers, and try again.
                                    var csrf = httpresp.Headers.GetValues(XTransSessIdHeader)
                                                               .FirstOrDefault();
                                    if (string.IsNullOrEmpty(csrf))
                                        throw new ClientException("No receipt of CSRF token");

                                    // Get rid of the bad CSRF value and put the good
                                    // one in:
                                    _ = httpClient.DefaultRequestHeaders.Remove(XTransSessIdHeader);
                                    httpClient.DefaultRequestHeaders.Add(XTransSessIdHeader, csrf);

                                    // Now that the CSRF stuff is "correct", try to post
                                    // the request again.
                                    continue;
                                case HttpStatusCode.Unauthorized:
                                    throw new ClientAuthorizationException();
                                case HttpStatusCode.Forbidden:
                                    throw new ClientException("Forbidden (are you trying to connect to /web?)");
                                case (HttpStatusCode)421:
                                    throw new ClientException("Rebinding error. Try a white list on the daemon.");
                                default:
                                    throw new ClientException($"WTF Error {httpresp.StatusCode}");
                            }
                        }
                    }
                    throw new ClientException("Unable to establish anti-CSRF connection - tried 3 times.");
                }
                catch (WebException ex)
                {
                    throw new ClientException($"Web Exception {ex.Message}");
                }
                catch (HttpRequestException ex)
                {
                    throw new ClientException($"HTTP Request error {ex.Message}");
                }
                catch (COMException ex)
                {
                    throw new ClientException($"COM error {ex.Message}");
                }
                catch (TaskCanceledException)
                {
                    throw new ClientTimeoutException();
                }
                finally
                {
                    _ = accessSemaphore.Release();
                }
            }
        }

        public async Task<bool> TryOpenAsync()
        {
            IsOpen = false;

            if (Host == null)
                return false;

            await SendRequestAsync(new StatsRequest(), new ResponseBase());
            IsOpen = true;
            return true;
        }


        public async Task<IEnumerable<Torrent>> GetTorrentsAsync(
            IEnumerable<int> ids = null,
            IEnumerable<string> fields = null)
        {
            var req =
                ids == null && fields == null
                ? new TorrentsRequest()
                : new TorrentsRequest(ids, fields);

            var resp = new TorrentsResponse();

            await SendRequestAsync(req, resp);
            return resp.Arguments?.Torrents;
        }


        public async Task<Session> GetSessionAsync()
        {
            var resp = new SessionResponse();

            await SendRequestAsync(new SessionRequest(), resp);
            return resp.Session;
        }

        public async Task<SessionStats> GetStatsAsync()
        {
            var resp = new StatsResponse();

            await SendRequestAsync(new StatsRequest(), resp);
            return resp?.SessionStats;
        }

        public async Task SetSessionAsync(SessionMutator mutator) => await SendRequestAsync(new MutateSessionRequest(mutator),
                                   new ResponseBase());

        public async Task SetTorrentAsync(TorrentMutator mutator) => await SendRequestAsync(new MutateTorrentRequest(mutator),
                                   new ResponseBase());

        public async Task RemoveTorrentsAsync(IEnumerable<int> ids, bool deleteData) => await SendRequestAsync(new RemoveTorrentsRequest(ids, deleteData),
                                   new ResponseBase());

        public async Task ReannounceTorrentsAsync(IEnumerable<int> ids) => await SendRequestAsync(new TorrentActionRequest(ids, TorrentAction.Reannounce),
                                   new ResponseBase());

        public async Task StartTorrentsAsync(IEnumerable<int> ids) => await SendRequestAsync(new TorrentActionRequest(ids, TorrentAction.Start),
                                   new ResponseBase());

        public async Task StopTorrentsAsync(IEnumerable<int> ids) => await SendRequestAsync(new TorrentActionRequest(ids, TorrentAction.Stop),
                                   new ResponseBase());

        public async Task<(Torrent, bool)> AddMagnetAsync(string magnetLink, bool paused)
        {
            var req = new AddMagnetRequest(magnetLink, paused);
            var resp = new AddTorrentResponse();

            await SendRequestAsync(req, resp);
            var addedMagnet = new Torrent()
            {
                Id = resp.Id,
                Name = resp.Name,
                HashString = resp.HashString,
            };
            return (addedMagnet, resp.IsDuplicate);
        }

        public async Task<(Torrent, bool)> AddTorrentAsync(string base64,
                                                           bool paused,
                                                           IEnumerable<int> unwantedIndexes)
        {
            var req = new AddTorrentRequest(base64, paused, unwantedIndexes);
            var resp = new AddTorrentResponse();

            await SendRequestAsync(req, resp);
            var addedTorrent = new Torrent
            {
                Id = resp.Id,
                Name = resp.Name,
                HashString = resp.HashString,
            };
            return (addedTorrent, resp.IsDuplicate);
        }

        public async Task<long> GetFreeSpaceAsync(string downloadDirectory)
        {
            var req = new FreeSpaceRequest(downloadDirectory);
            var resp = new FreeSpaceResponse();

            await SendRequestAsync(req, resp);
            return resp.FreeSpace;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    accessSemaphore.Dispose();
                    httpClient.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
