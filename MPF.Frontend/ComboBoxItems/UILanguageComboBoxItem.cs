using System;
using System.Collections.Generic;

namespace MPF.Frontend.ComboBoxItems
{
    /// <summary>
    /// Represents a single item in the Default UI Language combo box
    /// </summary>
    public class UILanguageComboBoxItem : IEquatable<UILanguageComboBoxItem>, IElement
    {
        private readonly string UILanguage;

        private static readonly string[] languages = { "ENG", "한국어" };

        public UILanguageComboBoxItem(string language) => UILanguage = language;

        /// <inheritdoc/>
        public string Name
        {
            get
            {
                if (Array.Exists(languages, lang => lang == UILanguage))
                    return UILanguage;
                else
                    return "Auto Detect";
            }
        }

        public override string ToString() => Name;

        /// <summary>
        /// Generate all elements for the known system combo box
        /// </summary>
        /// <returns></returns>
        public static List<UILanguageComboBoxItem> GenerateElements()
        {
            var langValues = new List<UILanguageComboBoxItem>()
            {
                new UILanguageComboBoxItem(""),
            };

            foreach (var lang in languages)
            {
                langValues.Add(new UILanguageComboBoxItem(lang));
            }

            return langValues;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return Equals(obj as UILanguageComboBoxItem);
        }

        /// <inheritdoc/>
        public bool Equals(UILanguageComboBoxItem? other)
        {
            if (other == null)
                return false;

            return UILanguage == other.UILanguage;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => base.GetHashCode();
    }
}
