namespace MPF.ExecutionContexts.Aaru
{
    /// <summary>
    /// Context-specific settings that can be used by caller
    /// </summary>
    public class DumpSettings : BaseDumpSettings
    {
        /// <summary>
        /// Enable debug output while dumping by default
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool EnableDebug { get; set; } = SettingConstants.EnableDebugDefault;

        /// <summary>
        /// Enable verbose output while dumping by default
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool EnableVerbose { get; set; } = SettingConstants.EnableVerboseDefault;

        /// <summary>
        /// Enable force dumping of media by default
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool ForceDumping { get; set; } = SettingConstants.ForceDumpingDefault;

        /// <summary>
        /// Default number of sector/subchannel rereads
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public int RereadCount { get; set; } = SettingConstants.RereadCountDefault;

        /// <summary>
        /// Strip personal data information from Aaru metadata by default
        /// </summary>
        /// <remarks>Version 1 and greater</remarks>
        public bool StripPersonalData { get; set; } = SettingConstants.StripPersonalDataDefault;
    }
}
