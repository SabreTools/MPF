using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MPF.Utilities
{
    public class ProcessingQueue<T> : IDisposable
    {
        /// <summary>
        /// Internal queue to hold data to process
        /// </summary>
        private readonly ConcurrentQueue<T> InternalQueue;

        /// <summary>
        /// Custom processing step for dequeued data
        /// </summary>
        private readonly Action<T> CustomProcessing;

        /// <summary>
        /// Internal processing task for dequeueing
        /// </summary>
        private readonly Task ProcessingTask;

        public ProcessingQueue(Action<T> customProcessing)
        {
            this.InternalQueue = new ConcurrentQueue<T>();
            this.CustomProcessing = customProcessing;
            this.ProcessingTask = Task.Run(() => ProcessQueue());
        }

        /// <summary>
        /// Dispose the current instance
        /// </summary>
        public void Dispose()
        {
            this.ProcessingTask.Dispose();
        }

        /// <summary>
        /// Enqueue a new item for processing
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(T item)
        {
            this.InternalQueue.Enqueue(item);
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
