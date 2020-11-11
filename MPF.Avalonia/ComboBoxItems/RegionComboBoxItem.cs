using MPF.Utilities;
using MPF.Web;

namespace MPF.Avalonia
{
    /// <summary>
    /// Represents a single item in the Region combo box
    /// </summary>
    public class RegionComboBoxItem
    {
        private object data;

        public RegionComboBoxItem(Region? region) => data = region;

        public static implicit operator Region? (RegionComboBoxItem item) => item.data as Region?;

        public string Name
        {
            get
            {
                return (data as Region?).LongName();
            }
        }

        public Region? Value
        {
            get { return data as Region?; }
        }
    }
}
