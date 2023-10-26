namespace MPF.Core.Data
{
    /// <summary>
    /// Available hashing types
    /// </summary>
    public enum Hash
    {
        CRC32,
        CRC64,
        MD5,
        SHA1,
        SHA256,
        SHA384,
        SHA512,
        XxHash32,
        XxHash64,
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
        Redumper,

        // Verification support only
        CleanRip,
        DCDumper,
        UmdImageCreator,
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
}
