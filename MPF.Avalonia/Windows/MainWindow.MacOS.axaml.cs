using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MPF.Avalonia.Windows
{
    /// <summary>
    /// macOS-specific functionality for the main window: the native application menu and
    /// output path normalization
    /// </summary>
    public partial class MainWindow
    {
        #region Fields

        /// <summary>
        /// Guard against re-entrancy while normalizing the output path
        /// </summary>
        private bool _normalizingOutputPath;

        // macOS native menu items, retained so their headers can be updated on language change
        private NativeMenuItem? _nativeFileMenuGroup;
        private NativeMenuItem? _nativeToolsMenuGroup;
        private NativeMenuItem? _nativeHelpMenuGroup;
        private NativeMenuItem? _nativeLanguageMenuGroup;
        private NativeMenuItem? _nativeExitMenuItem;
        private NativeMenuItem? _nativeCheckDumpMenuItem;
        private NativeMenuItem? _nativeCreateIrdMenuItem;
        private NativeMenuItem? _nativeOptionsMenuItem;
        private NativeMenuItem? _nativeDebugViewMenuItem;
        private NativeMenuItem? _nativeAboutMenuItem;
        private NativeMenuItem? _nativeCheckForUpdatesMenuItem;

        #endregion

        /// <summary>
        /// Build and assign the macOS native application menu
        /// </summary>
        private void ConfigurePlatformMenus()
        {
            if (!OperatingSystem.IsMacOS())
                return;

            var nativeMenu = new NativeMenu();
            _nativeExitMenuItem = CreateNativeMenuItem(StringResource("ExitMenuItemString", "Exit"), AppExitClick, this);
            _nativeFileMenuGroup = CreateNativeMenuGroup(StringResource("FileMenuString", "File"), _nativeExitMenuItem);
            nativeMenu.Add(_nativeFileMenuGroup);

            _nativeCheckDumpMenuItem = CreateNativeMenuItem(StringResource("CheckDumpMenuItemString", "Check Dump"), CheckDumpMenuItemClick, this);
            _nativeCreateIrdMenuItem = CreateNativeMenuItem(StringResource("CreatePS3IRDDumpMenuItemString", "Create PS3 IRD"), CreateIRDMenuItemClick, this);
            _nativeOptionsMenuItem = CreateNativeMenuItem(StringResource("OptionsDumpMenuItemString", "Options"), OptionsMenuItemClick, this);
            _nativeDebugViewMenuItem = CreateNativeMenuItem(StringResource("DebugInfoWindowMenuItemString", "Debug Info Window"), DebugViewClick, this);
            _nativeToolsMenuGroup = CreateNativeMenuGroup(
                StringResource("ToolsMenuString", "Tools"),
                _nativeCheckDumpMenuItem,
                _nativeCreateIrdMenuItem,
                _nativeOptionsMenuItem,
                _nativeDebugViewMenuItem);
            nativeMenu.Add(_nativeToolsMenuGroup);

            _nativeAboutMenuItem = CreateNativeMenuItem(StringResource("AboutMenuItemString", "About"), AboutClick, this);
            _nativeCheckForUpdatesMenuItem = CreateNativeMenuItem(StringResource("CheckForUpdateMenuItemString", "Check for Updates"), CheckForUpdatesClick, this);
            _nativeHelpMenuGroup = CreateNativeMenuGroup(
                StringResource("HelpMenuString", "Help"),
                _nativeAboutMenuItem,
                _nativeCheckForUpdatesMenuItem);
            nativeMenu.Add(_nativeHelpMenuGroup);

            _nativeLanguageMenuGroup = CreateNativeMenuGroup(
                NativeLanguageMenuHeader(),
                CreateNativeMenuItem("English", LanguageMenuItemClick, EnglishMenuItem),
                CreateNativeMenuItem("Deutsch", LanguageMenuItemClick, GermanMenuItem),
                CreateNativeMenuItem("Español", LanguageMenuItemClick, SpanishMenuItem),
                CreateNativeMenuItem("Français", LanguageMenuItemClick, FrenchMenuItem),
                CreateNativeMenuItem("Italiano", LanguageMenuItemClick, ItalianMenuItem),
                CreateNativeMenuItem("日本語", LanguageMenuItemClick, JapaneseMenuItem),
                CreateNativeMenuItem("한국어", LanguageMenuItemClick, KoreanMenuItem),
                CreateNativeMenuItem("Polski", LanguageMenuItemClick, PolishMenuItem),
                CreateNativeMenuItem("Português", LanguageMenuItemClick, PortugueseMenuItem),
                CreateNativeMenuItem("Русский", LanguageMenuItemClick, RussianMenuItem),
                CreateNativeMenuItem("Svenska", LanguageMenuItemClick, SwedishMenuItem),
                CreateNativeMenuItem("Українська", LanguageMenuItemClick, UkrainianMenuItem));
            nativeMenu.Add(_nativeLanguageMenuGroup);

            NativeMenu.SetMenu(this, nativeMenu);
        }

        /// <summary>
        /// Create a native menu group with the given header containing the provided items
        /// </summary>
        private static NativeMenuItem CreateNativeMenuGroup(string header, params NativeMenuItem[] items)
        {
            var group = new NativeMenuItem { Header = CleanMenuHeader(header) };
            var menu = new NativeMenu();
            foreach (NativeMenuItem item in items)
            {
                menu.Add(item);
            }

            group.Menu = menu;
            return group;
        }

        /// <summary>
        /// Create a native menu item that invokes the given click handler when clicked
        /// </summary>
        /// <param name="header">Header text for the menu item</param>
        /// <param name="onClick">Click event handler to invoke</param>
        /// <param name="sender">Sender to pass to the click handler</param>
        private static NativeMenuItem CreateNativeMenuItem(string header, EventHandler<RoutedEventArgs> onClick, object? sender)
        {
            var item = new NativeMenuItem { Header = CleanMenuHeader(header) };
            item.Click += (_, _) => onClick(sender, new RoutedEventArgs());
            return item;
        }

        /// <summary>
        /// Strip access-key underscores from a menu header for native display
        /// </summary>
        private static string CleanMenuHeader(string header)
            => header.Replace("_", string.Empty);

        /// <summary>
        /// Get the localized header for the native language menu group
        /// </summary>
        private static string NativeLanguageMenuHeader()
            => StringResource("LanguageMenuString", "ENG");

        /// <summary>
        /// Refresh the native language menu group header
        /// </summary>
        private void UpdateNativeLanguageMenuHeader()
        {
            var nativeLanguageMenuHeader = NativeLanguageMenuHeader();
            _nativeLanguageMenuGroup?.Header = CleanMenuHeader(nativeLanguageMenuHeader);
        }

        /// <summary>
        /// Refresh all native menu headers to the current interface language
        /// </summary>
        private void UpdateNativeMenuHeaders()
        {
            if (!OperatingSystem.IsMacOS())
                return;

            _nativeFileMenuGroup?.Header = CleanMenuHeader(StringResource("FileMenuString", "File"));
            _nativeToolsMenuGroup?.Header = CleanMenuHeader(StringResource("ToolsMenuString", "Tools"));
            _nativeHelpMenuGroup?.Header = CleanMenuHeader(StringResource("HelpMenuString", "Help"));
            _nativeExitMenuItem?.Header = CleanMenuHeader(StringResource("ExitMenuItemString", "Exit"));
            _nativeCheckDumpMenuItem?.Header = CleanMenuHeader(StringResource("CheckDumpMenuItemString", "Check Dump"));
            _nativeCreateIrdMenuItem?.Header = CleanMenuHeader(StringResource("CreatePS3IRDDumpMenuItemString", "Create PS3 IRD"));
            _nativeOptionsMenuItem?.Header = CleanMenuHeader(StringResource("OptionsDumpMenuItemString", "Options"));
            _nativeDebugViewMenuItem?.Header = CleanMenuHeader(StringResource("DebugInfoWindowMenuItemString", "Debug Info Window"));
            _nativeAboutMenuItem?.Header = CleanMenuHeader(StringResource("AboutMenuItemString", "About"));
            _nativeCheckForUpdatesMenuItem?.Header = CleanMenuHeader(StringResource("CheckForUpdateMenuItemString", "Check for Updates"));

            UpdateNativeLanguageMenuHeader();
        }

        /// <summary>
        /// Remove the generated /Volumes/ prefix from the output path on macOS
        /// </summary>
        private void NormalizeMacOutputPath()
        {
            if (_normalizingOutputPath || !OperatingSystem.IsMacOS())
                return;

            string normalized = RemoveMacVolumesPrefix(MainViewModel.OutputPath);
            if (normalized == MainViewModel.OutputPath)
                return;

            try
            {
                _normalizingOutputPath = true;
                MainViewModel.OutputPath = normalized;
            }
            finally
            {
                _normalizingOutputPath = false;
            }
        }

        /// <summary>
        /// Strip the sanitized "_Volumes_" prefix that can leak into generated output paths
        /// </summary>
        private static string RemoveMacVolumesPrefix(string outputPath)
        {
            const string generatedVolumesPrefix = "_Volumes_";

            if (string.IsNullOrEmpty(outputPath))
                return outputPath;

            string normalized = outputPath;
            if (normalized.StartsWith(generatedVolumesPrefix, StringComparison.Ordinal))
                normalized = normalized[generatedVolumesPrefix.Length..];

            return normalized
                .Replace($"/{generatedVolumesPrefix}", "/", StringComparison.Ordinal)
                .Replace($"\\{generatedVolumesPrefix}", "\\", StringComparison.Ordinal);
        }
    }
}
