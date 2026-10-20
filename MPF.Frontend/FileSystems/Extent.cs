namespace MPF.Frontend.FileSystems
{
    /// <summary>
    /// Contiguous part of a file on a volume
    /// </summary>
    internal sealed class Extent
    {
        /// <summary>
        /// Byte offset of the extent on the volume
        /// </summary>
        public long Offset { get; }

        /// <summary>
        /// Length of the extent in bytes
        /// </summary>
        public long Length { get; }

        /// <summary>
        /// True if the extent is recorded on the volume, false if it reads as zeros
        /// </summary>
        public bool Recorded { get; }

        /// <param name="offset">Byte offset of the extent on the volume</param>
        /// <param name="length">Length of the extent in bytes</param>
        /// <param name="recorded">True if the extent is recorded on the volume, false if it reads as zeros</param>
        public Extent(long offset, long length, bool recorded)
        {
            Offset = offset;
            Length = length;
            Recorded = recorded;
        }
    }
}
