namespace MPF.Aaru
{
    /// <summary>
    /// Top-level commands for Aaru
    /// </summary>
    public static class CommandStrings
    {
        public const string NONE = "";

        // Archive Family
        public const string ArchivePrefixShort = "arc";
        public const string ArchivePrefixLong = "archive";
        public const string ArchiveInfo = "info";

        // Database Family
        public const string DatabasePrefixShort = "db";
        public const string DatabasePrefixLong = "database";
        public const string DatabaseStats = "stats";
        public const string DatabaseUpdate = "update";

        // Device Family
        public const string DevicePrefixShort = "dev";
        public const string DevicePrefixLong = "device";
        public const string DeviceInfo = "info";
        public const string DeviceList = "list";
        public const string DeviceReport = "report";

        // Filesystem Family
        public const string FilesystemPrefixShort = "fi";
        public const string FilesystemPrefixShortAlt = "fs";
        public const string FilesystemPrefixLong = "filesystem";
        public const string FilesystemExtract = "extract";
        public const string FilesystemInfo = "info";
        public const string FilesystemListShort = "ls";
        public const string FilesystemListLong = "list";
        public const string FilesystemOptions = "options";

        // Image Family
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

        // Media Family
        public const string MediaPrefixShort = "m";
        public const string MediaPrefixLong = "media";
        public const string MediaDump = "dump";
        public const string MediaInfo = "info";
        public const string MediaScan = "scan";

        // Standalone Commands
        public const string Configure = "configure";
        public const string Formats = "formats";
        public const string ListEncodings = "list-encodings";
        public const string ListNamespaces = "list-namespaces";
        public const string Remote = "remote";
    }

    /// <summary>
    /// Supported encodings for Aaru
    /// </summary>
    /// TODO: Use to verify encoding settings
    public static class EncodingStrings
    {
        public const string ArabicMac = "x-mac-arabic";
        public const string AtariASCII = "atascii";
        public const string CentralEuropeanMac = "x-mac-ce";
        public const string CommodorePETSCII = "petscii";
        public const string CroatianMac = "x-mac-croatian";
        public const string CyrillicMac = "x-mac-cryillic";
        public const string FarsiMac = "x-mac-farsi";
        public const string GreekMac = "x-mac-greek";
        public const string HebrewMac = "x-mac-hebrew";
        public const string RomanianMac = "x-mac-romanian";
        public const string SinclairZXSpectrum = "spectrum";
        public const string SinclairZX80 = "zx80";
        public const string SinclairZX81 = "zx81";
        public const string TurkishMac = "x-mac-turkish";
        public const string UkrainianMac = "x-mac-ukrainian";
        public const string Unicode = "utf-16";
        public const string UnicodeBigEndian = "utf-16BE";
        public const string UnicodeUTF32BigEndian = "utf-32BE";
        public const string UnicodeUTF32 = "utf-32";
        public const string UnicodeUTF7 = "utf-7";
        public const string UnicodeUTF8 = "utf-8";
        public const string USASCII = "us-ascii";
        public const string WesternEuropeanAppleII = "apple2";
        public const string WesternEuropeanAppleIIc = "apple2c";
        public const string WesternEuropeanAppleIIe = "apple2e";
        public const string WesternEuropeanAppleIIgs = "apple2gs";
        public const string WesternEuropeanAppleLisa = "lisa";
        public const string WesternEuropeanAtariST = "atarist";
        public const string WesternEuropeanGEM = "gem";
        public const string WesternEuropeanGEOS = "geos";
        public const string WesternEuropeanISO = "iso-8859-1";
        public const string WesternEuropeanMac = "macintosh";
        public const string WesternEuropeanRadix50 = "radix50";
    }

