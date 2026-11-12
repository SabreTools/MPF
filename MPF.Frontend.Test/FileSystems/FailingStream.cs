using System;
using System.Collections.Generic;
using System.IO;

namespace MPF.Frontend.Test.FileSystems
{
    /// <summary>
    /// Readable stream that fails like a damaged disc from an offset on
    /// </summary>
    internal sealed class FailingStream : Stream
    {
        /// <summary>
        /// Contents of the stream
        /// </summary>
        private readonly MemoryStream _inner;

        /// <summary>
        /// First offset that cannot be read
        /// </summary>
        private readonly long _failFrom;

        /// <summary>
        /// 2048-byte sectors that were read
        /// </summary>
        public HashSet<long> SectorsRead { get; } = [];

        /// <summary>
        /// Number of bytes read, counting bytes read more than once
        /// </summary>
        public long BytesRead { get; private set; } = 0;

        /// <param name="length">Length of the stream, filled with zeros</param>
        /// <param name="failFrom">First offset that cannot be read</param>
        public FailingStream(long length, long failFrom)
            : this(new byte[length], failFrom)
        {
        }

        /// <param name="data">Contents of the stream</param>
        /// <param name="failFrom">First offset that cannot be read</param>
        public FailingStream(byte[] data, long failFrom)
        {
            _inner = new MemoryStream(data);
            _failFrom = failFrom;
        }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => true;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length => _inner.Length;

        /// <inheritdoc/>
        public override long Position
        {
            get => _inner.Position;
            set => _inner.Position = value;
        }

        /// <inheritdoc/>
        public override void Flush() { }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            int available = (int)Math.Max(0, Math.Min(count, _inner.Length - _inner.Position));
            if (available > 0 && _inner.Position + available > _failFrom)
                throw new IOException("Unreadable sector");

            long start = _inner.Position;
            int read = _inner.Read(buffer, offset, count);
            BytesRead += read;
            for (long sector = start / 2048; read > 0 && sector <= (start + read - 1) / 2048; sector++)
            {
                SectorsRead.Add(sector);
            }

            return read;
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
            => _inner.Seek(offset, origin);

        /// <inheritdoc/>
        public override void SetLength(long value)
            => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();
    }
}
