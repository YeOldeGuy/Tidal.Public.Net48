using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Prism.Events;
using Tidal.Client;
using Tidal.Models;
using Tidal.Models.BrokerMessages;
using Tidal.Models.Messages;
using Tidal.Properties;
using Tidal.Services.Abstract;

namespace Tidal.Services.Actual
{
    /// <summary>
    /// The heart of the app, if there is one, this processes requests to the
    /// client, which in turn send back the responses.
    /// </summary>
    /// <remarks>
    /// This is ridiculously simple; it simply watches the blocking queue for
    /// requests, dequeues, then invokes them. Most of the intelligence is in
    /// the individual requests, which do the client request then package up
    /// the response if necessary (some requests don't have a response).
    /// </remarks>
    internal class BrokerService : IBrokerService, IDisposable
    {
        private readonly IClient client;
        private readonly IHostService hostService;
        private readonly BlockingCollection<BrokerRequestBase> requests;
        private readonly List<IDisposable> disposables;

        private CancellationTokenSource cts;
        private Task queueProcessor;
        private bool isOpen;


        public BrokerService(IClient client, IHostService hostService, IMessenger messenger)
        {
            disposables = new List<IDisposable>();
            this.client = client;
            this.hostService = hostService;

            client.Timeout = TimeSpan.FromSeconds(20);
            requests = new BlockingCollection<BrokerRequestBase>();
            cts = new CancellationTokenSource();

            // Due to the non-intuitive way the EventAggregator is in Prism, and
            // my general lack o'smarts, I have to subscribe to each of the
            // different requests separately then queue them.

            disposables.Add(messenger.Subscribe<AddMagnetRequest>(QueueRequest, ThreadOption.PublisherThread));
            disposables.Add(messenger.Subscribe<AddTorrentRequest>(QueueRequest, ThreadOption.PublisherThread));
            disposables.Add(messenger.Subscribe<ReannounceTorrentsRequest>(QueueRequest, ThreadOption.PublisherThread));
            disposables.Add(messenger.Subscribe<RemoveTorrentsRequest>(QueueRequest, ThreadOption.PublisherThread));
            disposables.Add(messenger.Subscribe<SessionRequest>(QueueRequest, ThreadOption.PublisherThread));
            disposables.Add(messenger.Subscribe<SessionStatsRequest>(QueueRequest, ThreadOption.PublisherThread));
            disposables.Add(messenger.Subscribe<SetSessionRequest>(QueueRequest, ThreadOption.PublisherThread));
            disposables.Add(messenger.Subscribe<SetTorrentsRequest>(QueueRequest, ThreadOption.PublisherThread));
            disposables.Add(messenger.Subscribe<StartTorrentsRequest>(QueueRequest, ThreadOption.PublisherThread));
            disposables.Add(messenger.Subscribe<StopTorrentsRequest>(QueueRequest, ThreadOption.PublisherThread));
            disposables.Add(messenger.Subscribe<TorrentRequest>(QueueRequest, ThreadOption.PublisherThread));
            disposables.Add(messenger.Subscribe<FreeSpaceRequest>(QueueRequest, ThreadOption.PublisherThread));

            // This is separate from the others above. This message we listen
            // for then change the host parameters when we get it. The others
            // are simply queued up.
            disposables.Add(messenger.Subscribe<HostChangedMessage>(OnHostChanged));

            disposables.Add(messenger.Subscribe<ShutdownMessage>((m) => cts?.Cancel(), ThreadOption.PublisherThread));
        }


        public async Task<bool> OpenAsync(string ipAddr,
                                          bool useAuth = false,
                                          string username = null,
                                          string password = null,
                                          int port = 9091)
        {
            client.SetHost(ipAddr, port);
            if (useAuth)
            {
                client.SetAuthorizationInfo(username, password);
            }
            isOpen = await client.TryOpenAsync();
            return isOpen;
        }

        public async Task<bool> OpenAsync(Host host)
        {
            return await OpenAsync(host.Address,
                                   host.UseAuthentication,
                                   host.UserName,
                                   host.Password,
                                   host.Port);
        }

        private void OnHostChanged(HostChangedMessage hostMsg)
        {
            var host = hostService.Hosts.Where(h => h.Id == hostMsg.ActiveId).FirstOrDefault();
            if (host == null || host != hostService.ActiveHost)
                throw new InvalidOperationException(Resources.ActivateNonHostError);

            client.SetHost(host.Address, host.Port);
            client.ClearAuthorizationInfo();
            if (host.UseAuthentication)
            {
                client.SetAuthorizationInfo(host.UserName, host.Password);
            }
        }


        private void QueueRequest(BrokerRequestBase request)
        {
            CheckDisposed();
            requests.Add(request);
        }

        public void Start()
        {
            CheckDisposed();

            try
            {
                // start a process to watch the requests queue.
                if (queueProcessor == null || queueProcessor.IsCanceled || queueProcessor.IsCompleted)
                {
                    cts = new CancellationTokenSource();
                    queueProcessor = new Task(ProcessRequests, cts.Token);
                    queueProcessor.Start();
                }
            }
            catch (OperationCanceledException)
            {
                Purge();
            }
        }

        public void Stop()
        {
            CheckDisposed();

            if (queueProcessor == null || queueProcessor.IsCanceled || queueProcessor.IsCompleted)
                return;

            cts.Cancel();
            cts.Dispose();
            Purge();
            queueProcessor.Dispose();
            queueProcessor = null;
            isOpen = false;
        }


        private async void ProcessRequests()
        {
            // This is run as a background task with a simple Task.Start() and
            // will keep running until canceled. All it does is watch the
            // request queue and when there's something available, invokes the
            // request. It is up to the invocation code to handle sending out
            // the response.
            try
            {
                while (!isDisposed && queueProcessor != null && !queueProcessor.IsCanceled)
                {
                    CheckDisposed();
                    BrokerRequestBase request = requests.Take(cts.Token);
                    await request.Invoke(client);
                }
            }
            catch (OperationCanceledException)
            {
                Purge();
            }
        }

        private void Purge()
        {
            CheckDisposed();

            while (requests.Count > 0)
                requests.TryTake(out var _);
        }


        #region IDisposable Support
        private void CheckDisposed()
        {
            if (isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        private bool isDisposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    foreach (var disposable in disposables)
                        disposable.Dispose();

                    cts.Dispose();
                    queueProcessor.Dispose();
                    requests.Dispose();
                }
                isDisposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
