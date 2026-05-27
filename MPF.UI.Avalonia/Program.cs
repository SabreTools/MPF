using System;
using System.Diagnostics;
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
            PrepareExternalTools();
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

        /// <summary>
        /// macOS blocks unsigned, downloaded helper binaries (e.g. redumper, DiscImageCreator):
        /// Gatekeeper shows "cannot be opened" and on Apple Silicon the kernel kills unsigned
        /// Mach-O binaries outright. To make the bundled tools usable without the user running
        /// terminal commands, on first launch we strip the quarantine flag and apply an ad-hoc
        /// code signature to every Mach-O file under the sibling "Programs" folder.
        /// </summary>
        private static void PrepareExternalTools()
        {
            if (!OperatingSystem.IsMacOS())
                return;

            try
            {
                string programsDir = Path.Combine(Environment.CurrentDirectory, "Programs");
                if (!Directory.Exists(programsDir))
                    return;

                // Remove the download quarantine flag from the whole tree (best effort).
                RunQuiet("/usr/bin/xattr", "-dr", "com.apple.quarantine", programsDir);

                // Ad-hoc sign every Mach-O file so it can run locally on a signed-only OS.
                foreach (string file in Directory.EnumerateFiles(programsDir, "*", SearchOption.AllDirectories))
                {
                    if (IsMachO(file))
                        RunQuiet("/usr/bin/codesign", "--force", "--sign", "-", file);
                }
            }
            catch
            {
                // Best effort: if codesign/xattr are unavailable, fall back to the OS prompt.
            }
        }

        /// <summary>
        /// Returns true if the file begins with a Mach-O / universal-binary magic number.
        /// </summary>
        private static bool IsMachO(string path)
        {
            try
            {
                using var fs = File.OpenRead(path);
                Span<byte> magic = stackalloc byte[4];
                if (fs.Read(magic) != 4)
                    return false;

                uint m = (uint)(magic[0] << 24 | magic[1] << 16 | magic[2] << 8 | magic[3]);
                return m is 0xFEEDFACE // 32-bit
                          or 0xFEEDFACF // 64-bit
                          or 0xCEFAEDFE // 32-bit, byte-swapped
                          or 0xCFFAEDFE // 64-bit, byte-swapped
                          or 0xCAFEBABE // universal (fat)
                          or 0xBEBAFECA; // universal, byte-swapped
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Runs a process, discarding output and ignoring failures.
        /// </summary>
        private static void RunQuiet(string fileName, params string[] arguments)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                };
                foreach (string arg in arguments)
                    psi.ArgumentList.Add(arg);

                using var proc = Process.Start(psi);
                proc?.WaitForExit(15000);
            }
            catch
            {
                // Ignore: tool missing or not permitted.
            }
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
