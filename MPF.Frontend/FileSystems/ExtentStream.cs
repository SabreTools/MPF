using System;
using System.IO;

namespace MPF.Frontend.FileSystems
{
    /// <summary>
    /// Read-only stream of a file recorded in one or more extents of a volume
    /// </summary>
    internal sealed class ExtentStream : Stream
    {
        /// <summary>
        /// Stream of the volume the extents belong to
        /// </summary>
        private readonly Stream _volume;

        /// <summary>
        /// Extents of the file, in file order
        /// </summary>
        private readonly Extent[] _extents;

        /// <summary>
        /// Current position in the file
        /// </summary>
        private long _position = 0;

        /// <param name="volume">Stream of the volume, not disposed with this stream</param>
        /// <param name="extents">Extents of the file, in file order</param>
        /// <param name="length">Length of the file</param>
        public ExtentStream(Stream volume, Extent[] extents, long length)
        {
            _volume = volume;
            _extents = extents;
            Length = length;
        }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => true;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length { get; }

        /// <inheritdoc/>
        public override long Position
        {
            get => _position;
            set => _position = value;
        }

        /// <inheritdoc/>
        public override void Flush() { }

        /// <inheritdoc/>
        /// <remarks>
        /// Fills the request up to the end of the file, also across extents, as readers of file formats expect.
        /// Parts of the file that are not recorded, or that lie beyond the end of the volume, read as zeros
        /// </remarks>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_position >= Length || count <= 0)
                return 0;

            count = (int)Math.Min(count, Length - _position);

            int total = 0;
            while (total < count)
            {
                total += ReadExtent(buffer, offset + total, count - total);
            }

            return total;
        }

        /// <summary>
        /// Read from the extent containing the current position, up to the end of that extent
        /// </summary>
        /// <returns>Number of bytes read, at least one</returns>
        private int ReadExtent(byte[] buffer, int offset, int count)
        {
            // Find the extent containing the current position
            long extentStart = 0;
            foreach (var extent in _extents)
            {
                if (_position >= extentStart + extent.Length)
                {
                    extentStart += extent.Length;
                    continue;
                }

                long inExtent = _position - extentStart;
                count = (int)Math.Min(count, extent.Length - inExtent);

                // Read the recorded part that lies within the volume
                int read = 0;
                long volumeOffset = extent.Offset + inExtent;
                if (extent.Recorded && volumeOffset < _volume.Length)
                {
                    int available = (int)Math.Min(count, _volume.Length - volumeOffset);
                    _volume.Seek(volumeOffset, SeekOrigin.Begin);
                    while (read < available)
                    {
                        int chunk = _volume.Read(buffer, offset + read, available - read);
                        if (chunk <= 0)
                            break;

                        read += chunk;
                    }
                }

                // Everything else reads as zeros
                Array.Clear(buffer, offset + read, count - read);
                _position += count;
                return count;
            }

            // Data beyond the last extent reads as zeros
            Array.Clear(buffer, offset, count);
            _position += count;
            return count;
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            _position = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => _position + offset,
                SeekOrigin.End => Length + offset,
                _ => throw new ArgumentOutOfRangeException(nameof(origin)),
            };

            return _position;
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
            => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();
    }
}
