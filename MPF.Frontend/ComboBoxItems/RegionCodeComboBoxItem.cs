using System;
using System.Collections.Generic;
using SabreTools.RedumpLib.Data;

namespace MPF.Frontend.ComboBoxItems
{
    /// <summary>
    /// Represents a single item in the Region combo box
    /// </summary>
    public class RegionCodeComboBoxItem : IEquatable<RegionCodeComboBoxItem>, IElement
    {
        private readonly RegionCode Data;

        public RegionCodeComboBoxItem(RegionCode data) => Data = data;

        /// <summary>
        /// Allow elements to be used as their internal enum type
        /// </summary>
        public static implicit operator RegionCode?(RegionCodeComboBoxItem item) => item.Data;

        /// <inheritdoc/>
        public string Name => Data.Name;

        public override string ToString() => Name;

        /// <summary>
        /// Internal enum value
        /// </summary>
        public RegionCode Value => Data;

        /// <summary>
        /// Determine if the item is selected or not
        /// </summary>
        /// <remarks>Only applies to CheckBox type</remarks>
        public bool IsChecked { get; set; }

        /// <summary>
        /// Generate all elements associated with the data enum type
        /// </summary>
        /// <returns></returns>
        public static List<RegionCodeComboBoxItem> GenerateElements()
        {
            var enumArr = (RegionCode[])Enum.GetValues(typeof(RegionCode));
            return [.. Array.ConvertAll(enumArr, e => new RegionCodeComboBoxItem(e))];
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return Equals(obj as RegionCodeComboBoxItem);
        }

        /// <inheritdoc/>
        public bool Equals(RegionCodeComboBoxItem? other)
        {
            if (other is null)
                return false;

            return Name == other.Name;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => base.GetHashCode();
    }
}
