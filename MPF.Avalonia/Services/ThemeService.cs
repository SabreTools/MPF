using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using MPF.Frontend;

namespace MPF.Avalonia.Services
{
    internal static class ThemeService
    {
        public static void Apply(IResourceDictionary resources, Options options)
        {
            bool darkMode = options.GUI.Theming.EnableDarkMode;
            Color background = darkMode ? Color.Parse("#FF202020") : Color.Parse("#FFF5F5F5");
            Color foreground = darkMode ? Colors.White : Color.Parse("#FF111111");

            if (!string.IsNullOrWhiteSpace(options.GUI.Theming.CustomBackgroundColor)
                && Color.TryParse(options.GUI.Theming.CustomBackgroundColor, out Color customBackground))
            {
                background = customBackground;
            }

            if (!string.IsNullOrWhiteSpace(options.GUI.Theming.CustomTextColor)
                && Color.TryParse(options.GUI.Theming.CustomTextColor, out Color customForeground))
            {
                foreground = customForeground;
            }

            resources["AppBackgroundBrush"] = new SolidColorBrush(background);
            resources["AppForegroundBrush"] = new SolidColorBrush(foreground);
            resources["PanelBackgroundBrush"] = new SolidColorBrush(darkMode ? Color.Parse("#FF2B2B2B") : Colors.White);
            resources["PanelBorderBrush"] = new SolidColorBrush(darkMode ? Color.Parse("#FF4A4A4A") : Color.Parse("#FFD0D0D0"));
            resources["LogBackgroundBrush"] = new SolidColorBrush(Color.Parse("#FF202020"));
            resources["LogForegroundBrush"] = new SolidColorBrush(Color.Parse("#FFF0F0F0"));

            if (global::Avalonia.Application.Current is { } application)
                application.RequestedThemeVariant = darkMode ? ThemeVariant.Dark : ThemeVariant.Light;
        }
    }
}
