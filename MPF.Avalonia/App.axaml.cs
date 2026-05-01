using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MPF.Frontend.Tools;
using MPF.Avalonia.Services;
using MPF.Avalonia.Windows;

namespace MPF.Avalonia
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            Name = "MPF";
            AvaloniaXamlLoader.Load(this);
            StringResourceLoader.LoadEnglish(Resources);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            ThemeService.ApplySystemTheme(Resources, OptionsLoader.LoadFromConfig());

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.MainWindow = new MainWindow();

            base.OnFrameworkInitializationCompleted();
        }
    }
}
