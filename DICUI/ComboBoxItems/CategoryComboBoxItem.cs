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

        public CategoryComboBoxItem(Category? category) => data = category;

        public static implicit operator Category? (CategoryComboBoxItem item) => item.data as Category?;

        public string Name
        {
            get { return (data as Category?).LongName(); }
        }

        public bool IsChecked { get; set; }

        public Category? Value
        {
            get { return data as Category?; }
        }
    }
}
