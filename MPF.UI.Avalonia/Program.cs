// MPF cross-platform (Avalonia) UI — contributed by Knutwurst (https://github.com/knutwurst)
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
        /// terminal commands, on every launch we strip the quarantine flag from, and ad-hoc
        /// code-sign, every Mach-O file under the sibling "Programs" folder (and add the
        /// @executable_path/lib rpath for tools that ship their dylibs in a sibling "lib"). Work
        /// is skipped when a binary is already signed / already has the rpath, so re-launches are
        /// cheap. Best effort: it degrades gracefully if codesign/xattr/install_name_tool/otool
        /// are unavailable (e.g. no Xcode Command Line Tools).
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
                Run("/usr/bin/xattr", out _, "-dr", "com.apple.quarantine", programsDir);

                foreach (string file in Directory.EnumerateFiles(programsDir, "*", SearchOption.AllDirectories))
                {
                    // Isolate per-file failures so one bad file doesn't abort the rest.
                    try
                    {
                        if (!IsMachO(file))
                            continue;

                        bool modified = false;

                        // Some tools (e.g. redumper) bundle their dylibs in a sibling "lib" folder
                        // but only carry an rpath pointing elsewhere, so dyld can't find them. Add
                        // an @executable_path/lib rpath for executables that have such a folder and
                        // don't already have it (avoids a duplicate-rpath error on re-launch).
                        string? dir = Path.GetDirectoryName(file);
                        bool isDylib = file.EndsWith(".dylib", StringComparison.OrdinalIgnoreCase);
                        if (!isDylib && dir != null && Directory.Exists(Path.Combine(dir, "lib")) && !HasExecutablePathLibRpath(file))
                        {
                            Run("/usr/bin/install_name_tool", out _, "-add_rpath", "@executable_path/lib", file);
                            modified = true;
                        }

                        // Ad-hoc sign if we changed the binary (install_name_tool invalidates the
                        // signature) or if it isn't validly signed yet. Skipping already-signed,
                        // unchanged binaries avoids rewriting them on every launch.
                        if (modified || !IsValidlySigned(file))
                            Run("/usr/bin/codesign", out _, "--force", "--sign", "-", file);
                    }
                    catch
                    {
                        // Ignore this file and continue with the rest.
                    }
                }
            }
            catch
            {
                // Best effort: if the directory can't be enumerated, fall back to OS behavior.
            }
        }

        /// <summary>True if the Mach-O already has an @executable_path/lib LC_RPATH.</summary>
        private static bool HasExecutablePathLibRpath(string file)
            => Run("/usr/bin/otool", out string output, "-l", file) == 0
               && output.Contains("@executable_path/lib");

        /// <summary>True if the file carries a valid (incl. ad-hoc) code signature.</summary>
        private static bool IsValidlySigned(string file)
            => Run("/usr/bin/codesign", out _, "--verify", file) == 0;

        /// <summary>
        /// Returns true if the file begins with a Mach-O / universal-binary magic number.
        /// </summary>
        private static bool IsMachO(string path)
        {
            // Java .class files share the 0xCAFEBABE magic with fat Mach-O binaries; skip them.
            if (path.EndsWith(".class", StringComparison.OrdinalIgnoreCase))
                return false;

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
        /// Runs a process, capturing stdout, and returns its exit code (-1 on failure).
        /// Only stdout is redirected (and fully drained before waiting) to avoid a pipe-buffer
        /// deadlock; stderr is left to the (absent) console.
        /// </summary>
        private static int Run(string fileName, out string output, params string[] arguments)
        {
            output = string.Empty;
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = false,
                    CreateNoWindow = true,
                };
                foreach (string arg in arguments)
                    psi.ArgumentList.Add(arg);

                using var proc = Process.Start(psi);
                if (proc is null)
                    return -1;

                output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit(15000);
                return proc.HasExited ? proc.ExitCode : -1;
            }
            catch
            {
                // Tool missing or not permitted.
                return -1;
            }
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                // No bundled font: use the OS system font (San Francisco on macOS) for a native look.
                .LogToTrace();
    }
}
