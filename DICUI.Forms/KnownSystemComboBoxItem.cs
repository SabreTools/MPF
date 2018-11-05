using DICUI.Data;
using DICUI.Utilities;

namespace DICUI
{
    /// <summary>
    /// Represents a single item in the System combo box
    /// </summary>
    public class KnownSystemComboBoxItem
    {
        private object data;

        public KnownSystemComboBoxItem(KnownSystem? system) => data = system;
        public KnownSystemComboBoxItem(KnownSystemCategory? category) => data = category;

        //public Brush Foreground { get => IsHeader() ? Brushes.Gray : Brushes.Black; } // TODO: Re-enable this

        public bool IsHeader() => data is KnownSystemCategory?;
        public bool IsSystem() => data is KnownSystem?;

        public static implicit operator KnownSystem? (KnownSystemComboBoxItem item) => item.data as KnownSystem?;

        public string Name
        {
            get
            {
                if (IsHeader())
                    return "---------- " + (data as KnownSystemCategory?).Name() + " ----------";
                else
                    return (data as KnownSystem?).Name();
            }
        }
    }
}
