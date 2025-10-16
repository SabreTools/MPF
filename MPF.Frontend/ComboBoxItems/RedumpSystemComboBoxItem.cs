using System;
using System.Collections.Generic;
#if NET35_OR_GREATER || NETCOREAPP
using System.Linq;
#endif
using SabreTools.RedumpLib.Data;

namespace MPF.Frontend.ComboBoxItems
{
    /// <summary>
    /// Represents a single item in the System combo box
    /// </summary>
    public class RedumpSystemComboBoxItem : IEquatable<RedumpSystemComboBoxItem>, IElement
    {
        private readonly object? Data;

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
        public static List<RedumpSystemComboBoxItem> GenerateElements()
        {
            var enumArr = (RedumpSystem[])Enum.GetValues(typeof(RedumpSystem));
            var nullableArr = Array.ConvertAll(enumArr, s => (RedumpSystem?)s);
            var knownSystems = Array.FindAll(nullableArr,
                s => !s.IsMarker() && s.GetCategory() != SystemCategory.NONE);

#if NET20
            // The resulting dictionary does not have ordered value lists
            var mapping = new Dictionary<SystemCategory, List<RedumpSystem?>>();
            foreach (var knownSystem in knownSystems)
            {
                var category = knownSystem.GetCategory();
                if (!mapping.ContainsKey(category))
                    mapping[category] = [];

                mapping[category].Add(knownSystem);
            }
#else
            // The resulting dictionary has ordered value lists
            Dictionary<SystemCategory, List<RedumpSystem?>> mapping = knownSystems
                .GroupBy(s => s.GetCategory())
                .ToDictionary(
                    k => k.Key,
                    v => v
                        .OrderBy(s => s.LongName())
                        .ToList()
                );
#endif

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

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return Equals(obj as RedumpSystemComboBoxItem);
        }

        /// <inheritdoc/>
        public bool Equals(RedumpSystemComboBoxItem? other)
        {
            if (other == null)
                return false;

            return Value == other.Value;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => base.GetHashCode();
    }
}
