using System;
using System.Collections.Generic;
using System.Windows.Media;
using static MPF.Frontend.InterfaceConstants;

namespace MPF.UI
{
    /// <summary>
    /// Variables for UI elements
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Set of accepted speeds for CD and GD media
        /// </summary>
        public static DoubleCollection SpeedsForCDAsCollection => GetDoubleCollectionFromIntList(CD);

        /// <summary>
        /// Set of accepted speeds for DVD media
        /// </summary>
        public static DoubleCollection SpeedsForDVDAsCollection => GetDoubleCollectionFromIntList(DVD);

        /// <summary>
        /// Set of accepted speeds for HD-DVD media
        /// </summary>
        public static DoubleCollection SpeedsForHDDVDAsCollection => GetDoubleCollectionFromIntList(HDDVD);

        /// <summary>
        /// Set of accepted speeds for BD media
        /// </summary>
        public static DoubleCollection SpeedsForBDAsCollection => GetDoubleCollectionFromIntList(BD);

        /// <summary>
        /// Create a DoubleCollection out of a list of integer values
        /// </summary>
        private static DoubleCollection GetDoubleCollectionFromIntList(List<int> list)
            => [.. list.ConvertAll(i => Convert.ToDouble(i))];
    }
}
