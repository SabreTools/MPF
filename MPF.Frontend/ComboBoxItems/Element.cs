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
        private readonly TEnum Data;

        public Element(TEnum data) => Data = data;

        /// <summary>
        /// Allow elements to be used as their internal enum type
        /// </summary>
        /// <param name="item"></param>
        public static implicit operator TEnum? (Element<TEnum> item) => item?.Data;

        /// <inheritdoc/>
        public string Name => EnumExtensions.GetLongName(Data);

        public override string ToString() => Name;

        /// <summary>
        /// Internal enum value
        /// </summary>
        public TEnum Value => Data;

        /// <summary>
        /// Determine if the item is selected or not
        /// </summary>
        /// <remarks>Only applies to CheckBox type</remarks>
        public bool IsChecked { get; set; }

        /// <summary>
        /// Generate all elements associated with the data enum type
        /// </summary>
        /// <returns></returns>
        public static List<Element<TEnum>> GenerateElements()
        {
            var enumArr = (TEnum[])Enum.GetValues(typeof(TEnum));
            return [.. Array.ConvertAll(enumArr, e => new Element<TEnum>(e))];
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return Equals(obj as Element<TEnum>);
        }

        /// <inheritdoc/>
        public bool Equals(Element<TEnum>? other)
        {
            if (other == null)
                return false;

            return Name == other.Name;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => base.GetHashCode();
    }
}
