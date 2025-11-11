#if NET20 || NET35
using System.Collections;
using System.Collections.Generic;

namespace MPF.Frontend
{
    internal interface IReadOnlyCollection<T> : IEnumerable<T>
    {
        int Count { get; }
    }

    internal sealed class ConcurrentQueue<T> : IReadOnlyCollection<T>
    {
        private Queue<T> _queue = new Queue<T>();

        private object _lock = new object();

        public int Count => _queue.Count;

        public void Enqueue(T item)
        {
            lock (_lock)
            {
                _queue.Enqueue(item);
            }
        }

        public bool TryDequeue(out T item)
        {
            lock (_lock)
            {
                item = default(T)!;
                if (_queue.Count == 0)
                    return false;

                item = _queue.Dequeue();
                return true;
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _queue.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _queue.GetEnumerator();
    }
}
#endif
