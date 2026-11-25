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
        public LanguageCode Value => Data;

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
        private readonly LanguageCode Data;

        /// <summary>
        /// List of Redump-supported Languages
        /// </summary>
        private static readonly List<LanguageCode> RedumpLanguages =
        [
            LanguageCode.Afrikaans,
            LanguageCode.Albanian,
            LanguageCode.Arabic,
            LanguageCode.Armenian,
            LanguageCode.Azerbaijani,
            LanguageCode.Basque,
            LanguageCode.Belarusian,
            LanguageCode.Bulgarian,
            LanguageCode.Catalan,
            LanguageCode.Chinese,
            LanguageCode.Croatian,
            LanguageCode.Czech,
            LanguageCode.Danish,
            LanguageCode.Dutch,
            LanguageCode.English,
            LanguageCode.Estonian,
            LanguageCode.Finnish,
            LanguageCode.French,
            LanguageCode.Gaelic,
            LanguageCode.Galician,
            LanguageCode.German,
            LanguageCode.Greek,
            LanguageCode.Hebrew,
            LanguageCode.Hindi,
            LanguageCode.Hungarian,
            LanguageCode.Icelandic,
            LanguageCode.Indonesian,
            LanguageCode.Irish,
            LanguageCode.Italian,
            LanguageCode.Japanese,
            LanguageCode.Kalaallisut,
            LanguageCode.Korean,
            LanguageCode.Latin,
            LanguageCode.Latvian,
            LanguageCode.Lithuanian,
            LanguageCode.Macedonian,
            LanguageCode.Malay,
            LanguageCode.Maori,
            LanguageCode.Norwegian,
            LanguageCode.Persian,
            LanguageCode.Polish,
            LanguageCode.Portuguese,
            LanguageCode.Panjabi,
            LanguageCode.Romanian,
            LanguageCode.Russian,
            LanguageCode.Serbian,
            LanguageCode.Slovak,
            LanguageCode.Slovenian,
            LanguageCode.Spanish,
            LanguageCode.Swedish,
            LanguageCode.Tamil,
            LanguageCode.Thai,
            LanguageCode.Turkish,
            LanguageCode.Ukrainian,
            LanguageCode.Vietnamese,
            LanguageCode.Welsh,
        ];

        #endregion

        public LanguageCodeComboBoxItem(LanguageCode data) => Data = data;

        /// <inheritdoc/>
        public override string ToString() => Name;

        #region Operators

        /// <summary>
        /// Allow elements to be used as their internal type
        /// </summary>
        public static implicit operator LanguageCode?(LanguageCodeComboBoxItem item) => item.Data;

        #endregion

        #region Generation

        /// <summary>
        /// Generate all elements associated with the data type
        /// </summary>
        /// <param name="limited">Indicates if a limited, Redump-supported set should be returned</param>
        public static List<LanguageCodeComboBoxItem> GenerateElements(bool limited = false)
        {
            // If using a limited, Redump-supported set
            if (limited)
                return RedumpLanguages.ConvertAll(l => new LanguageCodeComboBoxItem(l));

            return [.. Array.ConvertAll(LanguageCode.AllLanguages, e => new LanguageCodeComboBoxItem(e))];
        }

        #endregion

        #region IEquatable Implementation

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

        #endregion
    }
}
