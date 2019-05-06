using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using DICUI.Data;

namespace DICUI
{
    /// <summary>
    /// Variables for UI elements
    /// </summary>
    public static class Constants
    {
        public const string StartDumping = "Start Dumping";
        public const string StopDumping = "Stop Dumping";

        public const int LogWindowMarginFromMainWindow = 10;

        // Private lists of known drive speed ranges
        private static IReadOnlyList<int> cd { get; } = new List<int> { 1, 2, 3, 4, 6, 8, 12, 16, 20, 24, 32, 40, 44, 48, 52, 56, 72 };
        private static IReadOnlyList<int> dvd { get; } = cd.Where(s => s <= 24).ToList();
        private static IReadOnlyList<int> bd { get; } = cd.Where(s => s <= 16).ToList();
        private static IReadOnlyList<int> unknown { get; } = cd; // TODO: All or {1}? Maybe null?

        /// <summary>
        /// Get list of all drive speeds for a given MediaType
        /// </summary>
        /// <param name="type">MediaType? that represents the current item</param>
        /// <returns>Read-only list of drive speeds</returns>
        public static IReadOnlyList<int> GetSpeedsForMediaType(MediaType? type)
        {
            switch (type)
            {
                case MediaType.CDROM:
                case MediaType.GDROM:
                    return cd;
                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.NintendoGameCubeGameDisc:
                case MediaType.NintendoWiiOpticalDisc:
                    return dvd;
                case MediaType.BluRay:
                    return bd;
                default:
                    return unknown;
            }
        }

        // Create collections for UI based on known drive speeds
        public static DoubleCollection SpeedsForCDAsCollection { get; } = GetDoubleCollectionFromIntList(cd);
        public static DoubleCollection SpeedsForDVDAsCollection { get; } = GetDoubleCollectionFromIntList(dvd);
        public static DoubleCollection SpeedsForBDAsCollection { get; } = GetDoubleCollectionFromIntList(bd);
        private static DoubleCollection GetDoubleCollectionFromIntList(IReadOnlyList<int> list)
            => new DoubleCollection(list.Select(i => Convert.ToDouble(i)).ToList());
    }
}
