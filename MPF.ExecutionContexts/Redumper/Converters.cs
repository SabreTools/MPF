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
#pragma warning disable IDE0072
            return type switch
            {
                PhysicalMediaType.CDROM
                    or PhysicalMediaType.GDROM => ".bin",
                PhysicalMediaType.DVD
                    or PhysicalMediaType.HDDVD
                    or PhysicalMediaType.BluRay
                    or PhysicalMediaType.NintendoWiiOpticalDisc => ".iso",
                PhysicalMediaType.NintendoGameCubeGameDisc => ".raw",
                PhysicalMediaType.NintendoWiiUOpticalDisc => ".wud",
                _ => null,
            };
#pragma warning restore IDE0072
        }

        #endregion
    }
}
