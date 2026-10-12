using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MPF.Avalonia
{
    /// <summary>
    /// Decouples the process-output producer from the consumer with
    /// a bounded channel, so a firehose of output can never block the
    /// reader thread or grow memory without bound
    /// </summary>
    public sealed class OutputPump
    {
        /// <summary>
        /// Bounded channel for reading and writing
        /// </summary>
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
        /// Drain every chunk currently queued into one list, without waiting
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

        #region Passthrough Functionality

        /// <inheritdoc cref="ChannelWriter.TryComplete(Exception?)"/>
        public bool TryComplete()
            => _channel.Writer.TryComplete();

        /// <inheritdoc cref="ChannelWriter.TryWrite(T)"/>
        public bool TryWrite(string item)
            => _channel.Writer.TryWrite(item);

        /// <inheritdoc cref="ChannelReader.WaitToReadAsync(CancellationToken)"/>
        public ValueTask<bool> WaitToReadAsync(CancellationToken ct = default)
            => _channel.Reader.WaitToReadAsync(ct);

        #endregion
    }
}
