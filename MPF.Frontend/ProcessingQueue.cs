using System;
#if NET40_OR_GREATER || NETCOREAPP
using System.Collections.Concurrent;
#endif
using System.Threading;
using System.Threading.Tasks;

namespace MPF.Frontend
{
    public sealed class ProcessingQueue<T> : IDisposable
    {
        /// <summary>
        /// Internal queue to hold data to process
        /// </summary>
        private readonly ConcurrentQueue<T> _internalQueue;

        /// <summary>
        /// Custom processing step for dequeued data
        /// </summary>
        private readonly Action<T> _customProcessing;

        /// <summary>
        /// Cancellation method for the processing task
        /// </summary>
        private readonly CancellationTokenSource _tokenSource;

        public ProcessingQueue(Action<T> customProcessing)
        {
            _internalQueue = [];
            _customProcessing = customProcessing;
            _tokenSource = new CancellationTokenSource();
#if NET20 || NET35
            Task.Run(() => ProcessQueue());
#elif NET40
            Task.Factory.StartNew(() => ProcessQueue());
#else
            Task.Run(() => ProcessQueue(), _tokenSource.Token);
#endif
        }

        /// <summary>
        /// Dispose the current instance
        /// </summary>
        public void Dispose() => _tokenSource.Cancel();

        /// <summary>
        /// Enqueue a new item for processing
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(T? item)
        {
            // Only accept new data when not cancelled
            if (item != null && !_tokenSource.IsCancellationRequested)
                _internalQueue.Enqueue(item);
        }

        /// <summary>
        /// Process
        /// </summary>
        private void ProcessQueue()
        {
            while (true)
            {
                // Nothing in the queue means we get to idle
                if (_internalQueue.Count == 0)
                {
                    if (_tokenSource.IsCancellationRequested)
                        break;

                    Thread.Sleep(25);
                    continue;
                }

                // Get the next item from the queue
                if (!_internalQueue.TryDequeue(out var nextItem))
                    continue;

                // Invoke the lambda, if possible
                _customProcessing?.Invoke(nextItem);
            }
        }
    }
}
