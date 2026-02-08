namespace MPF.Frontend
{
    /// <summary>
    /// Interface language
    /// </summary>
    public enum InterfaceLanguage
    {
        /// <summary>
        /// Default to auto-detecting language
        /// </summary>
        AutoDetect = 0,

        // Selectable languages (in Manual Window dropdown)
        English,
        French,
        German,
        Italian,
        Japanese,
        Korean,
        Polish,
        Russian,
        Spanish,
        Swedish,
        Ukrainian,

        // Hidden languages (not in Main Window dropdown)
        L337,
    }

    /// <summary>
    /// Drive type for dumping
    /// </summary>
    public enum InternalDriveType
    {
        Optical,
        Floppy,
        HardDisk,
        Removable,
    }

    /// <summary>
    /// Program that is being used to dump media
    /// </summary>
    public enum InternalProgram
    {
        NONE = 0,

        // Dumping support
        Aaru,
        DiscImageCreator,
        // Dreamdump,
        Redumper,

        // Verification support only
        CleanRip,
        PS3CFW,
        UmdImageCreator,
        XboxBackupCreator,
    }

    /// <summary>
    /// Log level for output
    /// </summary>
    public enum LogLevel
    {
        USER_GENERIC,
        USER_SUCCESS,
        VERBOSE,
        ERROR,
        SECRET,
    }
}
