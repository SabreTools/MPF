using System;
using System.IO;

namespace MPF.Frontend.Test.FileSystems
{
    /// <summary>
    /// Readable stream that fails like a raw device on macOS unless it is read in whole blocks
    /// </summary>
    internal sealed class WholeBlockStream : Stream
    {
        /// <summary>
        /// Contents of the stream
        /// </summary>
        private readonly MemoryStream _inner;

        /// <summary>
        /// Number of bytes in a block
        /// </summary>
        private readonly int _blockSize;

        /// <summary>
        /// Number of reads
        /// </summary>
        public int Reads { get; private set; } = 0;

        /// <param name="data">Contents of the stream</param>
        /// <param name="blockSize">Number of bytes in a block</param>
        public WholeBlockStream(byte[] data, int blockSize)
        {
            _inner = new MemoryStream(data);
            _blockSize = blockSize;
        }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => true;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length => 0;

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
            if (_inner.Position % _blockSize != 0 || count % _blockSize != 0)
                throw new IOException("Invalid argument");

            Reads++;
            return _inner.Read(buffer, offset, count);
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
