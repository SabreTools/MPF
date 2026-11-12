using System;
using System.Collections.Generic;
using SabreTools.RedumpLib.Data;

namespace MPF.Frontend.ComboBoxItems
{
    /// <summary>
    /// Represents a single item in the Physical Media Type combo box
    /// </summary>
    public class PhysicalMediaTypeComboBoxItem : IEquatable<PhysicalMediaTypeComboBoxItem>, IElement
    {
        private readonly PhysicalMediaType Data;

        public PhysicalMediaTypeComboBoxItem(PhysicalMediaType data) => Data = data;

        /// <summary>
        /// Allow elements to be used as their internal enum type
        /// </summary>
        public static implicit operator PhysicalMediaType?(PhysicalMediaTypeComboBoxItem item) => item.Data;

        /// <inheritdoc/>
        public string Name => Data.Name;

        public override string ToString() => Name;

        /// <summary>
        /// Internal enum value
        /// </summary>
        public PhysicalMediaType Value => Data;

        /// <summary>
        /// Determine if the item is selected or not
        /// </summary>
        /// <remarks>Only applies to CheckBox type</remarks>
        public bool IsChecked
        {
            get;
            set
            {
                field = value;
                Checked(this);
            }
        }

        /// <summary>
        /// Generate all elements associated with the data enum type
        /// </summary>
        /// <returns></returns>
        public static List<PhysicalMediaTypeComboBoxItem> GenerateElements()
            => [.. Array.ConvertAll(PhysicalMediaType.AllMediaTypes, e => new PhysicalMediaTypeComboBoxItem(e))];

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return Equals(obj as PhysicalMediaTypeComboBoxItem);
        }

        /// <inheritdoc/>
        public bool Equals(PhysicalMediaTypeComboBoxItem? other)
        {
            if (other is null)
                return false;

            return Name == other.Name;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => base.GetHashCode();

        /// <summary>
        /// Occurs when the checked state is changed
        /// </summary>
        public event SelectionChanged Checked = (_) => { return; };
    }
}
