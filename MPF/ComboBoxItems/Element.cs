using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MPF
{
    /// <summary>
    /// A generic combo box element
    /// </summary>
    /// <typeparam name="T">Enum type representing the possible values</typeparam>
    public class Element<T> : IElement where T : struct, Enum
    {
        private readonly T Data;

        public Element(T data)
        {
            Data = data;
        }

        /// <summary>
        /// Allow elements to be used as their internal enum type
        /// </summary>
        /// <param name="item"></param>
        public static implicit operator T? (Element<T> item) => item.Data;

        /// <summary>
        /// Display name for the combo box element
        /// </summary>
        public string Name => new EnumDescriptionConverter().Convert(Data, null, null, CultureInfo.CurrentCulture) as string;

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
        /// Generate all elements assocaited with the data enum type
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Element<T>> GenerateElements()
        {
            return Enum.GetValues(typeof(T))
                .OfType<T>()
                .Select(e => new Element<T>(e));
        }
    }
}
