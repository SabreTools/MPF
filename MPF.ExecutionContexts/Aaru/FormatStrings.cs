namespace MPF.ExecutionContexts.Aaru
{
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
}