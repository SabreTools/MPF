using SabreTools.RedumpLib.Data;

namespace MPF.Core.Modules.DiscImageCreator
{
    public static class Converters
    {
        #region Cross-enumeration conversions

        /// <summary>
        /// Get the most common known system for a given MediaType
        /// </summary>
        /// <param name="baseCommand">Command value to check</param>
        /// <returns>RedumpSystem if possible, null on error</returns>
        public static RedumpSystem? ToRedumpSystem(string baseCommand)
        {
            switch (baseCommand)
            {
                case CommandStrings.Audio:
                    return RedumpSystem.AudioCD;
                case CommandStrings.CompactDisc:
                case CommandStrings.Data:
                case CommandStrings.DigitalVideoDisc:
                case CommandStrings.Disk:
                case CommandStrings.Floppy:
                case CommandStrings.Tape:
                    return RedumpSystem.IBMPCcompatible;
                case CommandStrings.GDROM:
                case CommandStrings.Swap:
                    return RedumpSystem.SegaDreamcast;
                case CommandStrings.BluRay:
                    return RedumpSystem.SonyPlayStation3;
                case CommandStrings.SACD:
                    return RedumpSystem.SuperAudioCD;
                case CommandStrings.XBOX:
                case CommandStrings.XBOXSwap:
                    return RedumpSystem.MicrosoftXbox;
                case CommandStrings.XGD2Swap:
                case CommandStrings.XGD3Swap:
                    return RedumpSystem.MicrosoftXbox360;
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
        public static MediaType? ToMediaType(string baseCommand)
        {
            switch (baseCommand)
            {
                case CommandStrings.Audio:
                case CommandStrings.CompactDisc:
                case CommandStrings.Data:
                case CommandStrings.SACD:
                    return MediaType.CDROM;
                case CommandStrings.GDROM:
                case CommandStrings.Swap:
                    return MediaType.GDROM;
                case CommandStrings.DigitalVideoDisc:
                case CommandStrings.XBOX:
                case CommandStrings.XBOXSwap:
                case CommandStrings.XGD2Swap:
                case CommandStrings.XGD3Swap:
                    return MediaType.DVD;
                case CommandStrings.BluRay:
                    return MediaType.BluRay;

                // Non-optical
                case CommandStrings.Floppy:
                    return MediaType.FloppyDisk;
                case CommandStrings.Disk:
                    return MediaType.HardDisk;
                case CommandStrings.Tape:
                    return MediaType.DataCartridge;
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
    }
}