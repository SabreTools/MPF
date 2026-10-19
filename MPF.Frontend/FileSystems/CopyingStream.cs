using System;
using System.IO;

namespace MPF.Frontend.FileSystems
{
    /// <summary>
    /// Read-only stream that writes what is read from the start of another stream to an output
    /// </summary>
    /// <remarks>
    /// Used to copy a file while it is identified and opened as its type, so it is read from the disc only once
    /// </remarks>
    internal sealed class CopyingStream : Stream
    {
        /// <summary>
        /// Stream to read from, not disposed with this stream
        /// </summary>
        private readonly Stream _source;

        /// <summary>
        /// Stream to write to, not disposed with this stream
        /// </summary>
        private readonly Stream _output;

        /// <summary>
        /// Length of the start of the source that was written to the output
        /// </summary>
        public long Copied { get; private set; } = 0;

        /// <summary>
        /// Error reading or writing, kept since readers of file formats ignore errors
        /// </summary>
        public Exception? Error { get; private set; }

        /// <param name="source">Seekable stream to read from</param>
        /// <param name="output">Stream to write to, at the position matching the start of the source</param>
        public CopyingStream(Stream source, Stream output)
        {
            _source = source;
            _output = output;
        }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => true;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length => _source.Length;

        /// <inheritdoc/>
        public override long Position
        {
            get => _source.Position;
            set => _source.Position = value;
        }

        /// <inheritdoc/>
        public override void Flush() { }

        /// <inheritdoc/>
        /// <remarks>
        /// Only reads that continue the copied start are written, so the output is written in order
        /// </remarks>
        public override int Read(byte[] buffer, int offset, int count)
        {
            try
            {
                long start = _source.Position;
                int read = _source.Read(buffer, offset, count);
                long end = start + read;
                if (start <= Copied && end > Copied)
                {
                    _output.Write(buffer, offset + (int)(Copied - start), (int)(end - Copied));
                    Copied = end;
                }

                return read;
            }
            catch (Exception ex)
            {
                Error = ex;
                throw;
            }
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
            => _source.Seek(offset, origin);

        /// <inheritdoc/>
        public override void SetLength(long value)
            => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();
    }
}
