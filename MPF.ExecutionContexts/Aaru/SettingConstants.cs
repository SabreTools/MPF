namespace MPF.ExecutionContexts.Aaru
{
    public static class SettingConstants
    {
        public const string EnableDebug = "AaruEnableDebug";
        public const bool EnableDebugDefault = false;

        public const string EnableVerbose = "AaruEnableVerbose";
        public const bool EnableVerboseDefault = true;

        public const string ForceDumping = "AaruForceDumping";
        public const bool ForceDumpingDefault = true;

        public const string RereadCount = "AaruRereadCount";
        public const int RereadCountDefault = 5;

        public const string StripPersonalData = "AaruStripPersonalData";
        public const bool StripPersonalDataDefault = false;
    }
}
