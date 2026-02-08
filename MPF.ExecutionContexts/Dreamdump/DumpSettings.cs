namespace MPF.ExecutionContexts.Dreamdump
{
    /// <summary>
    /// Context-specific settings that can be used by caller
    /// </summary>
    public class DumpSettings : BaseDumpSettings
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
