using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MPF.Utilities
{
    internal class ProcessingQueue<T>
    {
        /// <summary>
        /// Internal queue to hold data to process
        /// </summary>
        private readonly ConcurrentQueue<T> InternalQueue;

        /// <summary>
        /// Custom processing step for dequeued data
        /// </summary>
        private readonly Action<T> CustomProcessing;

        public ProcessingQueue(Action<T> customProcessing)
        {
            this.InternalQueue = new ConcurrentQueue<T>();
            this.CustomProcessing = customProcessing;
            Task.Run(() => ProcessQueue());
        }

        /// <summary>
        /// Process
        /// </summary>
        private void ProcessQueue()
        {
            while (true)
            {
                // Nothing in the queue means we get to idle
                if (InternalQueue.Count == 0)
                    continue;

                // Get the next item from the queue
                if (!InternalQueue.TryDequeue(out T nextItem))
                    continue;

                // Invoke the lambda, if possible
                this.CustomProcessing?.Invoke(nextItem);
            }
        }
    }
}
