namespace MPF.Core.ExecutionContexts.Aaru
{
    /// <summary>
    /// Supported options for Aaru
    /// </summary>
    /// TODO: Use to verify option settings
    public static class OptionStrings
    {
        // Aaru format
        public const string AaruCompress = "compress"; // boolean, default true;
        public const string AaruDeduplicate = "deduplicate"; // boolean, default true
        public const string AaruDictionary = "dictionary"; // number, default 33554432
        public const string AaruMaxDDTSize = "max_ddt_size"; // number, default 256
        public const string AaruMD5 = "md5"; // boolean, default false
        public const string AaruSectorsPerBlock = "sectors_per_block"; // number, default 4096 [power of 2]
        public const string AaruSHA1 = "sha1"; // boolean, default false
        public const string AaruSHA256 = "sha256"; // boolean, default false
        public const string AaruSpamSum = "spamsum"; // boolean, default false

        // ACT Apricot Disk Image
        public const string ACTApricotDiskImageCompress = "compress"; // boolean, default false

        // Apple DiskCopy 4.2
        public const string AppleDiskCopyMacOSX = "macosx"; // boolean, default false

        // CDRDAO tocfile
        public const string CDRDAOTocfileSeparate = "separate"; // boolean, default false

        // CDRWin cuesheet
        public const string CDRWinCuesheetSeparate = "separate"; // boolean, default false

        // ISO9660 Filesystem
        public const string ISO9660FSUseEvd = "use_evd"; // boolean, default false
        public const string ISO9660FSUsePathTable = "use_path_table"; // boolean, default false
        public const string ISO9660FSUseTransTbl = "use_trans_tbl"; // boolean, default false

        // VMware disk image
        public const string VMwareDiskImageAdapterType = "adapter_type"; // string, default ide [ide, lsilogic, buslogic, legacyESX]
        public const string VMwareDiskImageHWVersion = "hwversion"; // number, default 4
        public const string VMwareDiskImageSparse = "sparse"; // boolean, default false
        public const string VMwareDiskImageSplit = "split"; // boolean, default false
    }
}