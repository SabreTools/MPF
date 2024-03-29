﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using static MPF.Core.Data.Interface;

namespace MPF.UI.Core
{
    /// <summary>
    /// Variables for UI elements
    /// </summary>
    public static class Constants
    {
        // Create collections for UI based on known drive speeds
        public static DoubleCollection SpeedsForCDAsCollection { get; } = GetDoubleCollectionFromIntList(CD);
        public static DoubleCollection SpeedsForDVDAsCollection { get; } = GetDoubleCollectionFromIntList(DVD);
        public static DoubleCollection SpeedsForHDDVDAsCollection { get; } = GetDoubleCollectionFromIntList(HDDVD);
        public static DoubleCollection SpeedsForBDAsCollection { get; } = GetDoubleCollectionFromIntList(BD);

#if NET20 || NET35 || NET40
        private static DoubleCollection GetDoubleCollectionFromIntList(IList<int> list)
#else
        private static DoubleCollection GetDoubleCollectionFromIntList(IReadOnlyList<int> list)
#endif
            => new(list.Select(i => Convert.ToDouble(i)).ToList());
    }
}
