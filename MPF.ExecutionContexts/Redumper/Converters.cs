using SabreTools.RedumpLib.Data;

namespace MPF.ExecutionContexts.Redumper
{
    public static class Converters
    {
        #region Cross-enumeration conversions

        /// <summary>
        /// Get the default extension for a given disc type
        /// </summary>
        /// <param name="type">PhysicalMediaType value to check</param>
        /// <returns>Valid extension (with leading '.'), null on error</returns>
        public static string? Extension(PhysicalMediaType? type)
        {
            if (type == PhysicalMediaType.CDROM
                || type == PhysicalMediaType.GDROM)
            {
                return ".bin";
            }
            else if (type == PhysicalMediaType.DVD
                || type == PhysicalMediaType.HDDVD
                || type == PhysicalMediaType.BluRay
                || type == PhysicalMediaType.NintendoWiiOpticalDisc
                || type == PhysicalMediaType.XGD)
            {
                return ".iso";
            }
            else if (type == PhysicalMediaType.NintendoGameCubeGameDisc)
            {
                return ".raw";
            }
            else if (type == PhysicalMediaType.NintendoWiiUOpticalDisc)
            {
                return ".wud";
            }

            return null;
        }

        #endregion
    }
}
