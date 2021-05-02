using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tidal.Models;
using Tidal.Services.Abstract;

namespace Tidal.Services.Actual
{
    public class TaskService : ITaskService, IDisposable
    {
        private readonly ConcurrentDictionary<string, ScheduledTask> tasks;
        private CancellationTokenSource cts;
        private Task maintask;

        public TaskService()
        {
            tasks = new ConcurrentDictionary<string, ScheduledTask>();
        }

        public string Add(string key, Func<Task> action, TimeSpan span)
        {
            CheckDisposed();

            return Add(new ScheduledTask(action, span, key));
        }

        public string Add(ScheduledTask scheduledTask)
        {
            CheckDisposed();

            if (tasks.ContainsKey(scheduledTask.Key))
                throw new ArgumentException($"key {scheduledTask.Key} already used");

            tasks.TryAdd(scheduledTask.Key, scheduledTask);
            return scheduledTask.Key;
        }

        public void Remove(string key)
        {
            CheckDisposed();

            tasks.TryRemove(key, out _);

            // There's a possibility that the task we're removing is the one
            // that's scheduled to be run next. If you look at the code in
            // MainLoop(), setting off the timer early does nothing but check
            // the pending tasks when it may not need it. If they're not due
            // to run, they won't.
        }

        public void Start()
        {
            CheckDisposed();

            if (cts == null || cts.IsCancellationRequested)
            {
                cts?.Dispose();
                cts = new CancellationTokenSource();
                maintask?.Dispose();
                maintask = new Task<Task>(MainLoop, cts.Token);
                maintask.Start();
            }
        }

        public void Stop()
        {
            CheckDisposed();

            cts?.Cancel();
            cts?.Dispose();
            cts = null;

            tasks.Clear();
        }

        public void ChangeInterval(string key, TimeSpan newInterval)
        {
            CheckDisposed();

            if (tasks.ContainsKey(key))
            {
                var task = tasks[key];
                if (task.Interval != newInterval)
                {
                    if (newInterval <= TimeSpan.Zero)
                        throw new ArgumentException(
                            "Interval less or equal to zero",
                            nameof(newInterval));
                    task.ChangeInterval(newInterval).ResetNextRun();
                }
            }
        }

        private async Task MainLoop()
        {
            CancellationToken token = cts.Token;
            TimeSpan interval = GetTimeToNextTask();

            // This is meant to be run in a separate thread as the call to
            // WaitOne will block until interval has elapsed or the token
            // is canceled.

            while (!token.WaitHandle.WaitOne(interval))
            {
                try
                {
                    foreach (var task in tasks.Values)
                    {
                        // Get the current time in a temp, use that instead of
                        // reusing DateTime.Now, just in case one of the
                        // Invoke() calls takes a while.

                        var now = DateTime.Now;
                        if (task.NextRun <= now)
                        {
                            await task.AsyncTask.Invoke();
                            task.ResetNextRun();
                        }
                    }
                    interval = GetTimeToNextTask();
                }
                catch (TaskCanceledException) { }
            }
        }

        private TimeSpan GetTimeToNextTask()
        {
            CheckDisposed();

            if (tasks.IsEmpty)
                return TimeSpan.FromSeconds(3.0);

            var nextTask = tasks.Values.OrderBy(t => t.NextRun).FirstOrDefault();
            if (nextTask is null)
                return TimeSpan.FromSeconds(1.0);

            var interval = nextTask.NextRun - DateTime.Now;
            return (interval < TimeSpan.Zero) ? TimeSpan.Zero : interval;
        }

        private void CheckDisposed()
        {
            if (disposedValue)
                throw new ObjectDisposedException(GetType().FullName);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    cts.Dispose();
                    maintask.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion IDisposable Support
    }
}
