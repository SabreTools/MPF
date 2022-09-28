namespace MPF.Modules.Redumper
{
    /// <summary>
    /// Top-level commands for Redumper
    /// </summary>
    public static class CommandStrings
    {
        public const string NONE = "";
        public const string CD = "cd";
        public const string Dump = "dump";
        public const string Info = "info";
        public const string Protection = "protection";
        public const string Refine = "refine";
        public const string Split = "split";
    }

    /// <summary>
    /// Dumping flags for Redumper
    /// </summary>
    public static class FlagStrings
    {
        public const string AudioSilenceThreshold = "--audio-silence-threshold";
        public const string CDiCorrectOffset = "--cdi-correct-offset";
        public const string CDiReadyNormalize = "--cdi-ready-normalize";
        public const string DescrambleNew = "--descramble-new";
        public const string Drive = "--drive";
        public const string ForceOffset = "--force-offset";
        public const string ForceQTOC = "--force-qtoc";
        public const string ForceSplit = "--force-split";
        public const string ForceTOC = "--force-toc";
        public const string HelpLong = "--help";
        public const string HelpShort = "-h";
        public const string ISO9660Trim = "--iso9660-trim";
        public const string ImageName = "--image-name";
        public const string ImagePath = "--image-path";
        public const string LeaveUnchanged = "--leave-unchanged";
        public const string Overwrite = "--overwrite";
        public const string RefineSubchannel = "--refine-subchannel";
        public const string Retries = "--retries";
        public const string RingSize = "--ring-size";
        public const string Skip = "--skip";
        public const string SkipFill = "--skip-fill";
        public const string SkipLeadIn = "--skip-leadin";
        public const string SkipSize = "--skip-size";
        public const string Speed = "--speed";
        public const string StopLBA = "--stop-lba";
        public const string Unsupported = "--unsupported";
        public const string Verbose = "--verbose";
    }
}