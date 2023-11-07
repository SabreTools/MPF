using System;
using System.Collections.Generic;
using System.Linq;
using MPF.Core.Converters;

namespace MPF.Core.UI.ComboBoxItems
{
    /// <summary>
    /// A generic combo box element
    /// </summary>
    /// <typeparam name="T">Enum type representing the possible values</typeparam>
    public class Element<T> : IEquatable<Element<T>>, IElement where T : struct, Enum
    {
        private readonly T Data;

        public Element(T data) => Data = data;

        /// <summary>
        /// Allow elements to be used as their internal enum type
        /// </summary>
        /// <param name="item"></param>
        public static implicit operator T? (Element<T> item) => item?.Data;

        /// <inheritdoc/>
        public string Name => EnumConverter.GetLongName(Data);

        public override string ToString() => Name;

        /// <summary>
        /// Internal enum value
        /// </summary>
        public T Value => Data;

        /// <summary>
        /// Determine if the item is selected or not
        /// </summary>
        /// <remarks>Only applies to CheckBox type</remarks>
        public bool IsChecked { get; set; }

        /// <summary>
        /// Generate all elements associated with the data enum type
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Element<T>> GenerateElements()
        {
            return Enum.GetValues(typeof(T))
                .OfType<T>()
                .Select(e => new Element<T>(e));
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return Equals(obj as Element<T>);
        }

        /// <inheritdoc/>
        public bool Equals(Element<T>? other)
        {
            if (other == null)
                return false;

            return Name == other.Name;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => base.GetHashCode();
    }
}
