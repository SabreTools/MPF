namespace MPF.Processors
{
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