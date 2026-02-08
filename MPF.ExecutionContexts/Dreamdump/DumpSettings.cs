namespace MPF.ExecutionContexts.Dreamdump
{
    /// <summary>
    /// Settings related to the dumping step (Dreamdump)
    /// </summary>
    public class DreamdumpDumpSettings
    {
        /// <summary>
        /// Enable options incompatible with redump submissions
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool NonRedumpMode { get; set; } = false;

        /// <summary>
        /// Currently selected default Dreamdump sector order
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public SectorOrder SectorOrder { get; set; } = SettingConstants.SectorOrderDefault;

        /// <summary>
        /// Default number of rereads
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public int RereadCount { get; set; } = SettingConstants.RereadCountDefault;
    }
}
