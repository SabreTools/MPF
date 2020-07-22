using DICUI.Data;
using DICUI.Utilities;

namespace DICUI
{
    /// <summary>
    /// Represents a single item in the Category combo box
    /// </summary>
    public class CategoryComboBoxItem
    {
        private object data;

        public CategoryComboBoxItem(DiscCategory? category) => data = category;

        public static implicit operator DiscCategory? (CategoryComboBoxItem item) => item.data as DiscCategory?;

        public string Name
        {
            get { return (data as DiscCategory?).LongName(); }
        }

        public bool IsChecked { get; set; }

        public DiscCategory? Value
        {
            get { return data as DiscCategory?; }
        }
    }
}
