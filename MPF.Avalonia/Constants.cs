using System;
using System.Collections.Generic;
using Avalonia.Collections;
using static MPF.Frontend.InterfaceConstants;

namespace MPF.Avalonia
{
    /// <summary>
    /// Variables for UI elements
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Set of accepted speeds for CD and GD media
        /// </summary>
        public static AvaloniaList<double> SpeedsForCDAsCollection => GetAvaloniaListFromIntList(CD);

        /// <summary>
        /// Set of accepted speeds for DVD media
        /// </summary>
        public static AvaloniaList<double> SpeedsForDVDAsCollection => GetAvaloniaListFromIntList(DVD);

        /// <summary>
        /// Set of accepted speeds for HD-DVD media
        /// </summary>
        public static AvaloniaList<double> SpeedsForHDDVDAsCollection => GetAvaloniaListFromIntList(HDDVD);

        /// <summary>
        /// Set of accepted speeds for BD media
        /// </summary>
        public static AvaloniaList<double> SpeedsForBDAsCollection => GetAvaloniaListFromIntList(BD);

        /// <summary>
        /// Create a AvaloniaList out of a list of integer values
        /// </summary>
        private static AvaloniaList<double> GetAvaloniaListFromIntList(List<int> list)
            => [.. list.ConvertAll(i => Convert.ToDouble(i))];
    }
}
