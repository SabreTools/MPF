using System;
using System.IO;
using DICUI.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DICUI.DiscImageCreator
{
    public static class Converters
    {
        #region Cross-enumeration conversions

        /// <summary>
        /// Get the most common known system for a given MediaType
        /// </summary>
        /// <param name="baseCommand">Command value to check</param>
        /// <returns>KnownSystem if possible, null on error</returns>
        public static KnownSystem? ToKnownSystem(Command baseCommand)
        {
            switch (baseCommand)
            {
                case Command.Audio:
                    return KnownSystem.AudioCD;
                case Command.CompactDisc:
                case Command.Data:
                case Command.DigitalVideoDisc:
                case Command.Disk:
                case Command.Floppy:
                    return KnownSystem.IBMPCCompatible;
                case Command.GDROM:
                case Command.Swap:
                    return KnownSystem.SegaDreamcast;
                case Command.BluRay:
                    return KnownSystem.SonyPlayStation3;
                case Command.SACD:
                    return KnownSystem.SuperAudioCD;
                case Command.XBOX:
                case Command.XBOXSwap:
                    return KnownSystem.MicrosoftXBOX;
                case Command.XGD2Swap:
                case Command.XGD3Swap:
                    return KnownSystem.MicrosoftXBOX360;
                default:
                    return null;
            }
        }
    
        /// <summary>
        /// Get the MediaType associated with a given base command
        /// </summary>
        /// <param name="baseCommand">Command value to check</param>
        /// <returns>MediaType if possible, null on error</returns>
        /// <remarks>This takes the "safe" route by assuming the larger of any given format</remarks>
        public static MediaType? ToMediaType(Command baseCommand)
        {
            switch (baseCommand)
            {
                case Command.Audio:
                case Command.CompactDisc:
                case Command.Data:
                case Command.SACD:
                    return MediaType.CDROM;
                case Command.GDROM:
                case Command.Swap:
                    return MediaType.GDROM;
                case Command.DigitalVideoDisc:
                case Command.XBOX:
                case Command.XBOXSwap:
                case Command.XGD2Swap:
                case Command.XGD3Swap:
                    return MediaType.DVD;
                case Command.BluRay:
                    return MediaType.BluRay;

                // Non-optical
                case Command.Floppy:
                    return MediaType.FloppyDisk;
                case Command.Disk:
                    return MediaType.HardDisk;
                default:
                    return null;
            }
        }
    
        /// <summary>
        /// Get the default extension for a given disc type
        /// </summary>
        /// <param name="type">MediaType value to check</param>
        /// <returns>Valid extension (with leading '.'), null on error</returns>
        public static string Extension(MediaType? type)
        {
            switch (type)
            {
                case MediaType.CDROM:
                case MediaType.GDROM:
                case MediaType.Cartridge:
                case MediaType.HardDisk:
                case MediaType.CompactFlash:
                case MediaType.MMC:
                case MediaType.SDCard:
                case MediaType.FlashDrive:
                    return ".bin";
                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:
                case MediaType.NintendoWiiOpticalDisc:
                case MediaType.UMD:
                    return ".iso";
                case MediaType.LaserDisc:
                case MediaType.NintendoGameCubeGameDisc:
                    return ".raw";
                case MediaType.NintendoWiiUOpticalDisc:
                    return ".wud";
                case MediaType.FloppyDisk:
                    return ".img";
                case MediaType.Cassette:
                    return ".wav";
                case MediaType.NONE:
                default:
                    return null;
            }
        }
    
        #endregion

        #region Convert to Long Name

        /// <summary>
        /// Get the string representation of the Command enum values
        /// </summary>
        /// <param name="command">Command value to convert</param>
        /// <returns>String representing the value, if possible</returns>
        public static string LongName(Command command)
        {
            switch (command)
            {
                case Command.Audio:
                    return CommandStrings.Audio;
                case Command.BluRay:
                    return CommandStrings.BluRay;
                case Command.Close:
                    return CommandStrings.Close;
                case Command.CompactDisc:
                    return CommandStrings.CompactDisc;
                case Command.Data:
                    return CommandStrings.Data;
                case Command.DigitalVideoDisc:
                    return CommandStrings.DigitalVideoDisc;
                case Command.Disk:
                    return CommandStrings.Disk;
                case Command.DriveSpeed:
                    return CommandStrings.DriveSpeed;
                case Command.Eject:
                    return CommandStrings.Eject;
                case Command.Floppy:
                    return CommandStrings.Floppy;
                case Command.GDROM:
                    return CommandStrings.GDROM;
                case Command.MDS:
                    return CommandStrings.MDS;
                case Command.Merge:
                    return CommandStrings.Merge;
                case Command.Reset:
                    return CommandStrings.Reset;
                case Command.SACD:
                    return CommandStrings.SACD;
                case Command.Start:
                    return CommandStrings.Start;
                case Command.Stop:
                    return CommandStrings.Stop;
                case Command.Sub:
                    return CommandStrings.Sub;
                case Command.Swap:
                    return CommandStrings.Swap;
                case Command.XBOX:
                    return CommandStrings.XBOX;
                case Command.XBOXSwap:
                    return CommandStrings.XBOXSwap;
                case Command.XGD2Swap:
                    return CommandStrings.XGD2Swap;
                case Command.XGD3Swap:
                    return CommandStrings.XGD3Swap;

                case Command.NONE:
                default:
                    return "";
            }
        }

        /// <summary>
        /// Get the string representation of the Flag enum values
        /// </summary>
        /// <param name="command">Flag value to convert</param>
        /// <returns>String representing the value, if possible</returns>
        public static string LongName(Flag flag)
        {
            switch (flag)
            {
                case Flag.AddOffset:
                    return FlagStrings.AddOffset;
                case Flag.AMSF:
                    return FlagStrings.AMSF;
                case Flag.AtariJaguar:
                    return FlagStrings.AtariJaguar;
                case Flag.BEOpcode:
                    return FlagStrings.BEOpcode;
                case Flag.C2Opcode:
                    return FlagStrings.C2Opcode;
                case Flag.CopyrightManagementInformation:
                    return FlagStrings.CopyrightManagementInformation;
                case Flag.D8Opcode:
                    return FlagStrings.D8Opcode;
                case Flag.DisableBeep:
                    return FlagStrings.DisableBeep;
                case Flag.ForceUnitAccess:
                    return FlagStrings.ForceUnitAccess;
                case Flag.MultiSession:
                    return FlagStrings.MultiSession;
                case Flag.NoFixSubP:
                    return FlagStrings.NoFixSubP;
                case Flag.NoFixSubQ:
                    return FlagStrings.NoFixSubQ;
                case Flag.NoFixSubQLibCrypt:
                    return FlagStrings.NoFixSubQLibCrypt;
                case Flag.NoFixSubRtoW:
                    return FlagStrings.NoFixSubRtoW;
                case Flag.NoFixSubQSecuROM:
                    return FlagStrings.NoFixSubQSecuROM;
                case Flag.NoSkipSS:
                    return FlagStrings.NoSkipSS;
                case Flag.Raw:
                    return FlagStrings.Raw;
                case Flag.Reverse:
                    return FlagStrings.Reverse;
                case Flag.ScanAntiMod:
                    return FlagStrings.ScanAntiMod;
                case Flag.ScanFileProtect:
                    return FlagStrings.ScanFileProtect;
                case Flag.ScanSectorProtect:
                    return FlagStrings.ScanSectorProtect;
                case Flag.SeventyFour:
                    return FlagStrings.SeventyFour;
                case Flag.SkipSector:
                    return FlagStrings.SkipSector;
                case Flag.SubchannelReadLevel:
                    return FlagStrings.SubchannelReadLevel;
                case Flag.VideoNow:
                    return FlagStrings.VideoNow;
                case Flag.VideoNowColor:
                    return FlagStrings.VideoNowColor;

                case Flag.NONE:
                default:
                    return "";
            }
        }

        #endregion

        #region Convert From String

        /// <summary>
        /// Get the Command enum value for a given string
        /// </summary>
        /// <param name="command">String value to convert</param>
        /// <returns>Command represented by the string(s), if possible</returns>
        public static Command StringToCommand(string command)
        {
            switch (command)
            {
                case CommandStrings.Audio:
                    return Command.Audio;
                case CommandStrings.BluRay:
                    return Command.BluRay;
                case CommandStrings.Close:
                    return Command.Close;
                case CommandStrings.CompactDisc:
                    return Command.CompactDisc;
                case CommandStrings.Data:
                    return Command.Data;
                case CommandStrings.DigitalVideoDisc:
                    return Command.DigitalVideoDisc;
                case CommandStrings.Disk:
                    return Command.Disk;
                case CommandStrings.DriveSpeed:
                    return Command.DriveSpeed;
                case CommandStrings.Eject:
                    return Command.Eject;
                case CommandStrings.Floppy:
                    return Command.Floppy;
                case CommandStrings.GDROM:
                    return Command.GDROM;
                case CommandStrings.MDS:
                    return Command.MDS;
                case CommandStrings.Merge:
                    return Command.Merge;
                case CommandStrings.Reset:
                    return Command.Reset;
                case CommandStrings.SACD:
                    return Command.SACD;
                case CommandStrings.Start:
                    return Command.Start;
                case CommandStrings.Stop:
                    return Command.Stop;
                case CommandStrings.Sub:
                    return Command.Sub;
                case CommandStrings.Swap:
                    return Command.Swap;
                case CommandStrings.XBOX:
                    return Command.XBOX;
                case CommandStrings.XBOXSwap:
                    return Command.XBOXSwap;
                case CommandStrings.XGD2Swap:
                    return Command.XGD2Swap;
                case CommandStrings.XGD3Swap:
                    return Command.XGD3Swap;
                    
                default:
                    return Command.NONE;
            }
        }

        #endregion
    }
}