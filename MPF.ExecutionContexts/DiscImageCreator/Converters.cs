using SabreTools.RedumpLib.Data;

namespace MPF.ExecutionContexts.DiscImageCreator
{
    public static class Converters
    {
        #region Cross-enumeration conversions

        /// <summary>
        /// Get the most common known system for a given PhysicalMediaType
        /// </summary>
        /// <param name="baseCommand">Command value to check</param>
        /// <returns>PhysicalSystem if possible, null on error</returns>
        public static PhysicalSystem? ToPhysicalSystem(string baseCommand)
        {
            return baseCommand switch
            {
                CommandStrings.Audio => (PhysicalSystem?)PhysicalSystem.AudioCD,
                CommandStrings.CompactDisc
                    or CommandStrings.Data
                    or CommandStrings.DigitalVideoDisc
                    or CommandStrings.Disk
                    or CommandStrings.Floppy
                    or CommandStrings.Tape => (PhysicalSystem?)PhysicalSystem.IBMPCcompatible,
                CommandStrings.GDROM
                    or CommandStrings.Swap => (PhysicalSystem?)PhysicalSystem.SegaDreamcast,
                CommandStrings.BluRay => (PhysicalSystem?)PhysicalSystem.SonyPlayStation3,
                CommandStrings.SACD => (PhysicalSystem?)PhysicalSystem.SuperAudioCD,
                CommandStrings.XBOX
                    or CommandStrings.XBOXSwap => (PhysicalSystem?)PhysicalSystem.MicrosoftXbox,
                CommandStrings.XGD2Swap
                    or CommandStrings.XGD3Swap => (PhysicalSystem?)PhysicalSystem.MicrosoftXbox360,
                _ => null,
            };
        }

        /// <summary>
        /// Get the PhysicalMediaType associated with a given base command
        /// </summary>
        /// <param name="baseCommand">Command value to check</param>
        /// <returns>PhysicalMediaType if possible, null on error</returns>
        /// <remarks>This takes the "safe" route by assuming the larger of any given format</remarks>
        public static PhysicalMediaType? ToPhysicalMediaType(string? baseCommand)
        {
            return baseCommand switch
            {
                CommandStrings.Audio
                    or CommandStrings.CompactDisc
                    or CommandStrings.Data
                    or CommandStrings.SACD => PhysicalMediaType.CDROM,
                CommandStrings.GDROM
                    or CommandStrings.Swap => PhysicalMediaType.GDROM,
                CommandStrings.DigitalVideoDisc => PhysicalMediaType.DVD,
                CommandStrings.XBOX
                    or CommandStrings.XBOXSwap
                    or CommandStrings.XGD2Swap
                    or CommandStrings.XGD3Swap => PhysicalMediaType.XGD,
                CommandStrings.BluRay => PhysicalMediaType.BluRay,

                // Non-optical
                CommandStrings.Floppy => PhysicalMediaType.FloppyDisk,
                CommandStrings.Disk => PhysicalMediaType.HardDisk,
                CommandStrings.Tape => PhysicalMediaType.DataCartridge,
                _ => null,
            };
        }

        /// <summary>
        /// Get the default extension for a given disc type
        /// </summary>
        /// <param name="type">PhysicalMediaType value to check</param>
        /// <returns>Valid extension (with leading '.'), null on error</returns>
        public static string? Extension(PhysicalMediaType? type)
        {
            if (type == PhysicalMediaType.CDROM
                || type == PhysicalMediaType.GDROM
                || type == PhysicalMediaType.Cartridge
                || type == PhysicalMediaType.HardDisk
                || type == PhysicalMediaType.CompactFlash
                || type == PhysicalMediaType.MMC
                || type == PhysicalMediaType.SDCard
                || type == PhysicalMediaType.FlashDrive)
            {
                return ".bin";
            }
            else if (type == PhysicalMediaType.DVD
                || type == PhysicalMediaType.HDDVD
                || type == PhysicalMediaType.BluRay
                || type == PhysicalMediaType.NintendoWiiOpticalDisc)
            {
                return ".iso";
            }
            else if (type == PhysicalMediaType.LaserDisc
                || type == PhysicalMediaType.NintendoGameCubeGameDisc)
            {
                return ".raw";
            }
            else if (type == PhysicalMediaType.NintendoWiiUOpticalDisc)
            {
                return ".wud";
            }
            else if (type == PhysicalMediaType.FloppyDisk)
            {
                return ".img";
            }
            else if (type == PhysicalMediaType.Cassette)
            {
                return ".wav";
            }

            return null;
        }

        #endregion
    }
}
