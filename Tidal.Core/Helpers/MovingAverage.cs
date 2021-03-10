using System.Collections.Generic;
using System.Linq;

namespace Tidal.Core.Helpers
{
    /// <summary>
    ///   A class to maintain a moving average.
    /// </summary>
    /// <remarks>
    ///   A moving average is exactly what it sounds like, an average over a
    ///   moving number of figures. The normal queue depth is five, and the
    ///   average is calculated over the five numbers in that queue, a normal
    ///   FIFO queue.
    /// </remarks>
    public class MovingAverage
    {
        private readonly Queue<double> queue;
        private static int runLength;


        /// <summary>
        /// Create a new <see cref="MovingAverage"/> with the specified
        /// </summary>
        /// <param name="runLength"></param>
        public MovingAverage(int runLength = 5)
        {
            queue = new Queue<double>(runLength);
            MovingAverage.runLength = runLength;
        }


        /// <summary>
        /// Push a value into the queue, calculate the new moving average,
        /// return that value.
        /// </summary>
        /// <param name="value">A new value to put in the queue.</param>
        /// <returns>The newly calucated average.</returns>
        public double Push(double value)
        {
            queue.Enqueue(value);
            while (queue.Count > runLength)
            {
                queue.Dequeue();
            }
            return Average;
        }


        /// <summary>
        /// Clears the queue.
        /// </summary>
        public void Clear()
        {
            queue.Clear();
        }


        /// <summary>
        /// Gets the current average without disturbing the queue.
        /// </summary>
        public double Average => queue.Any() ? queue.Average() : 0;
    }
}
