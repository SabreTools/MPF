using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MPF.Avalonia.Helpers
{
    /// <summary>
    /// Decouples the process-output producer from the (slow) UI consumer with a
    /// bounded channel, so a firehose of output can never block the reader thread
    /// or grow memory without bound -- the two failure modes behind the old lag.
    ///
    /// - Producer side (<see cref="Post"/>) is called from the stdout/stderr read
    ///   loops. It never blocks: when the consumer falls behind and the buffer is
    ///   full, the OLDEST queued chunk is dropped (<see cref="BoundedChannelFullMode.DropOldest"/>).
    ///   The raw stream is still written to the log file in full elsewhere, so
    ///   dropping only affects the live on-screen tail, never the saved log.
    /// - Consumer side drains everything currently available in ONE batch
    ///   (<see cref="DrainAvailable"/>), so the UI does a single dispatch per
    ///   flush tick instead of one per line. That is what keeps a high output
    ///   rate from saturating the UI thread.
    /// </summary>
    public sealed class OutputPump
    {
        private readonly Channel<string> _channel;

        public OutputPump(int capacity = 8192)
        {
            _channel = Channel.CreateBounded<string>(new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = false,
            });
        }

        /// <summary>
        /// Producer: enqueue a raw output chunk. Never blocks, never throws on full.
        /// </summary>
        public void Post(string chunk)
        {
            // With DropOldest, TryWrite always succeeds for a bounded channel and
            // silently evicts the oldest item when full -- exactly the backpressure
            // we want for a live tail. After completion TryWrite returns false, which
            // we ignore (output arriving after the window closed is simply dropped).
            _channel.Writer.TryWrite(chunk);
        }

        /// <summary>
        /// Signal that no more output will arrive.
        /// </summary>
        public void Complete() => _channel.Writer.TryComplete();

        /// <summary>
        /// Drain every chunk currently queued into one list, without waiting.
        /// Caller feeds the batch to a <see cref="TerminalBuffer"/> and pushes a
        /// single UI update. <paramref name="max"/> caps a single drain so one
        /// pathological burst cannot monopolize a flush tick.
        /// </summary>
        public List<string> DrainAvailable(int max = 4096)
        {
            var batch = new List<string>();
            while (batch.Count < max && _channel.Reader.TryRead(out string? chunk))
            {
                batch.Add(chunk);
            }

            return batch;
        }

        /// <summary>
        /// Await at least one chunk being available (or completion). For the flush loop.
        /// </summary>
        public ValueTask<bool> WaitToReadAsync(CancellationToken ct = default)
            => _channel.Reader.WaitToReadAsync(ct);

        /// <summary>
        /// Reference flush loop: wait, drain a batch, feed the buffer, raise one
        /// onBatch callback (with the number of chunks consumed in the batch), then
        /// throttle. In the GUI this callback marshals to the UI thread once per
        /// batch. Throttling is what bounds the dispatch rate.
        /// </summary>
        public async Task RunAsync(TerminalBuffer buffer, Action<int> onBatch, int flushIntervalMs = 75, CancellationToken ct = default)
        {
            try
            {
                while (await WaitToReadAsync(ct).ConfigureAwait(false))
                {
                    List<string> batch = DrainAvailable();
                    if (batch.Count > 0)
                    {
                        foreach (string chunk in batch)
                        {
                            buffer.Feed(chunk);
                        }

                        onBatch(batch.Count);
                    }

                    if (flushIntervalMs > 0)
                        await Task.Delay(flushIntervalMs, ct).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // expected on shutdown
            }

            // Final drain after completion so the tail is not lost.
            List<string> tail = DrainAvailable();
            foreach (string chunk in tail)
            {
                buffer.Feed(chunk);
            }

            buffer.Flush();
            onBatch(tail.Count);
        }
    }
}
