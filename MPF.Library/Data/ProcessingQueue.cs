using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace MPF.Data
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
        /// Cancellation method for the processing task
        /// </summary>
        private readonly CancellationTokenSource TokenSource;

        public ProcessingQueue(Action<T> customProcessing)
        {
            this.InternalQueue = new ConcurrentQueue<T>();
            this.CustomProcessing = customProcessing;
            this.TokenSource = new CancellationTokenSource();
            Task.Run(() => ProcessQueue(), this.TokenSource.Token);
        }

        /// <summary>
        /// Dispose the current instance
        /// </summary>
        public void Dispose()
        {
            this.TokenSource.Cancel();
        }

        /// <summary>
        /// Enqueue a new item for processing
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(T item)
        {
            if (!this.TokenSource.IsCancellationRequested)
                this.InternalQueue.Enqueue(item);
        }

        /// <summary>
        /// Process
        /// </summary>
        private void ProcessQueue()
        {
            while (true)
            {
                // If cancellation was requested, just do it
                if (this.TokenSource.IsCancellationRequested)
                    break;

                // Nothing in the queue means we get to idle
                if (this.InternalQueue.Count == 0)
                    continue;

                // Get the next item from the queue
                if (!this.InternalQueue.TryDequeue(out T nextItem))
                    continue;

                // Invoke the lambda, if possible
                this.CustomProcessing?.Invoke(nextItem);
            }
        }
    }
}
