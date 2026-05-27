using System;
using System.IO;
using Avalonia;

namespace MPF.UI.Avalonia
{
    internal sealed class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            SetWorkingDirectory();
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        /// <summary>
        /// MPF resolves bundled dumping programs via paths relative to the working directory
        /// (e.g. "Programs/Redumper/redumper"). When launched from a macOS .app bundle the OS sets
        /// the working directory to "/", so those relative paths never resolve. Point the working
        /// directory at the folder that holds the launchable artifact so a sibling "Programs" folder
        /// is found: next to the .app bundle on macOS, otherwise next to the executable.
        /// </summary>
        private static void SetWorkingDirectory()
        {
            try
            {
                var baseDir = new DirectoryInfo(AppContext.BaseDirectory);

                // Inside a macOS .app bundle the layout is <Name>.app/Contents/MacOS/<exe>.
                // Walk up to the directory that contains the .app so a sibling Programs/ resolves.
                if (baseDir.Name == "MacOS"
                    && baseDir.Parent?.Name == "Contents"
                    && baseDir.Parent.Parent?.Extension == ".app"
                    && baseDir.Parent.Parent.Parent is { } appContainer)
                {
                    Environment.CurrentDirectory = appContainer.FullName;
                }
                else
                {
                    Environment.CurrentDirectory = baseDir.FullName;
                }
            }
            catch
            {
                // Leave the working directory unchanged if anything goes wrong.
            }
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
