namespace DICUI.Data
{
    /// <summary>
    /// Top-level commands for DiscImageChef
    /// </summary>
    public static class ChefCommandStrings
    {
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
        public const string FilesystemListShort = "ls";
        public const string FilesystemListLong = "list";
        public const string FilesystemOptions = "options";

        // Image Family
        public const string ImagePrefixShort = "i";
        public const string ImagePrefixLong = "image";
        public const string ImageAnalyze = "analyze";
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
    /// Supported encodings for DiscImageChef
    /// </summary>
    public static class ChefEncodingStrings
    {
        public static string ArabicMac = "x-mac-arabic";
        public static string AtariASCII = "atascii";
        public static string CentralEuropeanMac = "x-mac-ce";
        public static string CommodorePETSCII = "petscii";
        public static string CroatianMac = "x-mac-croatian";
        public static string CyrillicMac = "x-mac-cryillic";
        public static string FarsiMac = "x-mac-farsi";
        public static string GreekMac = "x-mac-greek";
        public static string HebrewMac = "x-mac-hebrew";
        public static string RomanianMac = "x-mac-romanian";
        public static string SinclairZXSpectrum = "spectrum";
        public static string SinclairZX80 = "zx80";
        public static string SinclairZX81 = "zx81";
        public static string TurkishMac = "x-mac-turkish";
        public static string UkrainianMac = "x-mac-ukrainian";
        public static string Unicode = "utf-16";
        public static string UnicodeBigEndian = "utf-16BE";
        public static string UnicodeUTF32BigEndian = "utf-32BE";
        public static string UnicodeUTF32 = "utf-32";
        public static string UnicodeUTF7 = "utf-7";
        public static string UnicodeUTF8 = "utf-8";
        public static string USASCII = "us-ascii";
        public static string WesternEuropeanAppleII = "apple2";
        public static string WesternEuropeanAppleIIc = "apple2c";
        public static string WesternEuropeanAppleIIe = "apple2e";
        public static string WesternEuropeanAppleIIgs = "apple2gs";
        public static string WesternEuropeanAppleLisa = "lisa";
        public static string WesternEuropeanAtariST = "atarist";
        public static string WesternEuropeanGEM = "gem";
        public static string WesternEuropeanGEOS = "geos";
        public static string WesternEuropeanISO = "iso-8859-1";
        public static string WesternEuropeanMac = "macintosh";
        public static string WesternEuropeanRadix50 = "radix50";
    }

    /// <summary>
    /// Dumping flags for DiscImageChef
    /// </summary>
    public static class ChefFlagStrings
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
        public const string ExtendedAttributesShort = "-x";
        public const string ExtendedAttributesLong = "--xattrs";
        public const string FilesystemsShort = "-f";
        public const string FilesystemsLong = "--filesystems";
        public const string FirstPregapLong = "--first-pregap";
        public const string FixOffsetLong = "--fix-offset";
        public const string Fletcher16Long = "--fletcher16";
        public const string Fletcher32Long = "--fletcher32";
        public const string ForceShort = "-f";
        public const string ForceLong = "--force";
        public const string LongFormatShort = "-l";
        public const string LongFormatLong = "--long-format";
        public const string LongSectorsShort = "-r";
        public const string LongSectorsLong = "--long-sectors";
        public const string MD5Short = "-m";
        public const string MD5Long = "--md5";
        public const string MetadataLong = "--metadata";
        public const string PartitionsShort = "-p";
        public const string PartitionsLong = "--partitions";
        public const string PersistentLong = "--persistent";
        public const string ResumeShort = "-r";
        public const string ResumeLong = "--resume";
        public const string SectorTagsShort = "-p";
        public const string SectorTagsLong = "--sector-tags";
        public const string SeparatedTracksShort = "-t";
        public const string SeparatedTracksLong = "--separated-tracks";
        public const string SHA1Short = "-s";
        public const string SHA1Long = "--sha1";
        public const string SHA256Long = "--sha256";
        public const string SHA384Long = "--sha384";
        public const string SHA512Long = "--sha512";
        public const string SpamSumShort = "-f";
        public const string SpamSumLong = "--spamsum";
        public const string StopOnErrorShort = "-s";
        public const string StopOnErrorLong = "--stop-on-error";
        public const string TapeShort = "-t";
        public const string TapeLong = "--tape";
        public const string TrimLong = "--trim";
        public const string VerboseShort = "-v";
        public const string VerboseLong = "--verbose";
        public const string VerifyDiscShort = "-w";
        public const string VerifyDiscLong = "--verify-disc";
        public const string VerifySectorsShort = "-s";
        public const string VerifySectorsLong = "--verify-sectors";
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
    /// Supported formats for DiscImageChef
    /// </summary>
    public static class ChefFormatStrings
    {
        // Supported filters
        public static string AppleDouble = "AppleDouble";
        public static string AppleSingle = "AppleSingle";
        public static string BZip2 = "BZip2";
        public static string GZip = "GZip";
        public static string LZip = "LZip";
        public static string MacBinary = "MacBinary";
        public static string NoFilter = "No filter";
        public static string PCExchange = "PCExchange";
        public static string XZ = "XZ";

