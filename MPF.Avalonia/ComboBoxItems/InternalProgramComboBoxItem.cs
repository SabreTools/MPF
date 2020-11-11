using MPF.Data;
using MPF.Utilities;

namespace MPF.Avalonia
{
    /// <summary>
    /// Represents a single item in the Internal Program combo box
    /// </summary>
    public class InternalProgramComboBoxItem
    {
        private object data;

        public InternalProgramComboBoxItem(InternalProgram? internalProgram) => data = internalProgram;

        public static implicit operator InternalProgram? (InternalProgramComboBoxItem item) => item.data as InternalProgram?;

        public string Name
        {
            get
            {
                return (data as InternalProgram?).LongName();
            }
        }

        public InternalProgram? Value
        {
            get { return data as InternalProgram?; }
        }
    }
}
