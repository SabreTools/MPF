namespace MPF.ExecutionContexts.Redumper
{
    /// <summary>
    /// Drive read method option
    /// </summary>
    public enum ReadMethod
    {
        NONE = 0,

        BE,
        D8,
        BE_CDDA, // Currently an alias for BE
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