        // Read-only media image formats
        public static string AppleDiskArchivalRetrievalTool = "Apple Disk Archival/Retrieval Tool";
        public static string AppleNewDiskImageFormat = "Apple New Disk Image Format";
        public static string AppleNIB = "Apple NIB";
        public static string BlindWrite4 = "BlindWrite 4";
        public static string BlindWrite5 = "BlindWrite 5";
        public static string CPCEMUDiskFileAndExtendedCPCDiskFile = "CPCEMU Disk-File and Extended CPC Disk-File";
        public static string D2FDiskImage = "d2f disk image";
        public static string D88DiskImage = "D88 Disk Image";
        public static string DIMDiskImage = "DIM Disk Image";
        public static string DiscFerret = "DiscFerret";
        public static string DiscJuggler = "DiscJuggler";
        public static string DreamcastGDIImage = "Dreamcast GDI image";
        public static string DunfieldsIMD = "Dunfield's IMD";
        public static string HDCopyDiskImage = "HD-Copy disk image";
        public static string KryoFluxSTREAM = "KryoFlux STREAM";
        public static string MAMECompressedHunksOfData = "MAME Compressed Hunks of Data";
        public static string MicrosoftVHDX = "Microsoft VHDX";
        public static string NeroBurningROMImage = "Nero Burning ROM image";
        public static string PartCloneDiskImage = "PartClone disk image";
        public static string PartimageDiskImage = "Partimage disk image";
        public static string SpectrumFloppyDiskImage = "Spectrum Floppy Disk Image";
        public static string SuperCardPro = "SuperCardPro";
        public static string SydexCopyQM = "Sydex CopyQM";
        public static string SydexTeleDisk = "Sydex TeleDisk";

