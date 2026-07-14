using System;
using System.Collections.Generic;
using SabreTools.RedumpLib.Data;

namespace MPF.Frontend.ComboBoxItems
{
    /// <summary>
    /// Represents a single item in the Language combo box
    /// </summary>
    public class LanguageCodeComboBoxItem : IEquatable<LanguageCodeComboBoxItem>, IElement
    {
        private readonly LanguageCode Data;

        public LanguageCodeComboBoxItem(LanguageCode data) => Data = data;

        /// <summary>
        /// Allow elements to be used as their internal enum type
        /// </summary>
        public static implicit operator LanguageCode?(LanguageCodeComboBoxItem item) => item.Data;

        /// <inheritdoc/>
        public string Name => Data.Name;

        public override string ToString() => Name;

        /// <summary>
        /// Internal enum value
        /// </summary>
        public LanguageCode Value => Data;

        /// <summary>
        /// Determine if the item is selected or not
        /// </summary>
        /// <remarks>Only applies to CheckBox type</remarks>
        public bool IsChecked { get; set; }

        /// <summary>
        /// Generate all elements associated with the data enum type
        /// </summary>
        /// <returns></returns>
        public static List<LanguageCodeComboBoxItem> GenerateElements()
            => [.. Array.ConvertAll(LanguageCode.AllLanguages, e => new LanguageCodeComboBoxItem(e))];

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return Equals(obj as LanguageCodeComboBoxItem);
        }

        /// <inheritdoc/>
        public bool Equals(LanguageCodeComboBoxItem? other)
        {
            if (other is null)
                return false;

            return Name == other.Name;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => base.GetHashCode();
    }
}
