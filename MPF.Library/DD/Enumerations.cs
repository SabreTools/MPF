namespace MPF.DD
{
    /// <summary>
    /// Supported DD commands
    /// </summary>
    public enum Command: int
    {
        NONE = 0, // For DD, this represents a normal dump
        List,
    }

    /// <summary>
    /// Supported DD flags
    /// </summary>
    public enum Flag : int
    {
        NONE = 0,

        // Boolean flags
        Progress,
        Size,

        // Int64 flags
        BlockSize,
        Count,
        Seek,
        Skip,

        // String flags
        Filter,
        InputFile,
        OutputFile,
    }
}