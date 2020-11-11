namespace MPF.DD
{
    /// <summary>
    /// Top-level commands for DD
    /// </summary>
    public static class CommandStrings
    {
        public const string List = "--list";
    }

    /// <summary>
    /// Dumping flags for DD
    /// </summary>
    public static class FlagStrings
    {
        // Boolean flags
        public const string Progress = "--progress";
        public const string Size = "--size";

        // Int64 flags
        public const string BlockSize = "bs";
        public const string Count = "count";
        public const string Seek = "seek";
        public const string Skip = "skip";

        // String flags
        public const string Filter = "--filter";
        public const string InputFile = "if";
        public const string OutputFile = "of";
    }
}