        // Read/write media image formats
        public static string ACTApricotDiskImage = "ACT Apricot Disk Image";
        public static string Alcohol120MediaDescriptorStructure = "Alcohol 120% Media Descriptor Structure";
        public static string Anex86DiskImage = "Anex86 Disk Image";
        public static string Apple2InterleavedDiskImage = "Apple ][Interleaved Disk Image";
        public static string Apple2IMG = "Apple 2IMG";
        public static string AppleDiskCopy42 = "Apple DiskCopy 4.2";
        public static string AppleUniversalDiskImageFormat = "Apple Universal Disk Image Format";
        public static string BasicLisaUtility = "Basic Lisa Utility";
        public static string CDRDAOTocfile = "CDRDAO tocfile";
        public static string CDRWinCuesheet = "CDRWin cuesheet";
        public static string CisCopyDiskImageDCFile = "CisCopy Disk Image(DC-File)";
        public static string CloneCD = "CloneCD";
        public static string CopyTape = "CopyTape";
        public static string DigitalResearchDiskCopy = "Digital Research DiskCopy";
        public static string DiscImageChefFormat = "DiscImageChef format";
        public static string IBMSaveDskF = "IBM SaveDskF";
        public static string MAXIDiskImage = "MAXI Disk image";
        public static string ParallelsDiskImage = "Parallels disk image";
        public static string QEMUCopyOnWriteDiskImage = "QEMU Copy-On-Write disk image";
        public static string QEMUCopyOnWriteDiskImageV2 = "QEMU Copy-On-Write disk image v2";
        public static string QEMUEnhancedDiskImage = "QEMU Enhanced Disk image";
        public static string RawDiskImage = "Raw Disk Image";
        public static string RayAracheliansDiskIMage = "Ray Arachelian's Disk IMage";
        public static string RSIDEHardDiskImage = "RS-IDE Hard Disk Image";
        public static string T98HardDiskImage = "T98 Hard Disk Image";
        public static string T98NextNHDr0DiskImage = "T98-Next NHD r0 Disk Image";
        public static string Virtual98DiskImage = "Virtual98 Disk Image";
        public static string VirtualBoxDiskImage = "VirtualBox Disk Image";
        public static string VirtualPC = "VirtualPC";
        public static string VMwareDiskImage = "VMware disk image";

        // Supported filesystems for identification and information only
        public static string AcornAdvancedDiscFilingSystem = "Acorn Advanced Disc Filing System";
        public static string AlexanderOsipovDOSFileSystem = "Alexander Osipov DOS file system";
        public static string AmigaDOSFilesystem = "Amiga DOS filesystem";
        public static string AppleFileSystem = "Apple File System";
        public static string AppleHFSPlusFilesystem = "Apple HFS+ filesystem";
        public static string AppleHierarchicalFileSystem = "Apple Hierarchical File System";
        public static string AppleProDOSFilesystem = "Apple ProDOS filesystem";
        public static string AtheOSFilesystem = "AtheOS Filesystem";
        public static string BeFilesystem = "Be Filesystem";
        public static string BSDFastFileSystem = "BSD Fast File System(aka UNIX File System, UFS)";
        public static string BTreeFileSystem = "B-tree file system";
        public static string CommodoreFileSystem = "Commodore file system";
        public static string CramFilesystem = "Cram filesystem";
        public static string DumpEightPlugin = "dump(8) Plugin";
        public static string ECMA67 = "ECMA-67";
        public static string ExtentFileSystemPlugin = "Extent File System Plugin";
        public static string F2FSPlugin = "F2FS Plugin";
        public static string Files11OnDiskStructure = "Files-11 On-Disk Structure";
        public static string FossilFilesystemPlugin = "Fossil Filesystem Plugin";
        public static string HAMMERFilesystem = "HAMMER Filesystem";
        public static string HighPerformanceOpticalFileSystem = "High Performance Optical File System";
        public static string HPLogicalInterchangeFormatPlugin = "HP Logical Interchange Format Plugin";
        public static string JFSPlugin = "JFS Plugin";
        public static string LinuxExtendedFilesystem = "Linux extended Filesystem";
        public static string LinuxExtendedFilesystem234 = "Linux extended Filesystem 2, 3 and 4";
        public static string LocusFilesystemPlugin = "Locus Filesystem Plugin";
        public static string MicroDOSFileSystem = "MicroDOS file system";
        public static string MicrosoftExtendedFileAllocationTable = "Microsoft Extended File Allocation Table";
        public static string MinixFilesystem = "Minix Filesystem";
        public static string NewTechnologyFileSystem = "New Technology File System(NTFS)";
        public static string NILFS2Plugin = "NILFS2 Plugin";
        public static string NintendoOpticalFilesystems = "Nintendo optical filesystems";
        public static string OS2HighPerformanceFileSystem = "OS/2 High Performance File System";
        public static string OS9RandomBlockFilePlugin = "OS-9 Random Block File Plugin";
        public static string PCEngineCDPlugin = "PC Engine CD Plugin";
        public static string PCFXPlugin = "PC-FX Plugin";
        public static string ProfessionalFileSystem = "Professional File System";
        public static string QNX4Plugin = "QNX4 Plugin";
        public static string QNX6Plugin = "QNX6 Plugin";
        public static string ReiserFilesystemPlugin = "Reiser Filesystem Plugin";
        public static string Reiser4FilesystemPlugin = "Reiser4 Filesystem Plugin";
        public static string ResilientFileSystemPlugin = "Resilient File System plugin";
        public static string RT11FileSystem = "RT-11 file system";
        public static string SmartFileSystem = "SmartFileSystem";
        public static string SolarOSFilesystem = "Solar_OS filesystem";
        public static string SquashFilesystem = "Squash filesystem";
        public static string UNICOSFilesystemPlugin = "UNICOS Filesystem Plugin";
        public static string UniversalDiskFormat = "Universal Disk Format";
        public static string UNIXBootFilesystem = "UNIX Boot filesystem";
        public static string UNIXSystemVFilesystem = "UNIX System V filesystem";
        public static string VeritasFilesystem = "Veritas filesystem";
        public static string VMwareFilesystem = "VMware filesystem";
        public static string XFSFilesystemPlugin = "XFS Filesystem Plugin";
        public static string XiaFilesystem = "Xia filesystem";
        public static string ZFSFilesystemPlugin = "ZFS Filesystem Plugin";

