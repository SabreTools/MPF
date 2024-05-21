using SabreTools.RedumpLib.Data;

namespace MPF.Core.ExecutionContexts.Redumper
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
                MediaType.CDROM => ".bin",
                MediaType.DVD
                    or MediaType.HDDVD
                    or MediaType.BluRay => ".iso",
                _ => null,
            };
        }

        #endregion
    }
}