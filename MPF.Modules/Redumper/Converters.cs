using RedumpLib.Data;

namespace MPF.Modules.Redumper
{
    public static class Converters
    {
        #region Cross-enumeration conversions

        /// <summary>
        /// Get the default extension for a given disc type
        /// </summary>
        /// <param name="type">MediaType value to check</param>
        /// <returns>Valid extension (with leading '.'), null on error</returns>
        public static string Extension(MediaType? type)
        {
            // TODO: Determine what extensions are used for each supported type
            return ".bin";
        }

        #endregion
    }
}