    /// <summary>
    /// Dumping flags for Aaru
    /// </summary>
    public static class FlagStrings
    {
        // Boolean flags
        public const string Adler32Short = "-a";
        public const string Adler32Long = "--adler32";
        public const string ClearLong = "--clear";
        public const string ClearAllLong = "--clear-all";
        public const string CRC16Long = "--crc16";
        public const string CRC32Short = "-c";
        public const string CRC32Long = "--crc32";
        public const string CRC64Long = "--crc64";
        public const string DebugShort = "-d";
        public const string DebugLong = "--debug";
        public const string DiskTagsShort = "-f";
        public const string DiskTagsLong = "--disk-tags";
        public const string DuplicatedSectorsShort = "-p";
        public const string DuplicatedSectorsLong = "--duplicated-sectors";
        public const string EjectLong = "--eject";
        public const string ExtendedAttributesShort = "-x";
        public const string ExtendedAttributesLong = "--xattrs";
        public const string FilesystemsShort = "-f";
        public const string FilesystemsLong = "--filesystems";
        public const string FirstPregapLong = "--first-pregap";
        public const string FixOffsetLong = "--fix-offset";
        public const string FixSubchannelLong = "--fix-subchannel";
        public const string FixSubchannelCrcLong = "--fix-subchannel-crc";
        public const string FixSubchannelPositionLong = "--fix-subchannel-position";
        public const string Fletcher16Long = "--fletcher16";
        public const string Fletcher32Long = "--fletcher32";
        public const string ForceShort = "-f";
        public const string ForceLong = "--force";
        public const string GenerateSubchannelsLong = "--generate-subchannels";
        public const string HelpShort = "-h";
        public const string HelpShortAlt = "-?";
        public const string HelpLong = "--help";
        public const string LongFormatShort = "-l";
        public const string LongFormatLong = "--long-format";
        public const string LongSectorsShort = "-r";
        public const string LongSectorsLong = "--long-sectors";
        public const string MD5Short = "-m";
        public const string MD5Long = "--md5";
        public const string MetadataLong = "--metadata";
        public const string PartitionsShort = "-p";
        public const string PartitionsLong = "--partitions";
        public const string PauseLong = "--pause";
        public const string PersistentLong = "--persistent";
        public const string PrivateLong = "--private";
        public const string ResumeShort = "-r";
        public const string ResumeLong = "--resume";
        public const string RetrySubchannelLong = "--retry-subchannel";
        public const string SectorTagsShort = "-p";
        public const string SectorTagsLong = "--sector-tags";
        public const string SeparatedTracksShort = "-t";
        public const string SeparatedTracksLong = "--separated-tracks";
        public const string SHA1Short = "-s";
        public const string SHA1Long = "--sha1";
        public const string SHA256Long = "--sha256";
        public const string SHA384Long = "--sha384";
        public const string SHA512Long = "--sha512";
        public const string SkipCdiReadyHoleLong = "--skip-cdiready-hole";
        public const string SpamSumShort = "-f";
        public const string SpamSumLong = "--spamsum";
        public const string StopOnErrorShort = "-s";
        public const string StopOnErrorLong = "--stop-on-error";
        public const string StoreEncryptedLong = "--store-encrypted";
        public const string TapeShort = "-t";
        public const string TapeLong = "--tape";
        public const string TitleKeysLong = "--title-keys";
        public const string TrapDiscShort = "-t";
        public const string TrapDiscLong = "--trap-disc";
        public const string TrimLong = "--trim";
        public const string UseBufferedReadsLong = "--use-buffered-reads";
        public const string VerboseShort = "-v";
        public const string VerboseLong = "--verbose";
        public const string VerifyDiscShort = "-w";
        public const string VerifyDiscLong = "--verify-disc";
        public const string VerifySectorsShort = "-s";
        public const string VerifySectorsLong = "--verify-sectors";
        public const string VersionLong = "--version";
        public const string WholeDiscShort = "-w";
        public const string WholeDiscLong = "--whole-disc";

        // Int8 flags
        public const string SpeedLong = "--speed";

        // Int16 flags
        public const string RetryPassesShort = "-p";
        public const string RetryPassesLong = "--retry-passes";
        public const string WidthShort = "-w";
        public const string WidthLong = "--width";

        // Int32 flags
        public const string BlockSizeShort = "-b";
        public const string BlockSizeLong = "--block-size";
        public const string CountShort = "-c";
        public const string CountLong = "--count";
        public const string MaxBlocksLong = "--max-blocks";
        public const string MediaLastSequenceLong = "--media-lastsequence";
        public const string MediaSequenceLong = "--media-sequence";
        public const string SkipShort = "-k";
        public const string SkipLong = "--skip";

        // Int64 flags
        public const string LengthShort = "-l"; // or "all"
        public const string LengthLong = "--length"; // or "all"
        public const string StartShort = "-s";
        public const string StartLong = "--start";

