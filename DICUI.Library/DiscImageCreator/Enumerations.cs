namespace DICUI.DiscImageCreator
{
    /// <summary>
    /// Supported DiscImageCreator commands
    /// </summary>
    public enum Command
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
        XBOX,
        XBOXSwap,
        XGD2Swap,
        XGD3Swap,
    }

    /// <summary>
    /// Supported DiscImageCreator flags
    /// </summary>
    public enum Flag
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
        ForceUnitAccess,
        MultiSession,
        NoFixSubP,
        NoFixSubQ,
        NoFixSubQLibCrypt,
        NoFixSubRtoW,
        NoFixSubQSecuROM,
        NoSkipSS,
        Raw,
        Reverse,
        ScanAntiMod,
        ScanFileProtect,
        ScanSectorProtect,
        SeventyFour,
        SkipSector,
        SubchannelReadLevel,
        VideoNow,
        VideoNowColor,
    }
}