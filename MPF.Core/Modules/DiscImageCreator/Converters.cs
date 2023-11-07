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
            return baseCommand switch
            {
                CommandStrings.Audio => (RedumpSystem?)RedumpSystem.AudioCD,
                CommandStrings.CompactDisc
                    or CommandStrings.Data
                    or CommandStrings.DigitalVideoDisc
                    or CommandStrings.Disk
                    or CommandStrings.Floppy
                    or CommandStrings.Tape => (RedumpSystem?)RedumpSystem.IBMPCcompatible,
                CommandStrings.GDROM
                    or CommandStrings.Swap => (RedumpSystem?)RedumpSystem.SegaDreamcast,
                CommandStrings.BluRay => (RedumpSystem?)RedumpSystem.SonyPlayStation3,
                CommandStrings.SACD => (RedumpSystem?)RedumpSystem.SuperAudioCD,
                CommandStrings.XBOX
                    or CommandStrings.XBOXSwap => (RedumpSystem?)RedumpSystem.MicrosoftXbox,
                CommandStrings.XGD2Swap
                    or CommandStrings.XGD3Swap => (RedumpSystem?)RedumpSystem.MicrosoftXbox360,
                _ => null,
            };
        }

        /// <summary>
        /// Get the MediaType associated with a given base command
        /// </summary>
        /// <param name="baseCommand">Command value to check</param>
        /// <returns>MediaType if possible, null on error</returns>
        /// <remarks>This takes the "safe" route by assuming the larger of any given format</remarks>
        public static MediaType? ToMediaType(string? baseCommand)
        {
            return baseCommand switch
            {
                CommandStrings.Audio
                    or CommandStrings.CompactDisc
                    or CommandStrings.Data
                    or CommandStrings.SACD => (MediaType?)MediaType.CDROM,
                CommandStrings.GDROM
                    or CommandStrings.Swap => (MediaType?)MediaType.GDROM,
                CommandStrings.DigitalVideoDisc
                    or CommandStrings.XBOX
                    or CommandStrings.XBOXSwap
                    or CommandStrings.XGD2Swap
                    or CommandStrings.XGD3Swap => (MediaType?)MediaType.DVD,
                CommandStrings.BluRay => (MediaType?)MediaType.BluRay,

                // Non-optical
                CommandStrings.Floppy => (MediaType?)MediaType.FloppyDisk,
                CommandStrings.Disk => (MediaType?)MediaType.HardDisk,
                CommandStrings.Tape => (MediaType?)MediaType.DataCartridge,
                _ => null,
            };
        }

        /// <summary>
        /// Get the default extension for a given disc type
        /// </summary>
        /// <param name="type">MediaType value to check</param>
        /// <returns>Valid extension (with leading '.'), null on error</returns>
        public static string? Extension(MediaType? type)
        {
            return type switch
            {
                MediaType.CDROM
                    or MediaType.GDROM
                    or MediaType.Cartridge
                    or MediaType.HardDisk
                    or MediaType.CompactFlash
                    or MediaType.MMC
                    or MediaType.SDCard
                    or MediaType.FlashDrive => ".bin",
                MediaType.DVD
                    or MediaType.HDDVD
                    or MediaType.BluRay
                    or MediaType.NintendoWiiOpticalDisc => ".iso",
                MediaType.LaserDisc
                    or MediaType.NintendoGameCubeGameDisc => ".raw",
                MediaType.NintendoWiiUOpticalDisc => ".wud",
                MediaType.FloppyDisk => ".img",
                MediaType.Cassette => ".wav",
                _ => null,
            };
        }

        #endregion
    }
}