        // String flags
        public const string CommentsLong = "--comments";
        public const string CreatorLong = "--creator";
        public const string DriveManufacturerLong = "--drive-manufacturer";
        public const string DriveModelLong = "--drive-model";
        public const string DriveRevisionLong = "--drive-revision";
        public const string DriveSerialLong = "--drive-serial";
        public const string EncodingShort = "-e";
        public const string EncodingLong = "--encoding";
        public const string FormatConvertShort = "-p";
        public const string FormatConvertLong = "--format";
        public const string FormatDumpShort = "-t";
        public const string FormatDumpLong = "--format";
        public const string GeometryShort = "-g";
        public const string GeometryLong = "--geometry";
        public const string ImgBurnLogShort = "-b";
        public const string ImgBurnLogLong = "--ibg-log";
        public const string MediaBarcodeLong = "--media-barcode";
        public const string MediaManufacturerLong = "--media-manufacturer";
        public const string MediaModelLong = "--media-model";
        public const string MediaPartNumberLong = "--media-partnumber";
        public const string MediaSerialLong = "--media-serial";
        public const string MediaTitleLong = "--media-title";
        public const string MHDDLogShort = "-m";
        public const string MHDDLogLong = "--mhdd-log";
        public const string NamespaceShort = "-n";
        public const string NamespaceLong = "--namespace";
        public const string OptionsShort = "-O";
        public const string OptionsLong = "--options";
        public const string OutputPrefixShort = "-w";
        public const string OutputPrefixLong = "--output-prefix";
        public const string ResumeFileShort = "-r";
        public const string ResumeFileLong = "--resume-file";
        public const string SubchannelLong = "--subchannel";
        public const string XMLSidecarShort = "-x";
        public const string XMLSidecarLong = "--cicm-xml";
    }

    /// <summary>
    /// Supported formats for Aaru
    /// </summary>
    /// TODO: Use to verify format settings
    public static class FormatStrings
    {
        // Supported filters
        public const string AppleDouble = "AppleDouble";
        public const string AppleSingle = "AppleSingle";
        public const string BZip2 = "BZip2";
        public const string GZip = "GZip";
        public const string LZip = "LZip";
        public const string MacBinary = "MacBinary";
        public const string NoFilter = "No filter";
        public const string PCExchange = "PCExchange";
        public const string XZ = "XZ";

        // Read-only media image formats
        public const string AppleDiskArchivalRetrievalTool = "Apple Disk Archival/Retrieval Tool";
        public const string AppleNewDiskImageFormat = "Apple New Disk Image Format";
        public const string AppleNIB = "Apple NIB";
        public const string BlindWrite4 = "BlindWrite 4";
        public const string BlindWrite5 = "BlindWrite 5";
        public const string CPCEMUDiskFileAndExtendedCPCDiskFile = "CPCEMU Disk-File and Extended CPC Disk-File";
        public const string D2FDiskImage = "d2f disk image";
        public const string D88DiskImage = "D88 Disk Image";
        public const string DIMDiskImage = "DIM Disk Image";
        public const string DiscFerret = "DiscFerret";
        public const string DiscJuggler = "DiscJuggler";
        public const string DreamcastGDIImage = "Dreamcast GDI image";
        public const string DunfieldsIMD = "Dunfield's IMD";
        public const string HDCopyDiskImage = "HD-Copy disk image";
        public const string KryoFluxSTREAM = "KryoFlux STREAM";
        public const string MAMECompressedHunksOfData = "MAME Compressed Hunks of Data";
        public const string MicrosoftVHDX = "Microsoft VHDX";
        public const string NeroBurningROMImage = "Nero Burning ROM image";
        public const string PartCloneDiskImage = "PartClone disk image";
        public const string PartimageDiskImage = "Partimage disk image";
        public const string SpectrumFloppyDiskImage = "Spectrum Floppy Disk Image";
        public const string SuperCardPro = "SuperCardPro";
        public const string SydexCopyQM = "Sydex CopyQM";
        public const string SydexTeleDisk = "Sydex TeleDisk";

