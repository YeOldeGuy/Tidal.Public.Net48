using System;
using System.Threading.Tasks;

namespace Tidal.Models
{
    /// <summary>
    /// Represents a single, repetitive task for the <see cref="IScheduledTaskService"/>.
    /// </summary>
    public class ScheduledTask
    {
        /// <summary>
        ///   Create a scheduled task for <see cref="IScheduledTaskService"/>.
        /// </summary>
        /// <param name="task">
        ///   A simple <see cref="AsyncTask"/> to perform regularly.
        /// </param>
        /// <param name="interval">
        ///   The elapsed time between invocations of the action. If none is
        ///   specified, you get three seconds.
        /// </param>
        /// <param name="key">
        ///   A <b>unique</b> identifier for the task. If none is specified, a
        ///   GUID will be assigned.
        /// </param>
        public ScheduledTask(Func<Task> task, TimeSpan interval = default, string key = null)
        {
            AsyncTask = task;
            Key = string.IsNullOrEmpty(key) ? Guid.NewGuid().ToString() : key;
            Interval = interval == default ? TimeSpan.FromSeconds(3) : interval;
        }

        public Func<Task> AsyncTask { get; }
        public string Key { get; }
        public TimeSpan Interval { get; private set; }
        public DateTime NextRun { get; private set; }

        /// <summary>
        /// Adds the <see cref="Interval"/> to the <paramref name="baseTime"/>, which
        /// defaults to <see cref="DateTime.Now"/>.
        /// </summary>
        /// <param name="baseTime">
        ///   The base time to add the interval to. Setting this allows you to start
        ///   a task later than the interval would otherwise specify.
        /// </param>
        /// <returns>This instance, good for chaining calls.</returns>
        public ScheduledTask ResetNextRun(DateTime baseTime = default)
        {
            NextRun = baseTime == default ? DateTime.Now + Interval : baseTime + Interval;
            return this;
        }

        /// <summary>
        /// Change the initially specified interval between invocations.
        /// </summary>
        /// <param name="newInterval">
        ///   The new interval. If the timespan is less than or equal to zero,
        ///   the interval will become one second.
        /// </param>
        /// <returns>This instance, good for chaining calls.</returns>
        public ScheduledTask ChangeInterval(TimeSpan newInterval)
        {
            Interval = newInterval <= TimeSpan.Zero ? TimeSpan.FromSeconds(1) : newInterval;
            return this;
        }
    }
}
