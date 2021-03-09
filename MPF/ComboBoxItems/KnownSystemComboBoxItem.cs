using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using MPF.Data;
using MPF.Utilities;

namespace MPF
{
    /// <summary>
    /// Represents a single item in the System combo box
    /// </summary>
    public class KnownSystemComboBoxItem : IElement
    {
        private readonly object Data;

        public KnownSystemComboBoxItem(KnownSystem? system) => Data = system;
        public KnownSystemComboBoxItem(KnownSystemCategory? category) => Data = category;

        public static implicit operator KnownSystem?(KnownSystemComboBoxItem item) => item.Data as KnownSystem?;

        /// <inheritdoc/>
        public string Name
        {
            get
            {
                if (IsHeader)
                    return "---------- " + Converters.GetLongName(Data as KnownSystemCategory?) + " ----------";
                else
                    return Converters.GetLongName(Data as KnownSystem?);
            }
        }

        /// <summary>
        /// Internal enum value
        /// </summary>
        public KnownSystem? Value => Data as KnownSystem?;

        /// <summary>
        /// Determines the foreground color when displaying
        /// </summary>
        public Brush Foreground { get => IsHeader ? Brushes.Gray : Brushes.Black; }

        /// <summary>
        /// Determines if the item is a header value
        /// </summary>
        public bool IsHeader => Data is KnownSystemCategory?;

        /// <summary>
        /// Determines if the item is a standard system value
        /// </summary>
        public bool IsSystem => Data is KnownSystem?;

        /// <summary>
        /// Generate all elements for the known system combo box
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<KnownSystemComboBoxItem> GenerateElements()
        {
            var knownSystems = Enum.GetValues(typeof(KnownSystem))
                .OfType<KnownSystem?>()
                .Where(s => !s.IsMarker() && s != KnownSystem.NONE)
                .ToList();

            Dictionary<KnownSystemCategory, List<KnownSystem?>> mapping = knownSystems
                .GroupBy(s => s.Category())
                .ToDictionary(
                    k => k.Key,
                    v => v
                        .OrderBy(s => s.LongName())
                        .ToList()
                );

            var systemsValues = new List<KnownSystemComboBoxItem>
            {
                new KnownSystemComboBoxItem(KnownSystem.NONE),
            };

            foreach (var group in mapping)
            {
                systemsValues.Add(new KnownSystemComboBoxItem(group.Key));
                group.Value.ForEach(system => systemsValues.Add(new KnownSystemComboBoxItem(system)));
            }

            return systemsValues;
        }
    }
}