        // Supported filesystems that can read their contents
        public static string AppleDOSFileSystem = "Apple DOS File System";
        public static string AppleLisaFileSystem = "Apple Lisa File System";
        public static string AppleMacintoshFileSystem = "Apple Macintosh File System";
        public static string CPMFileSystem = "CP/M File System";
        public static string FATXFilesystemPlugin = "FATX Filesystem Plugin";
        public static string ISO9660Filesystem = "ISO9660 Filesystem";
        public static string MicrosoftFileAllocationTable = "Microsoft File Allocation Table";
        public static string OperaFilesystemPlugin = "Opera Filesystem Plugin";
        public static string UCSDPascalFilesystem = "U.C.S.D.Pascal filesystem";

        // Supported partitioning schemes
        public static string AcornFileCorePartitions = "Acorn FileCore partitions";
        public static string ACTApricotPartitions = "ACT Apricot partitions";
        public static string AmigaRigidDiskBlock = "Amiga Rigid Disk Block";
        public static string ApplePartitionMap = "Apple Partition Map";
        public static string AtariPartitions = "Atari partitions";
        public static string BSDDisklabel = "BSD disklabel";
        public static string DECDisklabel = "DEC disklabel";
        public static string DragonFlyBSD64bitDisklabel = "DragonFly BSD 64-bit disklabel";
        public static string GUIDPartitionTable = "GUID Partition Table";
        public static string Human68kPartitions = "Human 68k partitions";
        public static string MasterBootRecord = "Master Boot Record";
        public static string NECPC9800PartitionTable = "NEC PC-9800 partition table";
        public static string NeXTDisklabel = "NeXT Disklabel";
        public static string Plan9PartitionTable = "Plan9 partition table";
        public static string RioKarmaPartitioning = "Rio Karma partitioning";
        public static string SGIDiskVolumeHeader = "SGI Disk Volume Header";
        public static string SunDisklabel = "Sun Disklabel";
        public static string UNIXHardwired = "UNIX hardwired";
        public static string UNIXVTOC = "UNIX VTOC";
        public static string XboxPartitioning = "Xbox partitioning";
        public static string XENIX = "XENIX";
    }

    /// <summary>
    /// Supported namespaces for DiscImageChef
    /// </summary>
    public static class ChefNamespaceStrings
    {
        // Namespaces for Apple Lisa File System
        public static string LisaOfficeSystem = "office";
        public static string LisaPascalWorkshop = "workshop";

