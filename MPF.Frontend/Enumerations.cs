namespace MPF.Frontend
{
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
        USER,
        VERBOSE,
        ERROR,
        SECRET,
    }

    /// <summary>
    /// Drive read method option
    /// </summary>
    public enum RedumperReadMethod
    {
        NONE = 0,

        BE,
        D8,
        BE_CDDA,
    }

    /// <summary>
    /// Drive sector order option
    /// </summary>
    public enum RedumperSectorOrder
    {
        NONE = 0,

        DATA_C2_SUB,
        DATA_SUB_C2,
        DATA_SUB,
        DATA_C2,
    }
}