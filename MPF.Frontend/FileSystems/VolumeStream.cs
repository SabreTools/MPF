using System;
using System.IO;

namespace MPF.Frontend.FileSystems
{
    /// <summary>
    /// Read-only stream over the start of another stream, with a fixed length
    /// </summary>
    /// <remarks>
    /// Used for devices, which report no length of their own, and to keep reads within the
    /// volume, since the sectors after it may be unreadable (such as audio tracks)
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

        /// <param name="baseStream">Underlying seekable stream</param>
        /// <param name="length">Length of the stream</param>
        public VolumeStream(Stream baseStream, long length)
        {
            _baseStream = baseStream;
            _length = length;
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
            _baseStream.Seek(_position, SeekOrigin.Begin);
            int read = _baseStream.Read(buffer, offset, count);
            _position += read;
            return read;
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
