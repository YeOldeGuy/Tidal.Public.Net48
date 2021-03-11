using System;
using System.Threading.Tasks;
using Tidal.Models;

namespace Tidal.Services.Abstract
{
    /// <summary>
    /// Provides a method of scheduling an <see cref="Action"/> to be
    /// invoked as regular intervals.
    /// </summary>
    public interface ITaskService
    {
        /// <summary>
        /// Begin processing any <see cref="ScheduledTask"/> instances.
        /// </summary>
        void Start();

        /// <summary>
        /// Cease processing <see cref="ScheduledTask"/> instances being maintained.
        /// <br/>
        /// <b>All tasks will be removed.</b>
        /// </summary>
        void Stop();

        /// <summary>
        /// Add a new <see cref="ScheduledTask"/> to the service for processing.
        /// </summary>
        /// <param name="scheduledTask"></param>
        /// <returns>
        ///   Returns the ID string assigned. See <see cref="ScheduledTask.Key"/>.
        /// </returns>
        string Add(ScheduledTask scheduledTask);

        string Add(string key, Func<Task> action, TimeSpan span);

        /// <summary>
        /// Removes a <see cref="ScheduledTask"/> from the service. 
        /// </summary>
        /// <param name="key">The unique ID string of the <see cref="ScheduledTask"/>.</param>
        void Remove(string key);

        /// <summary>
        /// Changes the Interval of time between invocations of the specified
        /// <see cref="ScheduledTask"/>.
        /// </summary>
        /// <param name="key">The ID string of the <see cref="ScheduledTask"/>.</param>
        /// <param name="newInterval">
        ///   The new interval. Times less than zero are not accepted and will
        ///   be modified.
        /// </param>
        void ChangeInterval(string key, TimeSpan newInterval);
    }
}
