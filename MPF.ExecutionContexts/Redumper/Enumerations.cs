namespace MPF.ExecutionContexts.Redumper
{
    /// <summary>
    /// Drive type option
    /// </summary>
    public enum DriveType
    {
        NONE = 0,

        GENERIC,
        PLEXTOR,
        LG_ASU8A,
        LG_ASU8B,
        LG_ASU8C,
        LG_ASU3,
        LG_ASU2,
    }

    /// <summary>
    /// Drive read method option
    /// </summary>
    public enum ReadMethod
    {
        NONE = 0,

        BE,
        D8,
    }

    /// <summary>
    /// Drive sector order option
    /// </summary>
    public enum SectorOrder
    {
        NONE = 0,

        DATA_C2_SUB,
        DATA_SUB_C2,
        DATA_SUB,
        DATA_C2,
    }
}
