namespace MPF.ExecutionContexts.Aaru
{
    /// <summary>
    /// Top-level commands for Aaru
    /// </summary>
    public static class CommandStrings
    {
        public const string NONE = "";

        #region Archive Family

        public const string ArchivePrefixShort = "arc";
        public const string ArchivePrefixLong = "archive";
        public const string ArchiveInfo = "info";

        #endregion

        #region Database Family

        public const string DatabasePrefixShort = "db";
        public const string DatabasePrefixLong = "database";
        public const string DatabaseStats = "stats";
        public const string DatabaseUpdate = "update";

        #endregion

        #region Device Family

        public const string DevicePrefixShort = "dev";
        public const string DevicePrefixLong = "device";
        public const string DeviceInfo = "info";
        public const string DeviceList = "list";
        public const string DeviceReport = "report";

        #endregion

        #region Filesystem Family

        public const string FilesystemPrefixShort = "fi";
        public const string FilesystemPrefixShortAlt = "fs";
        public const string FilesystemPrefixLong = "filesystem";
        public const string FilesystemExtract = "extract";
        public const string FilesystemInfo = "info";
        public const string FilesystemListShort = "ls";
        public const string FilesystemListLong = "list";
        public const string FilesystemOptions = "options";

        #endregion

        #region Image Family

        public const string ImagePrefixShort = "i";
        public const string ImagePrefixLong = "image";
        public const string ImageChecksumShort = "chk";
        public const string ImageChecksumLong = "checksum";
        public const string ImageCompareShort = "cmp";
        public const string ImageCompareLong = "compare";
        public const string ImageConvert = "convert";
        public const string ImageCreateSidecar = "create-sidecar";
        public const string ImageDecode = "decode";
        public const string ImageEntropy = "entropy";
        public const string ImageInfo = "info";
        public const string ImageOptions = "options";
        public const string ImagePrint = "print";
        public const string ImageVerify = "verify";

        #endregion

        #region Media Family

        public const string MediaPrefixShort = "m";
        public const string MediaPrefixLong = "media";
        public const string MediaDump = "dump";
        public const string MediaInfo = "info";
        public const string MediaScan = "scan";

        #endregion

        #region Standalone Commands

        public const string Configure = "configure";
        public const string Formats = "formats";
        public const string ListEncodings = "list-encodings";
        public const string ListNamespaces = "list-namespaces";
        public const string Remote = "remote";

        #endregion
    }
}