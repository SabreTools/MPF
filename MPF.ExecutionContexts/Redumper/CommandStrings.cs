namespace MPF.ExecutionContexts.Redumper
{
    /// <summary>
    /// Top-level commands for Redumper
    /// </summary>
    public static class CommandStrings
    {
        public const string NONE = "";
        public const string CD = "cd";
        public const string DVD = "dvd"; // Synonym for CD
        public const string BluRay = "bd"; // Synonym for CD
        public const string SACD = "sacd"; // Synonym for CD
        public const string New = "new"; // Synonym for CD; Temporary command, to be removed later
        public const string Rings = "rings";
        public const string Dump = "dump";
        public const string DumpNew = "dumpnew"; // Temporary command, to be removed later
        public const string Refine = "refine";
        public const string RefineNew = "refinenew"; // Temporary command, to be removed later
        public const string Verify = "verify";
        public const string DVDKey = "dvdkey";
        public const string DVDIsoKey = "dvdisokey";
        public const string Protection = "protection";
        public const string Split = "split";
        public const string Hash = "hash";
        public const string Info = "info";
        public const string Skeleton = "skeleton";
    }
}