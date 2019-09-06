using DICUI.Data;
using DICUI.Utilities;

namespace DICUI
{
    /// <summary>
    /// Represents a single item in the Language combo box
    /// </summary>
    public class LanguageComboBoxItem
    {
        private object data;

        public LanguageComboBoxItem(Language? region) => data = region;

        public static implicit operator Language? (LanguageComboBoxItem item) => item.data as Language?;

        public string Name
        {
            get { return (data as Language?).LongName(); }
        }

        public bool IsChecked { get; set; }

        public Language? Value
        {
            get { return data as Language?; }
        }
    }
}
