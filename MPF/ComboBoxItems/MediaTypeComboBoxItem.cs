using MPF.Data;
using MPF.Utilities;

namespace MPF
{
    /// <summary>
    /// Represents a single item in the MediaType combo box
    /// </summary>
    public class MediaTypeComboBoxItem
    {
        private MediaType? data;

        public MediaTypeComboBoxItem(MediaType? mediaType) => data = mediaType;

        public static implicit operator MediaType? (MediaTypeComboBoxItem item) => item.data;

        public string Name { get { return data.LongName(); }
        }
    }
}
