using System;
using System.Collections.Generic;
using System.Linq;
using MPF.Converters;
using MPF.Data;
using MPF.Utilities;
using RedumpLib.Data;

namespace MPF
{
    /// <summary>
    /// Represents a single item in the System combo box
    /// </summary>
    public class RedumpSystemComboBoxItem : IElement
    {
        private readonly object Data;

        public RedumpSystemComboBoxItem(RedumpSystem? system) => Data = system;
        public RedumpSystemComboBoxItem(SystemCategory? category) => Data = category;

        public static implicit operator RedumpSystem?(RedumpSystemComboBoxItem item) => item.Data as RedumpSystem?;

        /// <inheritdoc/>
        public string Name
        {
            get
            {
                if (IsHeader)
                    return "---------- " + (Data as SystemCategory?).LongName() + " ----------";
                else
                    return (Data as RedumpSystem?).LongName() ?? "No system selected";
            }
        }

        public override string ToString() => Name;

        /// <summary>
        /// Internal enum value
        /// </summary>
        public RedumpSystem? Value => Data as RedumpSystem?;

        /// <summary>
        /// Determines if the item is a header value
        /// </summary>
        public bool IsHeader => Data is SystemCategory?;

        /// <summary>
        /// Determines if the item is a standard system value
        /// </summary>
        public bool IsSystem => Data is RedumpSystem?;

        /// <summary>
        /// Generate all elements for the known system combo box
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<RedumpSystemComboBoxItem> GenerateElements()
        {
            var knownSystems = Enum.GetValues(typeof(RedumpSystem))
                .OfType<RedumpSystem?>()
                .Where(s => !s.IsMarker() && s.GetCategory() != SystemCategory.NONE)
                .ToList();

            Dictionary<SystemCategory, List<RedumpSystem?>> mapping = knownSystems
                .GroupBy(s => s.GetCategory())
                .ToDictionary(
                    k => k.Key,
                    v => v
                        .OrderBy(s => s.LongName())
                        .ToList()
                );

            var systemsValues = new List<RedumpSystemComboBoxItem>
            {
                new RedumpSystemComboBoxItem((RedumpSystem?)null),
            };

            foreach (var group in mapping)
            {
                systemsValues.Add(new RedumpSystemComboBoxItem(group.Key));
                group.Value.ForEach(system => systemsValues.Add(new RedumpSystemComboBoxItem(system)));
            }

            return systemsValues;
        }
    }
}
