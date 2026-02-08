namespace MPF.ExecutionContexts.DiscImageCreator
{
    /// <summary>
    /// Context-specific settings that can be used by caller
    /// </summary>
    public class DumpSettings : BaseDumpSettings
    {
        /// <summary>
        /// Enable multi-sector read flag by default
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool MultiSectorRead { get; set; } = SettingConstants.MultiSectorReadDefault;

        /// <summary>
        /// Include a default multi-sector read value
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public int MultiSectorReadValue { get; set; } = SettingConstants.MultiSectorReadValueDefault;

        /// <summary>
        /// Enable overly-secure dumping flags by default
        /// </summary>
        /// <remarks>
        /// Version 1 and greater
        /// Split this into component parts later. Currently does:
        /// - Scan sector protection and set subchannel read level to 2 for CD
        /// - Set scan file protect flag for DVD
        /// </remarks>
        public bool ParanoidMode { get; set; } = SettingConstants.ParanoidModeDefault;

        /// <summary>
        /// Enable the Quiet flag by default
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool QuietMode { get; set; } = SettingConstants.QuietModeDefault;

        /// <summary>
        /// Default number of C2 rereads
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public int RereadCount { get; set; } = SettingConstants.RereadCountDefault;

        /// <summary>
        /// Default number of DVD/HD-DVD/BD rereads
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public int DVDRereadCount { get; set; } = SettingConstants.DVDRereadCountDefault;

        /// <summary>
        /// Use the CMI flag for supported disc types
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool UseCMIFlag { get; set; } = SettingConstants.UseCMIFlagDefault;
    }
}
