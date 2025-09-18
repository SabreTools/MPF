namespace MPF.ExecutionContexts.Redumper
{
    public static class SettingConstants
    {
        public const string DriveType = "RedumperDriveType";
        public static readonly string DriveTypeDefault = Redumper.DriveType.NONE.ToString();

        public const string EnableSkeleton = "RedumperEnableSkeleton";
        public const bool EnableSkeletonDefault = true;

        public const string EnableVerbose = "RedumperEnableVerbose";
        public const bool EnableVerboseDefault = false;

        public const string LeadinRetryCount = "RedumperLeadinRetryCount";
        public const int LeadinRetryCountDefault = 4;

        public const string ReadMethod = "RedumperReadMethod";
        public static readonly string ReadMethodDefault = Redumper.ReadMethod.NONE.ToString();

        public const string RefineSectorMode = "RedumperRefineSectorMode";
        public const bool RefineSectorModeDefault = false;

        public const string RereadCount = "RedumperRereadCount";
        public const int RereadCountDefault = 20;

        public const string SectorOrder = "RedumperSectorOrder";
        public static readonly string SectorOrderDefault = Redumper.SectorOrder.NONE.ToString();
    }
}