        // Read/write media image formats
        public const string AaruFormat = "Aaru Format";
        public const string ACTApricotDiskImage = "ACT Apricot Disk Image";
        public const string Alcohol120MediaDescriptorStructure = "Alcohol 120% Media Descriptor Structure";
        public const string Anex86DiskImage = "Anex86 Disk Image";
        public const string Apple2InterleavedDiskImage = "Apple ][Interleaved Disk Image";
        public const string Apple2IMG = "Apple 2IMG";
        public const string AppleDiskCopy42 = "Apple DiskCopy 4.2";
        public const string AppleUniversalDiskImageFormat = "Apple Universal Disk Image Format";
        public const string BasicLisaUtility = "Basic Lisa Utility";
        public const string CDRDAOTocfile = "CDRDAO tocfile";
        public const string CDRWinCuesheet = "CDRWin cuesheet";
        public const string CisCopyDiskImageDCFile = "CisCopy Disk Image(DC-File)";
        public const string CloneCD = "CloneCD";
        public const string CopyTape = "CopyTape";
        public const string DigitalResearchDiskCopy = "Digital Research DiskCopy";
        public const string IBMSaveDskF = "IBM SaveDskF";
        public const string MAXIDiskImage = "MAXI Disk image";
        public const string ParallelsDiskImage = "Parallels disk image";
        public const string QEMUCopyOnWriteDiskImage = "QEMU Copy-On-Write disk image";
        public const string QEMUCopyOnWriteDiskImageV2 = "QEMU Copy-On-Write disk image v2";
        public const string QEMUEnhancedDiskImage = "QEMU Enhanced Disk image";
        public const string RawDiskImage = "Raw Disk Image";
        public const string RayAracheliansDiskIMage = "Ray Arachelian's Disk IMage";
        public const string RSIDEHardDiskImage = "RS-IDE Hard Disk Image";
        public const string T98HardDiskImage = "T98 Hard Disk Image";
        public const string T98NextNHDr0DiskImage = "T98-Next NHD r0 Disk Image";
        public const string Virtual98DiskImage = "Virtual98 Disk Image";
        public const string VirtualBoxDiskImage = "VirtualBox Disk Image";
        public const string VirtualPC = "VirtualPC";
        public const string VMwareDiskImage = "VMware disk image";

        // Supported filesystems for identification and information only
        public const string AcornAdvancedDiscFilingSystem = "Acorn Advanced Disc Filing System";
        public const string AlexanderOsipovDOSFileSystem = "Alexander Osipov DOS file system";
        public const string AmigaDOSFilesystem = "Amiga DOS filesystem";
        public const string AppleFileSystem = "Apple File System";
        public const string AppleHFSPlusFilesystem = "Apple HFS+ filesystem";
        public const string AppleHierarchicalFileSystem = "Apple Hierarchical File System";
        public const string AppleProDOSFilesystem = "Apple ProDOS filesystem";
        public const string AtheOSFilesystem = "AtheOS Filesystem";
        public const string BeFilesystem = "Be Filesystem";
        public const string BSDFastFileSystem = "BSD Fast File System(aka UNIX File System, UFS)";
        public const string BTreeFileSystem = "B-tree file system";
        public const string CommodoreFileSystem = "Commodore file system";
        public const string CramFilesystem = "Cram filesystem";
        public const string DumpEightPlugin = "dump(8) Plugin";
        public const string ECMA67 = "ECMA-67";
        public const string ExtentFileSystemPlugin = "Extent File System Plugin";
        public const string F2FSPlugin = "F2FS Plugin";
        public const string Files11OnDiskStructure = "Files-11 On-Disk Structure";
        public const string FossilFilesystemPlugin = "Fossil Filesystem Plugin";
        public const string HAMMERFilesystem = "HAMMER Filesystem";
        public const string HighPerformanceOpticalFileSystem = "High Performance Optical File System";
        public const string HPLogicalInterchangeFormatPlugin = "HP Logical Interchange Format Plugin";
        public const string JFSPlugin = "JFS Plugin";
        public const string LinuxExtendedFilesystem = "Linux extended Filesystem";
        public const string LinuxExtendedFilesystem234 = "Linux extended Filesystem 2, 3 and 4";
        public const string LocusFilesystemPlugin = "Locus Filesystem Plugin";
        public const string MicroDOSFileSystem = "MicroDOS file system";
        public const string MicrosoftExtendedFileAllocationTable = "Microsoft Extended File Allocation Table";
        public const string MinixFilesystem = "Minix Filesystem";
        public const string NewTechnologyFileSystem = "New Technology File System(NTFS)";
        public const string NILFS2Plugin = "NILFS2 Plugin";
        public const string NintendoOpticalFilesystems = "Nintendo optical filesystems";
        public const string OS2HighPerformanceFileSystem = "OS/2 High Performance File System";
        public const string OS9RandomBlockFilePlugin = "OS-9 Random Block File Plugin";
        public const string PCEngineCDPlugin = "PC Engine CD Plugin";
        public const string PCFXPlugin = "PC-FX Plugin";
        public const string ProfessionalFileSystem = "Professional File System";
        public const string QNX4Plugin = "QNX4 Plugin";
        public const string QNX6Plugin = "QNX6 Plugin";
        public const string ReiserFilesystemPlugin = "Reiser Filesystem Plugin";
        public const string Reiser4FilesystemPlugin = "Reiser4 Filesystem Plugin";
        public const string ResilientFileSystemPlugin = "Resilient File System plugin";
        public const string RT11FileSystem = "RT-11 file system";
        public const string SmartFileSystem = "SmartFileSystem";
        public const string SolarOSFilesystem = "Solar_OS filesystem";
        public const string SquashFilesystem = "Squash filesystem";
        public const string UNICOSFilesystemPlugin = "UNICOS Filesystem Plugin";
        public const string UniversalDiskFormat = "Universal Disk Format";
        public const string UNIXBootFilesystem = "UNIX Boot filesystem";
        public const string UNIXSystemVFilesystem = "UNIX System V filesystem";
        public const string VeritasFilesystem = "Veritas filesystem";
        public const string VMwareFilesystem = "VMware filesystem";
        public const string XFSFilesystemPlugin = "XFS Filesystem Plugin";
        public const string XiaFilesystem = "Xia filesystem";
        public const string ZFSFilesystemPlugin = "ZFS Filesystem Plugin";

