using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace DICUI
{
    /// <summary>
    /// Variables for UI elements
    /// </summary>
    public static class Constants
    {
        public const int LogWindowMarginFromMainWindow = 10;

        // Private lists of known drive speed ranges
        private static IReadOnlyList<int> cd { get; } = new List<int> { 1, 2, 3, 4, 6, 8, 12, 16, 20, 24, 32, 40, 44, 48, 52, 56, 72 };
        private static IReadOnlyList<int> dvd { get; } = cd.Where(s => s <= 24).ToList();
        private static IReadOnlyList<int> bd { get; } = cd.Where(s => s <= 16).ToList();

        // Create collections for UI based on known drive speeds
        public static DoubleCollection SpeedsForCDAsCollection { get; } = GetDoubleCollectionFromIntList(cd);
        public static DoubleCollection SpeedsForDVDAsCollection { get; } = GetDoubleCollectionFromIntList(dvd);
        public static DoubleCollection SpeedsForBDAsCollection { get; } = GetDoubleCollectionFromIntList(bd);
        private static DoubleCollection GetDoubleCollectionFromIntList(IReadOnlyList<int> list)
            => new DoubleCollection(list.Select(i => Convert.ToDouble(i)).ToList());
    }
}
