namespace MPF.DiscImageCreator
{
    /// <summary>
    /// Supported DiscImageCreator commands
    /// </summary>
    public enum Command : int
    {
        NONE = 0,
        Audio,
        BluRay,
        Close,
        CompactDisc,
        Data,
        DigitalVideoDisc,
        Disk,
        DriveSpeed,
        Eject,
        Floppy,
        GDROM,
        MDS,
        Merge,
        Reset,
        SACD,
        Start,
        Stop,
        Sub,
        Swap,
        Tape,
        XBOX,
        XBOXSwap,
        XGD2Swap,
        XGD3Swap,
    }

    /// <summary>
    /// Supported DiscImageCreator flags
    /// </summary>
    public enum Flag : int
    {
        NONE = 0,
        AddOffset,
        AMSF,
        AtariJaguar,
        BEOpcode,
        C2Opcode,
        CopyrightManagementInformation,
        D8Opcode,
        DisableBeep,
        ExtractMicroSoftCabFile,
        Fix,
        ForceUnitAccess,
        MultiSectorRead,
        MultiSession,
        NoFixSubP,
        NoFixSubQ,
        NoFixSubQLibCrypt,
        NoFixSubRtoW,
        NoFixSubQSecuROM,
        NoSkipSS,
        PadSector,
        Raw,
        Resume,
        Reverse,
        ScanAntiMod,
        ScanFileProtect,
        ScanSectorProtect,
        SeventyFour,
        SkipSector,
        SubchannelReadLevel,
        UseAnchorVolumeDescriptorPointer,
        VideoNow,
        VideoNowColor,
        VideoNowXP,
    }
}