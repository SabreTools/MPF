namespace MPF.ExecutionContexts.Aaru
{
    /// <summary>
    /// Dumping flags for Aaru
    /// </summary>
    public static class FlagStrings
    {
        #region Precommand Flags

        public const string DebugShort = "-d";
        public const string DebugLong = "--debug";
        public const string HelpShort = "-h";
        public const string HelpShortAlt = "-?";
        public const string HelpLong = "--help";
        public const string VerboseShort = "-v";
        public const string VerboseLong = "--verbose";
        public const string VersionLong = "--version";

        #endregion

        #region Boolean flags

        public const string Adler32Short = "-a";
        public const string Adler32Long = "--adler32";
        public const string ClearLong = "--clear";
        public const string ClearAllLong = "--clear-all";
        public const string CRC16Long = "--crc16";
        public const string CRC32Short = "-c";
        public const string CRC32Long = "--crc32";
        public const string CRC64Long = "--crc64";
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
        public const string VerifyDiscShort = "-w";
        public const string VerifyDiscLong = "--verify-disc";
        public const string VerifySectorsShort = "-s";
        public const string VerifySectorsLong = "--verify-sectors";
        public const string WholeDiscShort = "-w";
        public const string WholeDiscLong = "--whole-disc";

        #endregion

        #region Int8 flags

        public const string SpeedLong = "--speed";

        #endregion

        #region Int16 flags

        public const string RetryPassesShort = "-p";
        public const string RetryPassesLong = "--retry-passes";
        public const string WidthShort = "-w";
        public const string WidthLong = "--width";

        #endregion

        #region Int32 flags

        public const string BlockSizeShort = "-b";
        public const string BlockSizeLong = "--block-size";
        public const string CountShort = "-c";
        public const string CountLong = "--count";
        public const string MaxBlocksLong = "--max-blocks";
        public const string MediaLastSequenceLong = "--media-lastsequence";
        public const string MediaSequenceLong = "--media-sequence";
        public const string SkipShort = "-k";
        public const string SkipLong = "--skip";

        #endregion

        #region Int64 flags

        public const string LengthShort = "-l"; // or "all"
        public const string LengthLong = "--length"; // or "all"
        public const string StartShort = "-s";
        public const string StartLong = "--start";

        #endregion

        #region String flags

        public const string CommentsLong = "--comments";
        public const string CreatorLong = "--creator";
        public const string DriveManufacturerLong = "--drive-manufacturer";
        public const string DriveModelLong = "--drive-model";
        public const string DriveRevisionLong = "--drive-revision";
        public const string DriveSerialLong = "--drive-serial";
        public const string EncodingShort = "-e";
        public const string EncodingLong = "--encoding";
        public const string FormatConvertShort = "-p";
        public const string FormatDumpShort = "-t";
        public const string FormatLong = "--format";
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

        #endregion
    }
}