using System.Windows.Media;

namespace MPF.UI.Themes
{
    /// <summary>
    /// Custom colour theme
    /// </summary>
    public sealed class CustomTheme : Theme
    {
        public CustomTheme(string? backgroundColor, string? foregroundColor)
        {
            if (!IsHexColor(backgroundColor) || !IsHexColor(foregroundColor))
                return;

            // Setup needed brushes
            var backgroundBrush = new SolidColorBrush { Color = Color.FromArgb(0xff, HexToHex(backgroundColor!.Substring(0, 2)), HexToHex(backgroundColor.Substring(2, 2)), HexToHex(backgroundColor.Substring(4, 2))) };
            var foregroundBrush = new SolidColorBrush { Color = Color.FromArgb(0xff, HexToHex(foregroundColor!.Substring(0, 2)), HexToHex(foregroundColor.Substring(2, 2)), HexToHex(foregroundColor.Substring(4, 2))) };

            // Handle application-wide resources
            this.ActiveBorderBrush = foregroundBrush;
            this.ControlBrush = backgroundBrush;
            this.ControlTextBrush = foregroundBrush;
            this.GrayTextBrush = foregroundBrush;
            this.WindowBrush = backgroundBrush;
            this.WindowTextBrush = foregroundBrush;

            // Handle Button-specific resources
            this.Button_Disabled_Background = backgroundBrush;
            this.Button_MouseOver_Background = backgroundBrush;
            this.Button_Pressed_Background = backgroundBrush;
            this.Button_Static_Background = backgroundBrush;

            // Handle ComboBox-specific resources
            this.ComboBox_Disabled_Background = backgroundBrush;
            this.ComboBox_Disabled_Editable_Background = backgroundBrush;
            this.ComboBox_Disabled_Editable_Button_Background = backgroundBrush;
            this.ComboBox_MouseOver_Background = backgroundBrush;
            this.ComboBox_MouseOver_Editable_Background = backgroundBrush;
            this.ComboBox_MouseOver_Editable_Button_Background = backgroundBrush;
            this.ComboBox_Pressed_Background = backgroundBrush;
            this.ComboBox_Pressed_Editable_Background = backgroundBrush;
            this.ComboBox_Pressed_Editable_Button_Background = backgroundBrush;
            this.ComboBox_Static_Background = backgroundBrush;
            this.ComboBox_Static_Editable_Background = backgroundBrush;
            this.ComboBox_Static_Editable_Button_Background = backgroundBrush;

            // Handle CustomMessageBox-specific resources
            this.CustomMessageBox_Static_Background = backgroundBrush;

            // Handle MenuItem-specific resources
            this.MenuItem_SubMenu_Background = backgroundBrush;
            this.MenuItem_SubMenu_Border = Brushes.DarkGray;

            // Handle ProgressBar-specific resources
            this.ProgressBar_Background = backgroundBrush;

            // Handle ScrollViewer-specific resources
            this.ScrollViewer_ScrollBar_Background = backgroundBrush;

            // Handle TabItem-specific resources
            this.TabItem_Selected_Background = backgroundBrush;
            this.TabItem_Static_Background = backgroundBrush;
            this.TabItem_Static_Border = Brushes.DarkGray;

            // Handle TextBox-specific resources
            this.TextBox_Static_Background = backgroundBrush;
        }

        /// <summary>
        /// Check whether a string represents a valid hex color
        /// </summary>
        public static bool IsHexColor(string? color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return false;

            if (color!.Length == 7 && color[0] == '#')
                color = color.Substring(1);

            if (color.Length != 6)
                return false;

            for (int i = 0; i < color.Length; i++)
            {
                switch (color[i])
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'E':
                    case 'F':
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                        continue;
                    default:
                        break;
                }
            }

            return true;
        }

        /// <summary>
        /// Converts string hex to int hex
        /// </summary>
        private static byte HexToHex(string? hex)
        {
            byte result = 0;

            if (string.IsNullOrEmpty(hex))
                return result;

            if (hex!.Length != 2)
                return result;

            switch (hex[0])
            {
                case '0':
                    result = 0 * 16;
                    break;
                case '1':
                    result = 1 * 16;
                    break;
                case '2':
                    result = 2 * 16;
                    break;
                case '3':
                    result = 3 * 16;
                    break;
                case '4':
                    result = 4 * 16;
                    break;
                case '5':
                    result = 5 * 16;
                    break;
                case '6':
                    result = 6 * 16;
                    break;
                case '7':
                    result = 7 * 16;
                    break;
                case '8':
                    result = 8 * 16;
                    break;
                case '9':
                    result = 9 * 16;
                    break;
                case 'A':
                    result = 0xA * 16;
                    break;
                case 'B':
                    result = 0xB * 16;
                    break;
                case 'C':
                    result = 0xC * 16;
                    break;
                case 'D':
                    result = 0xD * 16;
                    break;
                case 'E':
                    result = 0xE * 16;
                    break;
                case 'F':
                    result = 0xF * 16;
                    break;
                case 'a':
                    result = 0xa * 16;
                    break;
                case 'b':
                    result = 0xb * 16;
                    break;
                case 'c':
                    result = 0xc * 16;
                    break;
                case 'd':
                    result = 0xd * 16;
                    break;
                case 'e':
                    result = 0xe * 16;
                    break;
                case 'f':
                    result = 0xf * 16;
                    break;
                default:
                    return 0;
            }

            switch (hex[1])
            {
                case '0':
                    result += 0;
                    return result;
                case '1':
                    result += 1;
                    return result;
                case '2':
                    result += 2;
                    return result;
                case '3':
                    result += 3;
                    return result;
                case '4':
                    result += 4;
                    return result;
                case '5':
                    result += 5;
                    return result;
                case '6':
                    result += 6;
                    return result;
                case '7':
                    result += 7;
                    return result;
                case '8':
                    result += 8;
                    return result;
                case '9':
                    result += 9;
                    return result;
                case 'A':
                    result += 0xA;
                    return result;
                case 'B':
                    result += 0xB;
                    return result;
                case 'C':
                    result += 0xC;
                    return result;
                case 'D':
                    result += 0xD;
                    return result;
                case 'E':
                    result += 0xE;
                    return result;
                case 'F':
                    result += 0xF;
                    return result;
                case 'a':
                    result += 0xa;
                    return result;
                case 'b':
                    result += 0xb;
                    return result;
                case 'c':
                    result += 0xc;
                    return result;
                case 'd':
                    result += 0xd;
                    return result;
                case 'e':
                    result += 0xe;
                    return result;
                case 'f':
                    result += 0xf;
                    return result;
                default:
                    return 0;
            }
        }
    }
}
