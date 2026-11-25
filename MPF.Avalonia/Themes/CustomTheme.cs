using Avalonia.Media;

namespace MPF.Avalonia.Themes
{
    /// <summary>
    /// Custom theme
    /// </summary>
    internal sealed class CustomTheme : Theme
    {
        // TODO: Figure out how to have the rest of the colors set properly, currently based on dark mode values
        public CustomTheme(string? backgroundColor, string? textColor)
        {
            // Setup needed brushes
            var panelBackgroundBrush = new SolidColorBrush(Color.Parse("#FF19191C"));
            var panelBorderBrush = new SolidColorBrush(Color.Parse("#FF303036"));
            var interactiveBackgroundBrush = new SolidColorBrush(Color.Parse("#FF2A2A31"));
            var interactiveHoverBackgroundBrush = new SolidColorBrush(Color.Parse("#FF34343C"));
            var interactivePressedBackgroundBrush = new SolidColorBrush(Color.Parse("#FF151517"));
            var interactiveBorderBrush = new SolidColorBrush(Color.Parse("#FF383840"));

            // Handle custom overrides
            var windowBackgroundBrush = new SolidColorBrush(Color.Parse("#FF131314"));
            if (!string.IsNullOrWhiteSpace(backgroundColor) && Color.TryParse(backgroundColor, out Color customBackground))
                windowBackgroundBrush = new SolidColorBrush(customBackground);

            var windowForegroundBrush = new SolidColorBrush(Color.Parse("#FFD8D8D8"));
            if (!string.IsNullOrWhiteSpace(textColor) && Color.TryParse(textColor, out Color customForeground))
                windowForegroundBrush = new SolidColorBrush(customForeground);

            // Handle application-wide resources
            AppBackgroundBrush = windowBackgroundBrush;
            AppForegroundBrush = windowForegroundBrush;
            HeadingForegroundBrush = new SolidColorBrush(Color.Parse("#FFCFCFCF"));
            DisabledForegroundBrush = new SolidColorBrush(Color.Parse("#FF5C5C5C"));

            // Handle Panel-specific resources
            PanelBackgroundBrush = panelBackgroundBrush;
            PanelBorderBrush = panelBorderBrush;

            // Handle Input-specific resources
            InputBackgroundBrush = interactiveBackgroundBrush;
            DisabledInputBackgroundBrush = new SolidColorBrush(Color.Parse("#FF1D1D21"));
            DisabledInputForegroundBrush = new SolidColorBrush(Color.Parse("#FF8F8F96"));

            // Handle Button-specific resources
            ButtonBackgroundBrush = interactiveBackgroundBrush;
            ButtonHoverBackgroundBrush = interactiveHoverBackgroundBrush;
            ButtonPressedBackgroundBrush = interactivePressedBackgroundBrush;
            ButtonDisabledBackgroundBrush = windowBackgroundBrush;
            ButtonBorderBrush = interactiveBorderBrush;
            ButtonHoverBorderBrush = new SolidColorBrush(Color.Parse("#FF464650"));
            ButtonPressedBorderBrush = panelBorderBrush;
            ButtonDisabledBorderBrush = panelBorderBrush;

            // Handle Menu-specific resources
            MenuBackgroundBrush = new SolidColorBrush(Colors.Transparent);
            MenuSubMenuBackgroundBrush = panelBackgroundBrush;
            MenuSubMenuBorderBrush = interactiveBorderBrush;
            MenuItemHoverBackgroundBrush = interactiveHoverBackgroundBrush;
            MenuItemPressedBackgroundBrush = interactiveBackgroundBrush;

            // Handle Title Bar-specific resources
            TitleBarButtonHoverBrush = interactiveBackgroundBrush;
            TitleBarButtonPressedBrush = interactivePressedBackgroundBrush;

            // Handle Log-specific resources
            LogBackgroundBrush = new SolidColorBrush(Color.Parse("#FF151515"));
            LogForegroundBrush = new SolidColorBrush(Color.Parse("#FFF0F0F0"));
        }
    }
}
