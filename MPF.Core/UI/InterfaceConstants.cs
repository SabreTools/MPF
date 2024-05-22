using System.Collections.Generic;
using System.Linq;
using SabreTools.RedumpLib.Data;

namespace MPF.Core.UI
{
    /// <summary>
    /// Constant values for UI
    /// </summary>
    public static class InterfaceConstants
    {
        /// <summary>
        /// Set of all accepted speed values
        /// </summary>
        private static readonly List<int> _speedValues = [1, 2, 3, 4, 6, 8, 12, 16, 20, 24, 32, 40, 44, 48, 52, 56, 72];

        /// <summary>
        /// Set of accepted speeds for CD and GD media
        /// </summary>
        public static IList<int> CD => _speedValues.Where(s => s <= 72).ToList();

        /// <summary>
        /// Set of accepted speeds for DVD media
        /// </summary>
        public static IList<int> DVD => _speedValues.Where(s => s <= 24).ToList();

        /// <summary>
        /// Set of accepted speeds for HD-DVD media
        /// </summary>
        public static IList<int> HDDVD => _speedValues.Where(s => s <= 24).ToList();

        /// <summary>
        /// Set of accepted speeds for BD media
        /// </summary>
        public static IList<int> BD => _speedValues.Where(s => s <= 16).ToList();

        /// <summary>
        /// Set of accepted speeds for all other media
        /// </summary>
        public static IList<int> Unknown => _speedValues.Where(s => s <= 1).ToList();

        /// <summary>
        /// Get list of all drive speeds for a given MediaType
        /// </summary>
        /// <param name="type">MediaType? that represents the current item</param>
        /// <returns>Read-only list of drive speeds</returns>
        public static IList<int> GetSpeedsForMediaType(MediaType? type)
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
