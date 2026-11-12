using System;
using System.Collections.Generic;
using System.IO;

namespace MPF.Frontend.FileSystems
{
    /// <summary>
    /// Read-only stream over the start of another stream, with a fixed length
    /// </summary>
    /// <remarks>
    /// Used for devices, which report no length of their own and may only read whole blocks,
    /// and to keep reads within the volume, since the sectors after it may be unreadable (such
    /// as audio tracks)
    /// </remarks>
    internal sealed class VolumeStream : Stream
    {
        /// <summary>
        /// Underlying stream
        /// </summary>
        private readonly Stream _baseStream;

        /// <summary>
        /// Current position in the stream
        /// </summary>
        private long _position = 0;

        /// <summary>
        /// Length of the stream
        /// </summary>
        private long _length;

        /// <summary>
        /// Number of bytes in a block, 1 if the underlying stream can be read at any position
        /// </summary>
        private readonly int _blockSize;

        /// <summary>
        /// Blocks read from the underlying stream by their position, null if it is not read in whole blocks
        /// </summary>
        /// <remarks>Reading the file system reads some sectors more than once, which a raw device does not cache</remarks>
        private readonly Dictionary<long, byte[]>? _blocks;

        /// <summary>
        /// Number of blocks kept at most
        /// </summary>
        private const int MaxBlocks = 64;

        /// <param name="baseStream">Underlying seekable stream</param>
        /// <param name="length">Length of the stream</param>
        /// <param name="blockSize">Number of bytes in a block, if the underlying stream can only be read in whole blocks</param>
        public VolumeStream(Stream baseStream, long length, int blockSize = 1)
        {
            _baseStream = baseStream;
            _length = length;
            _blockSize = blockSize;
            if (blockSize > 1)
                _blocks = [];
        }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => true;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length => _length;

        /// <inheritdoc/>
        public override long Position
        {
            get => _position;
            set => _position = value;
        }

        /// <inheritdoc/>
        public override void Flush() { }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_position >= _length || count <= 0)
                return 0;

            count = (int)Math.Min(count, _length - _position);
            if (_blocks is null)
            {
                _baseStream.Seek(_position, SeekOrigin.Begin);
                int read = _baseStream.Read(buffer, offset, count);
                _position += read;
                return read;
            }

            // The blocks are read whole and copied from
            int total = 0;
            while (total < count)
            {
                long blockPosition = _position - (_position % _blockSize);
                if (!_blocks.TryGetValue(blockPosition, out var block))
                {
                    block = new byte[_blockSize];
                    _baseStream.Seek(blockPosition, SeekOrigin.Begin);
                    int read = _baseStream.Read(block, 0, _blockSize);
                    if (read < _blockSize)
                        Array.Resize(ref block, read);

                    if (_blocks.Count >= MaxBlocks)
                        _blocks.Clear();

                    _blocks[blockPosition] = block;
                }

                int chunk = (int)Math.Min(count - total, blockPosition + block.Length - _position);
                if (chunk <= 0)
                    break;

                Array.Copy(block, _position - blockPosition, buffer, offset + total, chunk);
                _position += chunk;
                total += chunk;
            }

            return total;
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            _position = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => _position + offset,
                SeekOrigin.End => _length + offset,
                _ => throw new ArgumentOutOfRangeException(nameof(origin)),
            };

            return _position;
        }

        /// <inheritdoc/>
        public override void SetLength(long value) => _length = value;

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _baseStream.Dispose();

            base.Dispose(disposing);
        }
    }
}
