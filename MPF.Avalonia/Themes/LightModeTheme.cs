using Avalonia.Media;

namespace MPF.Avalonia.Themes
{
    /// <summary>
    /// Default light-mode theme
    /// </summary>
    internal sealed class LightModeTheme : Theme
    {
        public LightModeTheme()
        {
            // Setup needed brushes
            var whiteBrush = new SolidColorBrush(Colors.White);
            var panelBorderBrush = new SolidColorBrush(Color.Parse("#FFD0D0D0"));
            var interactiveBackgroundBrush = new SolidColorBrush(Color.Parse("#FFEDEDED"));
            var interactivePressedBackgroundBrush = new SolidColorBrush(Color.Parse("#FFD8D8D8"));
            var disabledBackgroundBrush = new SolidColorBrush(Color.Parse("#FFE2E2E2"));
            var disabledForegroundBrush = new SolidColorBrush(Color.Parse("#FF8A8A8A"));

            // Handle application-wide resources
            AppBackgroundBrush = new SolidColorBrush(Color.Parse("#FFF5F5F5"));
            AppForegroundBrush = new SolidColorBrush(Color.Parse("#FF111111"));
            HeadingForegroundBrush = new SolidColorBrush(Color.Parse("#FF111111"));
            DisabledForegroundBrush = disabledForegroundBrush;

            // Handle Panel-specific resources
            PanelBackgroundBrush = whiteBrush;
            PanelBorderBrush = panelBorderBrush;

            // Handle Input-specific resources
            InputBackgroundBrush = whiteBrush;
            DisabledInputBackgroundBrush = disabledBackgroundBrush;
            DisabledInputForegroundBrush = disabledForegroundBrush;

            // Handle Button-specific resources
            ButtonBackgroundBrush = interactiveBackgroundBrush;
            ButtonHoverBackgroundBrush = new SolidColorBrush(Color.Parse("#FFF5F5F5"));
            ButtonPressedBackgroundBrush = interactivePressedBackgroundBrush;
            ButtonDisabledBackgroundBrush = disabledBackgroundBrush;
            ButtonBorderBrush = new SolidColorBrush(Color.Parse("#FFB8B8B8"));
            ButtonHoverBorderBrush = new SolidColorBrush(Color.Parse("#FF9F9F9F"));
            ButtonPressedBorderBrush = new SolidColorBrush(Color.Parse("#FF8E8E8E"));
            ButtonDisabledBorderBrush = new SolidColorBrush(Color.Parse("#FFD2D2D2"));

            // Handle Menu-specific resources
            MenuBackgroundBrush = new SolidColorBrush(Colors.Transparent);
            MenuSubMenuBackgroundBrush = whiteBrush;
            MenuSubMenuBorderBrush = panelBorderBrush;
            MenuItemHoverBackgroundBrush = interactiveBackgroundBrush;
            MenuItemPressedBackgroundBrush = interactivePressedBackgroundBrush;

            // Handle Title Bar-specific resources
            TitleBarButtonHoverBrush = new SolidColorBrush(Color.Parse("#FFE5E5E5"));
            TitleBarButtonPressedBrush = panelBorderBrush;

            // Handle Log-specific resources
            LogBackgroundBrush = new SolidColorBrush(Color.Parse("#FF202020"));
            LogForegroundBrush = new SolidColorBrush(Color.Parse("#FFF0F0F0"));
        }
    }
}
