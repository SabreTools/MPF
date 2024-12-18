namespace MPF.Processors
{
    /// <summary>
    /// Enum for SecuROM scheme type
    /// </summary>
    internal enum SecuROMScheme
    {
        Unknown,
        PreV3, // 216 Sectors
        V3, // 90 Sectors
        V4, // 99 Sectors
        V4Plus, // 11 Sectors
    }
}