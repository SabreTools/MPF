using SabreTools.RedumpLib.Data;

namespace MPF.ExecutionContexts.Redumper
{
    public static class Converters
    {
        #region Cross-enumeration conversions

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
                    or MediaType.GDROM => ".bin",
                MediaType.DVD
                    or MediaType.HDDVD
                    or MediaType.BluRay
                    or MediaType.NintendoWiiOpticalDisc => ".iso",
                MediaType.NintendoGameCubeGameDisc => ".raw",
                MediaType.NintendoWiiUOpticalDisc => ".wud",
                _ => null,
            };
        }

        #endregion
    }
}
