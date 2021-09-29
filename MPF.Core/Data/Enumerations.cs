namespace MPF.Core.Data
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
        DD,
        DiscImageCreator,

        // Verification support only
        CleanRip,
        DCDumper,
        UmdImageCreator,
    }
}