        // Namespaces for ISO9660 Filesystem
        public static string JolietVolumeDescriptor = "joliet";
        public static string PrimaryVolumeDescriptor = "normal";
        public static string PrimaryVolumeDescriptorwithEncoding = "romeo";
        public static string RockRidge = "rrip";
        public static string PrimaryVolumeDescriptorVersionSuffix = "vms";

        // Namespaces for Microsoft File Allocation Table
        public static string DOS = "dos";
        public static string LFNWhenAvailable = "ecs";
        public static string LongFileNames = "lfn";
        public static string WindowsNT = "nt";
        public static string OS2Extended = "os2";
    }

    /// <summary>
    /// Supported options for DiscImageChef
    /// </summary>
    public static class ChefOptionStrings
    {
        // ACT Apricot Disk Image
        public static string ACTApricotDiskImageCompress = "compress"; // boolean, default false
        
        // Apple DiskCopy 4.2
        public static string AppleDiskCopyMacOSX = "macosx"; // boolean, default false
        
        // CDRDAO tocfile
        public static string CDRDAOTocfileSeparate = "separate"; // boolean, default false
        
        // CDRWin cuesheet
        public static string CDRWinCuesheetSeparate = "separate"; // boolean, default false
        
        // DiscImageChef format
        public static string DiscImageChefDeduplicate = "deduplicate"; // boolean, default true
        public static string DiscImageChefDictionary = "dictionary"; // number, default 33554432
        public static string DiscImageChefMaxDDTSize = "max_ddt_size"; // number, default 256
        public static string DiscImageChefMD5 = "md5"; // boolean, default false
        public static string DiscImageChefNoCompress = "nocompress"; // boolean, default false
        public static string DiscImageChefSectorsPerBlock = "sectors_per_block"; // number, default 4096
        public static string DiscImageChefSHA1 = "sha1"; // boolean, default false
        public static string DiscImageChefSHA256 = "sha256"; // boolean, default false
        public static string DiscImageChefSpamSum = "spamsum"; // boolean, default false

        // VMware disk image
        public static string VMwareDiskImageAdapterType = "adapter_type"; // string, default ide
        public static string VMwareDiskImageHWVersion = "hwversion"; // number, default 4
        public static string VMwareDiskImageSparse = "sparse"; // boolean, default false
        public static string VMwareDiskImageSplit = "split"; // boolean, default false
    }

    /// <summary>
    /// Top-level commands for DiscImageCreator
    /// </summary>
    public static class CreatorCommandStrings
    {
        public const string Audio = "audio";
        public const string BluRay = "bd";
        public const string Close = "close";
        public const string CompactDisc = "cd";
        public const string Data = "data";
        public const string DigitalVideoDisc = "dvd";
        public const string Disk = "disk";
        public const string DriveSpeed = "ls";
        public const string Eject = "eject";
        public const string Floppy = "fd";
        public const string GDROM = "gd";
        public const string MDS = "mds";
        public const string Merge = "merge";
        public const string Reset = "reset";
        public const string SACD = "sacd";
        public const string Start = "start";
        public const string Stop = "stop";
        public const string Sub = "sub";
        public const string Swap = "swap";
        public const string XBOX = "xbox";
        public const string XBOXSwap = "xboxswap";
        public const string XGD2Swap = "xgd2swap";
        public const string XGD3Swap = "xgd3swap";
    }

