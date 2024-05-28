namespace MPF.ExecutionContexts.Redumper
{
    public static class SettingConstants
    {
        public const string EnableDebug = "RedumperEnableDebug";
        public const bool EnableDebugDefault = false;

        public const string EnableLeadinRetry = "RedumperEnableLeadinRetry";
        public const bool EnableLeadinRetryDefault = false;

        public const string EnableVerbose = "RedumperEnableVerbose";
        public const bool EnableVerboseDefault = true;

        public const string LeadinRetryCount = "RedumperLeadinRetryCount";
        public const int LeadinRetryCountDefault = 4;

        public const string ReadMethod = "RedumperReadMethod";
        public static readonly string ReadMethodDefault = Redumper.ReadMethod.NONE.ToString();

        public const string RereadCount = "RedumperRereadCount";
        public const int RereadCountDefault = 20;

        public const string SectorOrder = "RedumperSectorOrder";
        public static readonly string SectorOrderDefault = Redumper.SectorOrder.NONE.ToString();

        public const string UseGenericDriveType = "RedumperUseGenericDriveType";
        public const bool UseGenericDriveTypeDefault = false;
    }
}