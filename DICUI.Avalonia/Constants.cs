using System;
using System.Collections.Generic;
using System.Linq;
using DICUI.Data;

namespace DICUI.Avalonia
{
    /// <summary>
    /// Variables for UI elements
    /// </summary>
    public static class Constants
    {
        public const int LogWindowMarginFromMainWindow = 40;

        // Private lists of known drive speed ranges
        private static IReadOnlyList<int> cd { get; } = new List<int> { 1, 2, 3, 4, 6, 8, 12, 16, 20, 24, 32, 40, 44, 48, 52, 56, 72 };
        private static IReadOnlyList<int> dvd { get; } = cd.Where(s => s <= 24).ToList();
        private static IReadOnlyList<int> bd { get; } = cd.Where(s => s <= 16).ToList();

        // Create collections for UI based on known drive speeds
        public static List<double> SpeedsForCDAsCollection { get; } = GetDoubleCollectionFromIntList(cd);
        public static List<double> SpeedsForDVDAsCollection { get; } = GetDoubleCollectionFromIntList(dvd);
        public static List<double> SpeedsForBDAsCollection { get; } = GetDoubleCollectionFromIntList(bd);
        private static List<double> GetDoubleCollectionFromIntList(IReadOnlyList<int> list)
            => new List<double>(list.Select(i => Convert.ToDouble(i)).ToList());
    }
}
