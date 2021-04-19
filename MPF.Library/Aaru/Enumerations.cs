namespace MPF.Aaru
{
    /// <summary>
    /// Supported Aaru commands
    /// </summary>
    public enum Command : int
    {
        NONE = 0,

        // Database Family
        DatabaseStats,
        DatabaseUpdate,

        // Device Family
        DeviceInfo,
        DeviceList,
        DeviceReport,

        // Filesystem Family
        FilesystemExtract,
        FilesystemList,
        FilesystemOptions,

        // Image Family
        ImageAnalyze,
        ImageChecksum,
        ImageCompare,
        ImageConvert,
        ImageCreateSidecar,
        ImageDecode,
        ImageEntropy,
        ImageInfo,
        ImageOptions,
        ImagePrint,
        ImageVerify,

        // Media Family
        MediaDump,
        MediaInfo,
        MediaScan,

        // Standalone Commands
        Configure,
        Formats,
        ListEncodings,
        ListNamespaces,
        Remote,
    }
}