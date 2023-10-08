using System;
using System.Globalization;
using System.Windows.Data;
using MPF.Core.Data;
using MPF.Core.UI.ComboBoxItems;
using SabreTools.RedumpLib.Data;

namespace MPF.UI.Core
{
    internal class ElementConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case InternalProgram internalProgram:
                    return new Element<InternalProgram>(internalProgram);
                case RedumpSystem redumpSystem:
                    return new RedumpSystemComboBoxItem(redumpSystem);

                // Null values are treated as a system value
                default:
                    return new RedumpSystemComboBoxItem((RedumpSystem?)null);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // If it's an IElement but ends up null
            var element = value as IElement;
            if (element == null)
                return null;

            switch (element)
            {
                case Element<InternalProgram> ipElement:
                    return ipElement.Value;
                case RedumpSystemComboBoxItem rsElement:
                    return rsElement.Value;

                default: return null;
            }
        }
    }
}
