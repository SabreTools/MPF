using System;
using System.Collections.Generic;
using static MPF.Frontend.InterfaceConstants;

namespace MPF.UI.Avalonia
{
    /// <summary>
    /// Variables for UI elements
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Set of accepted speeds for CD and GD media
        /// </summary>
        /// <remarks>
        /// WPF used System.Windows.Media.DoubleCollection; replaced with List&lt;double&gt;
        /// which is bindable via Avalonia's ItemsSource and needs no WPF dependency.
        /// </remarks>
        public static List<double> SpeedsForCDAsCollection => GetDoubleCollectionFromIntList(CD);

        /// <summary>
        /// Set of accepted speeds for DVD media
        /// </summary>
        public static List<double> SpeedsForDVDAsCollection => GetDoubleCollectionFromIntList(DVD);

        /// <summary>
        /// Set of accepted speeds for HD-DVD media
        /// </summary>
        public static List<double> SpeedsForHDDVDAsCollection => GetDoubleCollectionFromIntList(HDDVD);

        /// <summary>
        /// Set of accepted speeds for BD media
        /// </summary>
        public static List<double> SpeedsForBDAsCollection => GetDoubleCollectionFromIntList(BD);

        /// <summary>
        /// Create a List&lt;double&gt; out of a list of integer values
        /// </summary>
        private static List<double> GetDoubleCollectionFromIntList(List<int> list)
            => list.ConvertAll(i => Convert.ToDouble(i));
    }
}
