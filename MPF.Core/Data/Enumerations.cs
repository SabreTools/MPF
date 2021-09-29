namespace MPF.Core.Data
{
    /// <summary>
    /// Drive type for dumping
    /// </summary>
    public enum InternalDriveType
    {
        Optical,
        Floppy,
        HardDisk,
        Removable,
    }

    /// <summary>
    /// Program that is being used to dump media
    /// </summary>
    public enum InternalProgram
    {
        NONE = 0,

        // Dumping support
        Aaru,
        DD,
        DiscImageCreator,

        // Verification support only
        CleanRip,
        DCDumper,
        UmdImageCreator,
    }

    /// <summary>
    /// Physical media types
    /// </summary>
    /// <see cref="https://docs.microsoft.com/en-us/previous-versions/windows/desktop/cimwin32a/win32-physicalmedia"/>
    public enum PhysicalMediaType : ushort
    {
        Unknown = 0,
        Other = 1,
        TapeCartridge = 2,
        QICCartridge = 3,
        AITCartridge = 4,
        DTFCartridge = 5,
        DATCartridge = 6,
        EightMillimeterTapeCartridge = 7,
        NineteenMillimeterTapeCartridge = 8,
        DLTCartridge = 9,
        HalfInchMagneticTapeCartridge = 10,
        CartridgeDisk = 11,
        JAZDisk = 12,
        ZIPDisk = 13,
        SyQuestDisk = 14,
        WinchesterRemovableDisk = 15,
        CDROM = 16,
        CDROMXA = 17,
        CDI = 18,
        CDRecordable = 19,
        WORM = 20,
        MagnetoOptical = 21,
        DVD = 22,
        DVDPlusRW = 23,
        DVDRAM = 24,
        DVDROM = 25,
        DVDVideo = 26,
        Divx = 27,
        FloppyDiskette = 28,
        HardDisk = 29,
        MemoryCard = 30,
        HardCopy = 31,
        ClikDisk = 32,
        CDRW = 33,
        CDDA = 34,
        CDPlus = 35,
        DVDRecordable = 36,
        DVDMinusRW = 37,
        DVDAudio = 38,
        DVD5 = 39,
        DVD9 = 40,
        DVD10 = 41,
        DVD18 = 42,
        MagnetoOpticalRewriteable = 43,
        MagnetoOpticalWriteOnce = 44,
        MagnetoOpticalRewriteableLIMDOW = 45,
        PhaseChangeWriteOnce = 46,
        PhaseChangeRewriteable = 47,
        PhaseChangeDualRewriteable = 48,
        AblativeWriteOnce = 49,
        NearFieldRecording = 50,
        MiniQic = 51,
        Travan = 52,
        EightMillimeterMetalParticle = 53,
        EightMillimeterAdvancedMetalEvaporate = 54,
        NCTP = 55,
        LTOUltrium = 56,
        LTOAccelis = 57,
        NineTrackTape = 58,
        EighteenTrackTape = 59,
        ThirtySixTrackTape = 60,
        Magstar3590 = 61,
        MagstarMP = 62,
        D2Tape = 63,
        TapeDSTSmall = 64,
        TapeDSTMedium = 65,
        TapeDSTLarge = 66,
    }
}
