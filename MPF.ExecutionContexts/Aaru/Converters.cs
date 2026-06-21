using SabreTools.RedumpLib.Data;

namespace MPF.ExecutionContexts.Aaru
{
    public static class Converters
    {
        #region Cross-enumeration conversions

        /// <summary>
        /// Get the default extension for a given disc type
        /// </summary>
        /// <param name="type">PhysicalMediaType value to check</param>
        /// <returns>Valid extension (with leading '.'), null on error</returns>
        public static string Extension(PhysicalMediaType? type)
        {
            // Aaru has a single, unified output format by default
            return ".aaruf";
        }

        #endregion
    }
}
