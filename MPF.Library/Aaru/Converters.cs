using MPF.Data;

namespace MPF.Aaru
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
            // Aaru has a single, unified output format by default
            return ".aaruf";
        }

        #endregion
    }
}
