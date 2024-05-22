using System.Collections.Generic;
using System.Linq;
using SabreTools.RedumpLib.Data;

namespace MPF.Core.Data
{
    /// <summary>
    /// Constant values for UI
    /// </summary>
    public static class Interface
    {
        // Lists of known drive speed ranges
#if NET20 || NET35 || NET40
        public static IList<int> CD { get; } = new List<int> { 1, 2, 3, 4, 6, 8, 12, 16, 20, 24, 32, 40, 44, 48, 52, 56, 72 };
        public static IList<int> DVD { get; } = CD.Where(s => s <= 24).ToList();
        public static IList<int> HDDVD { get; } = CD.Where(s => s <= 24).ToList();
        public static IList<int> BD { get; } = CD.Where(s => s <= 16).ToList();
        public static IList<int> Unknown { get; } = new List<int> { 1 };
#else
        public static IReadOnlyList<int> CD { get; } = new List<int> { 1, 2, 3, 4, 6, 8, 12, 16, 20, 24, 32, 40, 44, 48, 52, 56, 72 };
        public static IReadOnlyList<int> DVD { get; } = CD.Where(s => s <= 24).ToList();
        public static IReadOnlyList<int> HDDVD { get; } = CD.Where(s => s <= 24).ToList();
        public static IReadOnlyList<int> BD { get; } = CD.Where(s => s <= 16).ToList();
        public static IReadOnlyList<int> Unknown { get; } = new List<int> { 1 };
#endif

        /// <summary>
        /// Get list of all drive speeds for a given MediaType
        /// </summary>
        /// <param name="type">MediaType? that represents the current item</param>
        /// <returns>Read-only list of drive speeds</returns>
#if NET20 || NET35 || NET40
        public static IList<int> GetSpeedsForMediaType(MediaType? type)
#else
        public static IReadOnlyList<int> GetSpeedsForMediaType(MediaType? type)
#endif
        {
            return type switch
            {
                MediaType.CDROM
                    or MediaType.GDROM => CD,
                MediaType.DVD
                    or MediaType.NintendoGameCubeGameDisc
                    or MediaType.NintendoWiiOpticalDisc => DVD,
                MediaType.HDDVD => HDDVD,
                MediaType.BluRay => BD,
                _ => Unknown,
            };
        }
    }
}
