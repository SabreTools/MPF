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
    public class PhysicalSystemComboBoxItem : IEquatable<PhysicalSystemComboBoxItem>, IElement
    {
        private readonly object? Data;

        public PhysicalSystemComboBoxItem(PhysicalSystem? system) => Data = system;
        public PhysicalSystemComboBoxItem(SystemCategory? category) => Data = category;

        public static implicit operator PhysicalSystem?(PhysicalSystemComboBoxItem item) => item.Data as PhysicalSystem?;

        /// <inheritdoc/>
        public string Name
        {
            get
            {
                if (IsHeader)
                    return "---------- " + (Data as SystemCategory?).LongName() + " ----------";
                else
                    return (Data as PhysicalSystem?).LongName() ?? "No system selected";
            }
        }

        public override string ToString() => Name;

        /// <summary>
        /// Internal enum value
        /// </summary>
        public PhysicalSystem? Value => Data as PhysicalSystem?;

        /// <summary>
        /// Determines if the item is a header value
        /// </summary>
        public bool IsHeader => Data is SystemCategory?;

        /// <summary>
        /// Determines if the item is a standard system value
        /// </summary>
        public bool IsSystem => Data is PhysicalSystem?;

        /// <summary>
        /// Generate all elements for the known system combo box
        /// </summary>
        /// <returns></returns>
        public static List<PhysicalSystemComboBoxItem> GenerateElements()
        {
            var enumArr = (PhysicalSystem[])Enum.GetValues(typeof(PhysicalSystem));
            var nullableArr = Array.ConvertAll(enumArr, s => (PhysicalSystem?)s);
            var knownSystems = Array.FindAll(nullableArr,
                s => !s.IsMarker() && s.GetCategory() != SystemCategory.NONE);

#if NET20
            // The resulting dictionary does not have ordered value lists
            Dictionary<SystemCategory, List<PhysicalSystem?>> mapping = [];
            foreach (var knownSystem in knownSystems)
            {
                var category = knownSystem.GetCategory();
                if (!mapping.ContainsKey(category))
                    mapping[category] = [];

                mapping[category].Add(knownSystem);
            }
#else
            // The resulting dictionary has ordered value lists
            Dictionary<SystemCategory, List<PhysicalSystem?>> mapping = knownSystems
                .GroupBy(s => s.GetCategory())
                .ToDictionary(
                    k => k.Key,
                    v => v
                        .OrderBy(s => s.LongName())
                        .ToList()
                );
#endif

            var systemsValues = new List<PhysicalSystemComboBoxItem>
            {
                new((PhysicalSystem?)null),
            };

            foreach (var group in mapping)
            {
                systemsValues.Add(new PhysicalSystemComboBoxItem(group.Key));
                group.Value.ForEach(system => systemsValues.Add(new PhysicalSystemComboBoxItem(system)));
            }

            return systemsValues;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return Equals(obj as PhysicalSystemComboBoxItem);
        }

        /// <inheritdoc/>
        public bool Equals(PhysicalSystemComboBoxItem? other)
        {
            if (other is null)
                return false;

            return Value == other.Value;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => base.GetHashCode();
    }
}
