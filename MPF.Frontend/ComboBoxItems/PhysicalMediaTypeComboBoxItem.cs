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
        #region IElement Implementation

        /// <inheritdoc/>
        public string Name => Data.Name;

        /// <inheritdoc/>
        public bool IsChecked
        {
            get;
            set
            {
                field = value;
                Checked(this);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Internal enum value
        /// </summary>
        public PhysicalMediaType Value => Data;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the checked state is changed
        /// </summary>
        public event SelectionChanged Checked = (_) => { return; };

        #endregion

        #region Private Fields

        /// <summary>
        /// Backing data for the item
        /// </summary>
        private readonly PhysicalMediaType Data;

        #endregion

        public PhysicalMediaTypeComboBoxItem(PhysicalMediaType data) => Data = data;

        /// <inheritdoc/>
        public override string ToString() => Name;

        #region Operators

        /// <summary>
        /// Allow elements to be used as their internal type
        /// </summary>
        public static implicit operator PhysicalMediaType?(PhysicalMediaTypeComboBoxItem item) => item.Data;

        #endregion

        #region Generation

        /// <summary>
        /// Generate all elements associated with the data type
        /// </summary>
        /// <returns></returns>
        public static List<PhysicalMediaTypeComboBoxItem> GenerateElements()
            => [.. Array.ConvertAll(PhysicalMediaType.AllMediaTypes, e => new PhysicalMediaTypeComboBoxItem(e))];

        #endregion

        #region IEquatable Implementation

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

        #endregion
    }
}
