using System;
#if NET20 || NET35
using System.Collections.Generic;
#else
using System.Collections.Concurrent;
#endif
using System.Threading;
using System.Threading.Tasks;

namespace MPF.Core
{
    public sealed class ProcessingQueue<T> : IDisposable
    {
        /// <summary>
        /// Internal queue to hold data to process
        /// </summary>
#if NET20 || NET35
        private readonly Queue<T> _internalQueue;
#else
        private readonly ConcurrentQueue<T> _internalQueue;
#endif

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
#if NET20 || NET35
            _internalQueue = new Queue<T>();
#else
            _internalQueue = new ConcurrentQueue<T>();
#endif
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
#if NET20 || NET35
                if (_internalQueue.Count == 0)
#else
                if (_internalQueue.IsEmpty)
#endif
                {
                    if (_tokenSource.IsCancellationRequested)
                        break;

                    Thread.Sleep(1);
                    continue;
                }

#if NET20 || NET35
                // Get the next item from the queue and invoke the lambda, if possible
                _customProcessing?.Invoke(_internalQueue.Dequeue());
#else
                // Get the next item from the queue
                if (!_internalQueue.TryDequeue(out var nextItem))
                    continue;

                // Invoke the lambda, if possible
                _customProcessing?.Invoke(nextItem);
#endif
            }
        }
    }
}
