using System;
#if NET20 || NET35
using System.Collections.Generic;
#else
using System.Collections.Concurrent;
#endif
using System.Threading;
using System.Threading.Tasks;

namespace MPF.Core.Data
{
    public sealed class ProcessingQueue<T> : IDisposable
    {
        /// <summary>
        /// Internal queue to hold data to process
        /// </summary>
#if NET20 || NET35
        private readonly Queue<T> InternalQueue;
#else
        private readonly ConcurrentQueue<T> InternalQueue;
#endif

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
#if NET20 || NET35
            this.InternalQueue = new Queue<T>();
#else
            this.InternalQueue = new ConcurrentQueue<T>();
#endif
            this.CustomProcessing = customProcessing;
            this.TokenSource = new CancellationTokenSource();
#if NET20 || NET35
            Task.Run(() => ProcessQueue());
#elif NET40
            Task.Factory.StartNew(() => ProcessQueue());
#else
            Task.Run(() => ProcessQueue(), this.TokenSource.Token);
#endif
        }

        /// <summary>
        /// Dispose the current instance
        /// </summary>
        public void Dispose() => this.TokenSource.Cancel();

        /// <summary>
        /// Enqueue a new item for processing
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(T? item)
        {
            // Only accept new data when not cancelled
            if (item != null && !this.TokenSource.IsCancellationRequested)
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
#if NET20 || NET35
                if (InternalQueue.Count == 0)
#else
                if (InternalQueue.IsEmpty)
#endif
                {
                    if (this.TokenSource.IsCancellationRequested)
                        break;

                    Thread.Sleep(1);
                    continue;
                }

#if NET20 || NET35
                // Get the next item from the queue and invoke the lambda, if possible
                this.CustomProcessing?.Invoke(this.InternalQueue.Dequeue());
#else
                // Get the next item from the queue
                if (!this.InternalQueue.TryDequeue(out var nextItem))
                    continue;

                // Invoke the lambda, if possible
                this.CustomProcessing?.Invoke(nextItem);
#endif
            }
        }
    }
}
