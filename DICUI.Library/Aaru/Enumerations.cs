namespace DICUI.Aaru
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

    /// <summary>
    /// Supported Aaru flags
    /// </summary>
    public enum Flag : int
    {
        NONE = 0,

        // Boolean flags
        Adler32,
        Clear,
        ClearAll,
        CRC16,
        CRC32,
        CRC64,
        Debug,
        DiskTags,
        DuplicatedSectors,
        Eject,
        ExtendedAttributes,
        Filesystems,
        FirstPregap,
        FixOffset,
        FixSubchannel,
        FixSubchannelCrc,
        FixSubchannelPosition,
        Fletcher16,
        Fletcher32,
        Force,
        GenerateSubchannels,
        Help,
        LongFormat,
        LongSectors,
        MD5,
        Metadata,
        Partitions,
        Persistent,
        Private,
        Resume,
        RetrySubchannel,
        SectorTags,
        SeparatedTracks,
        SHA1,
        SHA256,
        SHA384,
        SHA512,
        SkipCdiReadyHole,
        SpamSum,
        StopOnError,
        Tape,
        Trim,
        Verbose,
        VerifyDisc,
        VerifySectors,
        Version,
        WholeDisc,

        // Int8 flags
        Speed,

        // Int16 flags
        RetryPasses,
        Width,

        // Int32 flags
        BlockSize,
        Count,
        MediaLastSequence,
        MediaSequence,
        Skip,

        // Int64 flags
        Length,
        Start,

        // String flags
        Comments,
        Creator,
        DriveManufacturer,
        DriveModel,
        DriveRevision,
        DriveSerial,
        Encoding,
        FormatConvert,
        FormatDump,
        ImgBurnLog,
        MediaBarcode,
        MediaManufacturer,
        MediaModel,
        MediaPartNumber,
        MediaSerial,
        MediaTitle,
        MHDDLog,
        Namespace,
        Options,
        OutputPrefix,
        ResumeFile,
        Subchannel,
        XMLSidecar,
    }
}