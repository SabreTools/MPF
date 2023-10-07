using SabreTools.RedumpLib.Data;

namespace MPF.Core.Modules.Redumper
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
            switch (type)
            {
                case MediaType.CDROM:
                    return ".bin";
                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:
                    return ".iso";
                case MediaType.NONE:
                default:
                    return null;
            }
        }

        #endregion
    }
}