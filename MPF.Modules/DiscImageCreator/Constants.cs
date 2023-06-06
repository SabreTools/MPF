namespace MPF.Modules.DiscImageCreator
{
    /// <summary>
    /// Top-level commands for DiscImageCreator
    /// </summary>
    public static class CommandStrings
    {
        public const string NONE = "";
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
        public const string Tape = "tape";
        public const string Version = "/v";
        public const string XBOX = "xbox";
        public const string XBOXSwap = "xboxswap";
        public const string XGD2Swap = "xgd2swap";
        public const string XGD3Swap = "xgd3swap";
    }

    /// <summary>
    /// Dumping flags for DiscImageCreator
    /// </summary>
    public static class FlagStrings
    {
        public const string AddOffset = "/a";
        public const string AMSF = "/p";
        public const string AtariJaguar = "/aj";
        public const string BEOpcode = "/be";
        public const string C2Opcode = "/c2";
        public const string CopyrightManagementInformation = "/c";
        public const string D8Opcode = "/d8";
        public const string DatExpand = "/d";
        public const string DisableBeep = "/q";
        public const string DVDReread = "/rr";
        public const string ExtractMicroSoftCabFile = "/mscf";
        public const string Fix = "/fix";
        public const string ForceUnitAccess = "/f";
        public const string MultiSectorRead = "/mr";
        public const string NoFixSubP = "/np";
        public const string NoFixSubQ = "/nq";
        public const string NoFixSubQLibCrypt = "/nl";
        public const string NoFixSubRtoW = "/nr";
        public const string NoFixSubQSecuROM = "/ns";
        public const string NoSkipSS = "/nss";
        public const string PadSector = "/ps";
        public const string Range = "/ra";
        public const string Raw = "/raw";
        public const string Resume = "/re";
        public const string Reverse = "/r";
        public const string ScanAntiMod = "/am";
        public const string ScanFileProtect = "/sf";
        public const string ScanSectorProtect = "/ss";
        public const string SeventyFour = "/74";
        public const string SkipSector = "/sk";
        public const string SubchannelReadLevel = "/s";
        public const string UseAnchorVolumeDescriptorPointer = "/avdp";
        public const string VideoNow = "/vn";
        public const string VideoNowColor = "/vnc";
        public const string VideoNowXP = "/vnx";
    }
}