using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MPF.Frontend;
using MPF.Frontend.Tools;
using MPF.Avalonia.Services;
using MPF.Avalonia.Windows;

namespace MPF.Avalonia
{
    /// <summary>
    /// Interaction logic for App.axaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Load the XAML, string resources, and set the application name
        /// </summary>
        public override void Initialize()
        {
            Name = "MPF";
            AvaloniaXamlLoader.Load(this);
            StringResourceLoader.Load(Resources, InterfaceLanguage.English);
        }

        /// <summary>
        /// Apply the configured theme and create the main window once the framework is ready
        /// </summary>
        public override void OnFrameworkInitializationCompleted()
        {
            ThemeService.ApplySystemTheme(Resources, OptionsLoader.LoadFromConfig());

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.MainWindow = new MainWindow();

            base.OnFrameworkInitializationCompleted();
        }
    }
}
