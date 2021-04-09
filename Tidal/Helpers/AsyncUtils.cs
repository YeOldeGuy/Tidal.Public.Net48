using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tidal.Helpers
{
    public static class AsyncUtils
    {
        private static readonly TaskFactory _taskFactory =
            new TaskFactory(CancellationToken.None,
                            TaskCreationOptions.None,
                            TaskContinuationOptions.None,
                            TaskScheduler.Default);

        /// <summary>
        /// Executes an async Task method which has a void return value synchronously
        /// <br/><c>USAGE: AsyncUtil.RunSync(() => AsyncMethod());</c>
        /// </summary>
        /// <param name="task">Task method to execute</param>
        public static void RunSync(Func<Task> task)
            => _taskFactory
                .StartNew(task)
                .Unwrap()
                .GetAwaiter()
                .GetResult();

        /// <summary>
        /// Executes an async Task&lt;T&gt; method which has a T return type synchronously
        /// <br/><c>USAGE: return AsyncUtil.RunSync(() => AsyncMethod());</c>
        /// </summary>
        /// <typeparam name="TResult">Return Type</typeparam>
        /// <param name="task">Task&lt;T&gt; method to execute</param>
        /// <returns></returns>
        public static TResult RunSync<TResult>(Func<Task<TResult>> task)
            => _taskFactory
                .StartNew(task)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
    }
}
