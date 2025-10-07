namespace MPF.Processors
{
        /// <summary>
    /// Indicates the type of compression used for logs
    /// </summary>
    public enum LogCompression
    {
        /// <summary>
        /// PKZIP using DEFLATE level 5
        /// </summary>
        DeflateDefault,

        /// <summary>
        /// PKZIP using DEFLATE level 9
        /// </summary>
        DeflateMaximum,
        
        /// <summary>
        /// PKZIP using Zstd level 19
        /// </summary>
        Zstd19,
    }

    /// <summary>
    /// Enum for SecuROM scheme type
    /// </summary>
    internal enum SecuROMScheme
    {
        Unknown,

        /// <summary>
        /// No SecuROM, 0 Sectors
        /// </summary>
        None,

        /// <summary>
        /// SecuROM 1-2, 216 Sectors
        /// </summary>
        PreV3,

        /// <summary>
        /// SecuROM 3, 90 Sectors
        /// </summary>
        V3,

        /// <summary>
        /// SecuROM 4, 99 Sectors
        /// </summary>
        V4,

        /// <summary>
        /// SecuROM 4+, 11 Sectors
        /// </summary>
        V4Plus,
    }
}