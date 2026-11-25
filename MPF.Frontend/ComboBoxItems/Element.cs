using System;
using System.Collections.Generic;

namespace MPF.Frontend.ComboBoxItems
{
    /// <summary>
    /// A generic combo box element
    /// </summary>
    /// <typeparam name="TEnum">Enum type representing the possible values</typeparam>
    public class Element<TEnum> : IEquatable<Element<TEnum>>, IElement where TEnum : struct, Enum
    {
        #region IElement Implementation

        /// <inheritdoc/>
        public string Name => EnumExtensions.GetLongName(Data);

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
        public TEnum Value => Data;

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
        private readonly TEnum Data;

        #endregion

        public Element(TEnum data) => Data = data;

        /// <inheritdoc/>
        public override string ToString() => Name;

        #region Operators

        /// <summary>
        /// Allow elements to be used as their internal type
        /// </summary>
        /// <param name="item"></param>
        public static implicit operator TEnum?(Element<TEnum> item) => item?.Data;

        #endregion

        #region Generation

        /// <summary>
        /// Generate all elements associated with the data type
        /// </summary>
        /// <returns></returns>
        public static List<Element<TEnum>> GenerateElements()
        {
            var enumArr = (TEnum[])Enum.GetValues(typeof(TEnum));
            return [.. Array.ConvertAll(enumArr, e => new Element<TEnum>(e))];
        }

        #endregion

        #region IEquatable Implementation

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return Equals(obj as Element<TEnum>);
        }

        /// <inheritdoc/>
        public bool Equals(Element<TEnum>? other)
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