        // Supported filesystems that can read their contents
        public const string AppleDOSFileSystem = "Apple DOS File System";
        public const string AppleLisaFileSystem = "Apple Lisa File System";
        public const string AppleMacintoshFileSystem = "Apple Macintosh File System";
        public const string CPMFileSystem = "CP/M File System";
        public const string FATXFilesystemPlugin = "FATX Filesystem Plugin";
        public const string ISO9660Filesystem = "ISO9660 Filesystem";
        public const string MicrosoftFileAllocationTable = "Microsoft File Allocation Table";
        public const string OperaFilesystemPlugin = "Opera Filesystem Plugin";
        public const string UCSDPascalFilesystem = "U.C.S.D.Pascal filesystem";

        // Supported partitioning schemes
        public const string AcornFileCorePartitions = "Acorn FileCore partitions";
        public const string ACTApricotPartitions = "ACT Apricot partitions";
        public const string AmigaRigidDiskBlock = "Amiga Rigid Disk Block";
        public const string ApplePartitionMap = "Apple Partition Map";
        public const string AtariPartitions = "Atari partitions";
        public const string BSDDisklabel = "BSD disklabel";
        public const string DECDisklabel = "DEC disklabel";
        public const string DragonFlyBSD64bitDisklabel = "DragonFly BSD 64-bit disklabel";
        public const string GUIDPartitionTable = "GUID Partition Table";
        public const string Human68kPartitions = "Human 68k partitions";
        public const string MasterBootRecord = "Master Boot Record";
        public const string NECPC9800PartitionTable = "NEC PC-9800 partition table";
        public const string NeXTDisklabel = "NeXT Disklabel";
        public const string Plan9PartitionTable = "Plan9 partition table";
        public const string RioKarmaPartitioning = "Rio Karma partitioning";
        public const string SGIDiskVolumeHeader = "SGI Disk Volume Header";
        public const string SunDisklabel = "Sun Disklabel";
        public const string UNIXHardwired = "UNIX hardwired";
        public const string UNIXVTOC = "UNIX VTOC";
        public const string XboxPartitioning = "Xbox partitioning";
        public const string XENIX = "XENIX";
    }

    /// <summary>
    /// Supported namespaces for Aaru
    /// </summary>
    /// TODO: Use to verify namespace settings
    public static class NamespaceStrings
    {
        // Namespaces for Apple Lisa File System
        public const string LisaOfficeSystem = "office";
        public const string LisaPascalWorkshop = "workshop"; // Default

        // Namespaces for ISO9660 Filesystem
        public const string JolietVolumeDescriptor = "joliet"; // Default
        public const string PrimaryVolumeDescriptor = "normal";
        public const string PrimaryVolumeDescriptorwithEncoding = "romeo";
        public const string RockRidge = "rrip";
        public const string PrimaryVolumeDescriptorVersionSuffix = "vms";

        // Namespaces for Microsoft File Allocation Table
        public const string DOS83UpperCase = "dos";
        public const string LFNWhenAvailableWithFallback = "ecs"; // Default
        public const string LongFileNames = "lfn";
        public const string WindowsNT83MixedCase = "nt";
        public const string OS2Extended = "os2";
    }

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