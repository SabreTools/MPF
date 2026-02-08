namespace MPF.ExecutionContexts.Redumper
{
    /// <summary>
    /// Settings related to the dumping step (Redumper)
    /// </summary>
    public class DumpSettings
    {
        /// <summary>
        /// Enable skeleton output while dumping by default
        /// </summary>
        /// <remarks>Version 1 and greater; This is a hidden setting</remarks>
        public bool EnableSkeleton { get; set; } = SettingConstants.EnableSkeletonDefault;

        /// <summary>
        /// Enable verbose output while dumping by default
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool EnableVerbose { get; set; } = SettingConstants.EnableVerboseDefault;

        /// <summary>
        /// Default number of redumper Plextor leadin retries
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public int LeadinRetryCount { get; set; } = SettingConstants.LeadinRetryCountDefault;

        /// <summary>
        /// Enable options incompatible with redump submissions
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool NonRedumpMode { get; set; } = false;

        /// <summary>
        /// Currently selected default redumper drive type
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public DriveType DriveType { get; set; } = SettingConstants.DriveTypeDefault;

        /// <summary>
        /// Currently selected default redumper drive pregap start sector
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public int DrivePregapStart { get; set; } = SettingConstants.DrivePregapStartDefault;

        /// <summary>
        /// Currently selected default redumper read method
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public ReadMethod ReadMethod { get; set; } = SettingConstants.ReadMethodDefault;

        /// <summary>
        /// Currently selected default redumper sector order
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public SectorOrder SectorOrder { get; set; } = SettingConstants.SectorOrderDefault;

        /// <summary>
        /// Default number of rereads
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public int RereadCount { get; set; } = SettingConstants.RereadCountDefault;

        /// <summary>
        /// Enable the refine sector mode flag by default
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool RefineSectorMode { get; set; } = SettingConstants.RefineSectorModeDefault;
    }
}