    /// <summary>
    /// Dumping flags for DiscImageCreator
    /// </summary>
    public static class CreatorFlagStrings
    {
        public const string AddOffset = "/a";
        public const string AMSF = "/p";
        public const string AtariJaguar = "/aj";
        public const string BEOpcode = "/be";
        public const string C2Opcode = "/c2";
        public const string CopyrightManagementInformation = "/c";
        public const string D8Opcode = "/d8";
        public const string DisableBeep = "/q";
        public const string ForceUnitAccess = "/f";
        public const string MCN = "/m";
        public const string MultiSession = "/ms";
        public const string NoFixSubP = "/np";
        public const string NoFixSubQ = "/nq";
        public const string NoFixSubQLibCrypt = "/nl";
        public const string NoFixSubRtoW = "/nr";
        public const string NoFixSubQSecuROM = "/ns";
        public const string NoSkipSS = "/nss";
        public const string Raw = "/raw";
        public const string Reverse = "/r";
        public const string ScanAntiMod = "/am";
        public const string ScanFileProtect = "/sf";
        public const string ScanSectorProtect = "/ss";
        public const string SeventyFour = "/74";
        public const string SkipSector = "/sk";
        public const string SubchannelReadLevel = "/s";
        public const string VideoNow = "/vn";
        public const string VideoNowColor = "/vnc";
    }

    /// <summary>
    /// Template field values for submission info
    /// </summary>
    public static class Template
    {
        // Manual information

        public const string TitleField = "Title";
        public const string ForeignTitleField = "Foreign Title (Non-latin)";
        public const string DiscNumberField = "Disc Number / Letter";
        public const string DiscTitleField = "Disc Title";
        public const string SystemField = "System";
        public const string MediaTypeField = "Media Type";
        public const string CategoryField = "Category";
        public const string RegionField = "Region";
        public const string LanguagesField = "Languages";
        public const string PlaystationLanguageSelectionViaField = "Language Selection Via";
        public const string DiscSerialField = "Disc Serial";
        public const string BarcodeField = "Barcode";
        public const string CommentsField = "Comments";
        public const string ContentsField = "Contents";
        public const string VersionField = "Version";
        public const string EditionField = "Edition/Release";
        public const string PlayStation3WiiDiscKeyField = "Disc Key";
        public const string PlayStation3DiscIDField = "Disc ID";
        public const string GameCubeWiiBCAField = "BCA";
        public const string CopyProtectionField = "Copy Protection";
        public const string MasteringRingField = "Mastering Code (laser branded/etched)";
        public const string MasteringSIDField = "Mastering SID Code";
        public const string MouldSIDField = "Mould SID Code";
        public const string AdditionalMouldField = "Additional Mould";
        public const string ToolstampField = "Toolstamp or Mastering Code (engraved/stamped)";

        // Automatic Information

        public const string PVDField = "Primary Volume Descriptor (PVD)";
        public const string DATField = "DAT";
        public const string SizeField = "Size";
        public const string CRC32Field = "CRC32";
        public const string MD5Field = "MD5";
        public const string SHA1Field = "SHA1";
        public const string MatchingIDsField = "Matching IDs";
        public const string ErrorCountField = "Error Count";
        public const string CuesheetField = "Cuesheet";
        public const string SubIntentionField = "SubIntention Data (SecuROM/LibCrypt)";
        public const string WriteOffsetField = "Write Offset";
        public const string LayerbreakField = "Layerbreak";
        public const string EXEDateBuildDate = "EXE/Build Date";
        public const string HeaderField = "Header";
        public const string PICField = "Permanent Information & Control (PIC)";
        public const string PlayStationEDCField = "EDC";
        public const string PlayStationAntiModchipField = "Anti-modchip";
        public const string PlayStationLibCryptField = "LibCrypt";
        public const string XBOXDMIHash = "DMI.bin Hashes";
        public const string XBOXPFIHash = "PFI.bin Hashes";
        public const string XBOXSSHash = "SS.bin Hashes";
        public const string XBOXSSRanges = "Security Sector Ranges";
        public const string XBOXSSVersion = "Security Sector Version";

        // Default values

        public const string RequiredValue = "(REQUIRED)";
        public const string RequiredIfExistsValue = "(REQUIRED, IF EXISTS)";
        public const string OptionalValue = "(OPTIONAL)";
        public const string DiscNotDetected = "Disc Not Detected";
    }
}
