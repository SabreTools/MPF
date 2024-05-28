namespace MPF.Core
{
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
