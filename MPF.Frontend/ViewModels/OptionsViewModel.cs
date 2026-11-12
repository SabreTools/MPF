using System.Collections.Generic;
using System.ComponentModel;
using MPF.Frontend.ComboBoxItems;
using SabreTools.RedumpLib.Data;
using DreamdumpSectorOrder = MPF.ExecutionContexts.Dreamdump.SectorOrder;
using LogCompression = MPF.Processors.LogCompression;
using RedumperDriveType = MPF.ExecutionContexts.Redumper.DriveType;
using RedumperReadMethod = MPF.ExecutionContexts.Redumper.ReadMethod;
using RedumperSectorOrder = MPF.ExecutionContexts.Redumper.SectorOrder;

namespace MPF.Frontend.ViewModels
{
    /// <summary>
    /// Constructor
    /// </summary>
    public class OptionsViewModel : INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// Current set of options
        /// </summary>
        public Options Options { get; }

        /// <summary>
        /// Flag for if settings were saved or not
        /// </summary>
        public bool SavedSettings { get; set; }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Properties

        /// <inheritdoc cref="ExecutionContexts.Aaru.DumpSettings.EnableDebug"/>
        public bool AaruEnableDebug
        {
            get => Options.Dumping.Aaru.EnableDebug;
            set
            {
                Options.Dumping.Aaru.EnableDebug = value;
                TriggerPropertyChanged(nameof(AaruEnableDebug));
            }
        }

        /// <inheritdoc cref="ExecutionContexts.Aaru.DumpSettings.EnableVerbose"/>
        public bool AaruEnableVerbose
        {
            get => Options.Dumping.Aaru.EnableVerbose;
            set
            {
                Options.Dumping.Aaru.EnableVerbose = value;
                TriggerPropertyChanged(nameof(AaruEnableVerbose));
            }
        }

        /// <inheritdoc cref="ExecutionContexts.Aaru.DumpSettings.ForceDumping"/>
        public bool AaruForceDumping
        {
            get => Options.Dumping.Aaru.ForceDumping;
            set
            {
                Options.Dumping.Aaru.ForceDumping = value;
                TriggerPropertyChanged(nameof(AaruForceDumping));
            }
        }

        /// <inheritdoc cref="DumpSettings.AaruPath"/>
        public string AaruPath
        {
            get => Options.Dumping.AaruPath;
            set
            {
                Options.Dumping.AaruPath = value;
                TriggerPropertyChanged(nameof(AaruPath));
            }
        }

        /// <inheritdoc cref="ExecutionContexts.Aaru.DumpSettings.RereadCount"/>
        public int AaruRereadCount
        {
            get => Options.Dumping.Aaru.RereadCount;
            set
            {
                Options.Dumping.Aaru.RereadCount = value;
                TriggerPropertyChanged(nameof(AaruRereadCount));
            }
        }

        /// <inheritdoc cref="ExecutionContexts.Aaru.DumpSettings.StripPersonalData"/>
        public bool AaruStripPersonalData
        {
            get => Options.Dumping.Aaru.StripPersonalData;
            set
            {
                Options.Dumping.Aaru.StripPersonalData = value;
                TriggerPropertyChanged(nameof(AaruStripPersonalData));
            }
        }

        /// <inheritdoc cref="ProcessingSettings.AddFilenameSuffix"/>
        public bool AddFilenameSuffix
        {
            get => Options.Processing.AddFilenameSuffix;
            set
            {
                Options.Processing.AddFilenameSuffix = value;
                TriggerPropertyChanged(nameof(AddFilenameSuffix));
            }
        }

        /// <inheritdoc cref="MediaInformationSettings.AddPlaceholders"/>
        public bool AddPlaceholders
        {
            get => Options.Processing.MediaInformation.AddPlaceholders;
            set
            {
                Options.Processing.MediaInformation.AddPlaceholders = value;
                TriggerPropertyChanged(nameof(AddPlaceholders));
            }
        }

        /// <inheritdoc cref="Options.CheckForUpdatesOnStartup"/>
        public bool CheckForUpdatesOnStartup
        {
            get => Options.CheckForUpdatesOnStartup;
            set
            {
                Options.CheckForUpdatesOnStartup = value;
                TriggerPropertyChanged(nameof(CheckForUpdatesOnStartup));
            }
        }

        /// <inheritdoc cref="ProcessingSettings.CompressLogFiles"/>
        public bool CompressLogFiles
        {
            get => Options.Processing.CompressLogFiles;
            set
            {
                Options.Processing.CompressLogFiles = value;
                TriggerPropertyChanged(nameof(CompressLogFiles));
            }
        }

        /// <inheritdoc cref="ProcessingSettings.CreateIRDAfterDumping"/>
        public bool CreateIRDAfterDumping
        {
            get => Options.Processing.CreateIRDAfterDumping;
            set
            {
                Options.Processing.CreateIRDAfterDumping = value;
                TriggerPropertyChanged(nameof(CreateIRDAfterDumping));
            }
        }

        /// <inheritdoc cref="GuiSettings.DefaultInterfaceLanguage"/>
        public InterfaceLanguage DefaultInterfaceLanguage
        {
            get => Options.GUI.DefaultInterfaceLanguage;
            set
            {
                Options.GUI.DefaultInterfaceLanguage = value;
                TriggerPropertyChanged(nameof(DefaultInterfaceLanguage));
            }
        }

        /// <inheritdoc cref="DumpSettings.DefaultOutputPath"/>
        public string? DefaultOutputPath
        {
            get => Options.Dumping.DefaultOutputPath;
            set
            {
                Options.Dumping.DefaultOutputPath = value;
                TriggerPropertyChanged(nameof(DefaultOutputPath));
            }
        }

        /// <inheritdoc cref="GuiSettings.DefaultInterfaceLanguage"/>
        public PhysicalSystem? DefaultSystem
        {
            get => Options.Dumping.DefaultSystem;
            set
            {
                Options.Dumping.DefaultSystem = value;
                TriggerPropertyChanged(nameof(DefaultSystem));
            }
        }

        /// <inheritdoc cref="ProcessingSettings.DeleteUnnecessaryFiles"/>
        public bool DeleteUnnecessaryFiles
        {
            get => Options.Processing.DeleteUnnecessaryFiles;
            set
            {
                Options.Processing.DeleteUnnecessaryFiles = value;
                TriggerPropertyChanged(nameof(DeleteUnnecessaryFiles));
            }
        }

        /// <inheritdoc cref="ExecutionContexts.DiscImageCreator.DumpSettings.RereadCount"/>
        public int DiscImageCreatorCDRereadCount
        {
            get => Options.Dumping.DIC.RereadCount;
            set
            {
                Options.Dumping.DIC.RereadCount = value;
                TriggerPropertyChanged(nameof(DiscImageCreatorCDRereadCount));
            }
        }

        /// <inheritdoc cref="ExecutionContexts.DiscImageCreator.DumpSettings.DVDRereadCount"/>
        public int DiscImageCreatorDVDRereadCount
        {
            get => Options.Dumping.DIC.DVDRereadCount;
            set
            {
                Options.Dumping.DIC.DVDRereadCount = value;
                TriggerPropertyChanged(nameof(DiscImageCreatorDVDRereadCount));
            }
        }

        /// <inheritdoc cref="ExecutionContexts.DiscImageCreator.DumpSettings.MultiSectorRead"/>
        public bool DiscImageCreatorMultiSectorRead
        {
            get => Options.Dumping.DIC.MultiSectorRead;
            set
            {
                Options.Dumping.DIC.MultiSectorRead = value;
                TriggerPropertyChanged(nameof(DiscImageCreatorMultiSectorRead));
            }
        }

        /// <inheritdoc cref="ExecutionContexts.DiscImageCreator.DumpSettings.MultiSectorReadValue"/>
        public int DiscImageCreatorMultiSectorReadValue
        {
            get => Options.Dumping.DIC.MultiSectorReadValue;
            set
            {
                Options.Dumping.DIC.MultiSectorReadValue = value;
                TriggerPropertyChanged(nameof(DiscImageCreatorMultiSectorReadValue));
            }
        }

        /// <inheritdoc cref="ExecutionContexts.DiscImageCreator.DumpSettings.ParanoidMode"/>
        public bool DiscImageCreatorParanoidMode
        {
            get => Options.Dumping.DIC.ParanoidMode;
            set
            {
                Options.Dumping.DIC.ParanoidMode = value;
                TriggerPropertyChanged(nameof(DiscImageCreatorParanoidMode));
            }
        }

        /// <inheritdoc cref="DumpSettings.DiscImageCreatorPath"/>
        public string DiscImageCreatorPath
        {
            get => Options.Dumping.DiscImageCreatorPath;
            set
            {
                Options.Dumping.DiscImageCreatorPath = value;
                TriggerPropertyChanged(nameof(DiscImageCreatorPath));
            }
        }

        /// <inheritdoc cref="ExecutionContexts.DiscImageCreator.DumpSettings.QuietMode"/>
        public bool DiscImageCreatorQuietMode
        {
            get => Options.Dumping.DIC.QuietMode;
            set
            {
                Options.Dumping.DIC.QuietMode = value;
                TriggerPropertyChanged(nameof(DiscImageCreatorQuietMode));
            }
        }

        /// <inheritdoc cref="ExecutionContexts.DiscImageCreator.DumpSettings.UseCMIFlag"/>
        public bool DiscImageCreatorUseCMIFlag
        {
            get => Options.Dumping.DIC.UseCMIFlag;
            set
            {
                Options.Dumping.DIC.UseCMIFlag = value;
                TriggerPropertyChanged(nameof(DiscImageCreatorUseCMIFlag));
            }
        }

        /// <inheritdoc cref="ExecutionContexts.Dreamdump.DumpSettings.NonRedumpMode"/>
        public bool DreamdumpNonRedumpMode
        {
            get => Options.Dumping.Dreamdump.NonRedumpMode;
            set
            {
                Options.Dumping.Dreamdump.NonRedumpMode = value;
                TriggerPropertyChanged(nameof(DreamdumpNonRedumpMode));
            }
        }

        /// <inheritdoc cref="DumpSettings.DreamdumpPath"/>
        public string DreamdumpPath
        {
            get => Options.Dumping.DreamdumpPath;
            set
            {
                Options.Dumping.DreamdumpPath = value;
                TriggerPropertyChanged(nameof(DreamdumpPath));
            }
        }

        /// <inheritdoc cref="ExecutionContexts.Dreamdump.DumpSettings.RereadCount"/>
        public int DreamdumpRereadCount
        {
            get => Options.Dumping.Dreamdump.RereadCount;
            set
            {
                Options.Dumping.Dreamdump.RereadCount = value;
                TriggerPropertyChanged(nameof(DreamdumpRereadCount));
            }
        }

        /// <inheritdoc cref="ExecutionContexts.Dreamdump.DumpSettings.SectorOrder"/>
        public DreamdumpSectorOrder DreamdumpSectorOrder
        {
            get => Options.Dumping.Dreamdump.SectorOrder;
            set
            {
                Options.Dumping.Dreamdump.SectorOrder = value;
                TriggerPropertyChanged(nameof(DreamdumpSectorOrder));
            }
        }

        /// <inheritdoc cref="DumpSpeeds.Bluray"/>
        public int DumpSpeedBluray
        {
            get => Options.Dumping.DumpSpeeds.Bluray;
            set
            {
                Options.Dumping.DumpSpeeds.Bluray = value;
                TriggerPropertyChanged(nameof(DumpSpeedBluray));
            }
        }

        /// <inheritdoc cref="DumpSpeeds.CD"/>
        public int DumpSpeedCD
        {
            get => Options.Dumping.DumpSpeeds.CD;
            set
            {
                Options.Dumping.DumpSpeeds.CD = value;
                TriggerPropertyChanged(nameof(DumpSpeedCD));
            }
        }

        /// <inheritdoc cref="DumpSpeeds.DVD"/>
        public int DumpSpeedDVD
        {
            get => Options.Dumping.DumpSpeeds.DVD;
            set
            {
                Options.Dumping.DumpSpeeds.DVD = value;
                TriggerPropertyChanged(nameof(DumpSpeedDVD));
            }
        }

        /// <inheritdoc cref="DumpSpeeds.HDDVD"/>
        public int DumpSpeedHDDVD
        {
            get => Options.Dumping.DumpSpeeds.HDDVD;
            set
            {
                Options.Dumping.DumpSpeeds.HDDVD = value;
                TriggerPropertyChanged(nameof(DumpSpeedHDDVD));
            }
        }

        /// <inheritdoc cref="ThemeSettings.EnableDarkMode"/>
        public bool EnableDarkMode
        {
            get => Options.GUI.Theming.EnableDarkMode;
            set
            {
                Options.GUI.Theming.EnableDarkMode = value;
                TriggerPropertyChanged(nameof(EnableDarkMode));
            }
        }

        /// <inheritdoc cref="MediaInformationSettings.EnableRedumpCompatibility"/>
        public bool EnableRedumpCompatibility
        {
            get => Options.Processing.MediaInformation.EnableRedumpCompatibility;
            set
            {
                Options.Processing.MediaInformation.EnableRedumpCompatibility = value;
                TriggerPropertyChanged(nameof(EnableRedumpCompatibility));
            }
        }

        /// <inheritdoc cref="MediaInformationSettings.EnableTabsInInputFields"/>
        public bool EnableTabsInInputFields
        {
            get => Options.Processing.MediaInformation.EnableTabsInInputFields;
            set
            {
                Options.Processing.MediaInformation.EnableTabsInInputFields = value;
                TriggerPropertyChanged(nameof(EnableTabsInInputFields));
            }
        }

        /// <inheritdoc cref="GuiSettings.FastUpdateLabel"/>
        public bool FastUpdateLabel
        {
            get => Options.GUI.FastUpdateLabel;
            set
            {
                Options.GUI.FastUpdateLabel = value;
                TriggerPropertyChanged(nameof(FastUpdateLabel));
            }
        }

        /// <inheritdoc cref="ProtectionScanningSettings.HideDriveLetters"/>
        public bool HideDriveLetters
        {
            get => Options.Processing.ProtectionScanning.HideDriveLetters;
            set
            {
                Options.Processing.ProtectionScanning.HideDriveLetters = value;
                TriggerPropertyChanged(nameof(HideDriveLetters));
            }
        }

        /// <inheritdoc cref="GuiSettings.IgnoreFixedDrives"/>
        public bool IgnoreFixedDrives
        {
            get => Options.GUI.IgnoreFixedDrives;
            set
            {
                Options.GUI.IgnoreFixedDrives = value;
                TriggerPropertyChanged(nameof(IgnoreFixedDrives));
            }
        }

        /// <inheritdoc cref="ProcessingSettings.IncludeArtifacts"/>
        public bool IncludeArtifacts
        {
            get => Options.Processing.IncludeArtifacts;
            set
            {
                Options.Processing.IncludeArtifacts = value;
                TriggerPropertyChanged(nameof(IncludeArtifacts));
            }
        }

        /// <inheritdoc cref="ProtectionScanningSettings.IncludeDebugProtectionInformation"/>
        public bool IncludeDebugProtectionInformation
        {
            get => Options.Processing.ProtectionScanning.IncludeDebugProtectionInformation;
            set
            {
                Options.Processing.ProtectionScanning.IncludeDebugProtectionInformation = value;
                TriggerPropertyChanged(nameof(IncludeDebugProtectionInformation));
            }
        }

        /// <inheritdoc cref="Options.InternalProgram"/>
        public InternalProgram InternalProgram
        {
            get => Options.InternalProgram;
            set
            {
                Options.InternalProgram = value;
                TriggerPropertyChanged(nameof(InternalProgram));
            }
        }

        /// <inheritdoc cref="ProcessingSettings.LogCompression"/>
        public LogCompression LogCompression
        {
            get => Options.Processing.LogCompression;
            set
            {
                Options.Processing.LogCompression = value;
                TriggerPropertyChanged(nameof(LogCompression));
            }
        }

        /// <inheritdoc cref="GuiSettings.OpenLogWindowAtStartup"/>
        public bool OpenLogWindowAtStartup
        {
            get => Options.GUI.OpenLogWindowAtStartup;
            set
            {
                Options.GUI.OpenLogWindowAtStartup = value;
                TriggerPropertyChanged(nameof(OpenLogWindowAtStartup));
            }
        }

        /// <inheritdoc cref="ProcessingSettings.OutputSubmissionJSON"/>
        public bool OutputSubmissionJSON
        {
            get => Options.Processing.OutputSubmissionJSON;
            set
            {
                Options.Processing.OutputSubmissionJSON = value;
                TriggerPropertyChanged(nameof(OutputSubmissionJSON));
            }
        }

        /// <inheritdoc cref="MediaInformationSettings.PromptForDiscInformation"/>
        public bool PromptForDiscInformation
        {
            get => Options.Processing.MediaInformation.PromptForDiscInformation;
            set
            {
                Options.Processing.MediaInformation.PromptForDiscInformation = value;
                TriggerPropertyChanged(nameof(PromptForDiscInformation));
            }
        }

        /// <inheritdoc cref="SiteLoginSettings.PullAllInformation"/>
        public bool PullAllInformation
        {
            get => Options.Processing.Login.PullAllInformation;
            set
            {
                Options.Processing.Login.PullAllInformation = value;
                TriggerPropertyChanged(nameof(PullAllInformation));
            }
        }

        /// <inheritdoc cref="ExecutionContexts.Redumper.DumpSettings.DrivePregapStart"/>
        public int RedumperDrivePregapStart
        {
            get => Options.Dumping.Redumper.DrivePregapStart;
            set
            {
                Options.Dumping.Redumper.DrivePregapStart = value;
                TriggerPropertyChanged(nameof(RedumperDrivePregapStart));
            }
        }

        /// <inheritdoc cref="ExecutionContexts.Redumper.DumpSettings.DriveType"/>
        public RedumperDriveType RedumperDriveType
        {
            get => Options.Dumping.Redumper.DriveType;
            set
            {
                Options.Dumping.Redumper.DriveType = value;
                TriggerPropertyChanged(nameof(RedumperDriveType));
            }
        }

        /// <inheritdoc cref="ExecutionContexts.Redumper.DumpSettings.EnableVerbose"/>
        public bool RedumperEnableVerbose
        {
            get => Options.Dumping.Redumper.EnableVerbose;
            set
            {
                Options.Dumping.Redumper.EnableVerbose = value;
                TriggerPropertyChanged(nameof(RedumperEnableVerbose));
            }
        }

        /// <inheritdoc cref="ExecutionContexts.Redumper.DumpSettings.LeadinRetryCount"/>
        public int RedumperLeadinRetryCount
        {
            get => Options.Dumping.Redumper.LeadinRetryCount;
            set
            {
                Options.Dumping.Redumper.LeadinRetryCount = value;
                TriggerPropertyChanged(nameof(RedumperLeadinRetryCount));
            }
        }

        /// <inheritdoc cref="ExecutionContexts.Redumper.DumpSettings.NonRedumpMode"/>
        public bool RedumperNonRedumpMode
        {
            get => Options.Dumping.Redumper.NonRedumpMode;
            set
            {
                Options.Dumping.Redumper.NonRedumpMode = value;
                TriggerPropertyChanged(nameof(RedumperNonRedumpMode));
            }
        }

        /// <inheritdoc cref="DumpSettings.RedumperPath"/>
        public string RedumperPath
        {
            get => Options.Dumping.RedumperPath;
            set
            {
                Options.Dumping.RedumperPath = value;
                TriggerPropertyChanged(nameof(RedumperPath));
            }
        }

        /// <inheritdoc cref="ExecutionContexts.Redumper.DumpSettings.ReadMethod"/>
        public RedumperReadMethod RedumperReadMethod
        {
            get => Options.Dumping.Redumper.ReadMethod;
            set
            {
                Options.Dumping.Redumper.ReadMethod = value;
                TriggerPropertyChanged(nameof(RedumperReadMethod));
            }
        }

        /// <inheritdoc cref="ExecutionContexts.Redumper.DumpSettings.RefineSectorMode"/>
        public bool RedumperRefineSectorMode
        {
            get => Options.Dumping.Redumper.RefineSectorMode;
            set
            {
                Options.Dumping.Redumper.RefineSectorMode = value;
                TriggerPropertyChanged(nameof(RedumperRefineSectorMode));
            }
        }

        /// <inheritdoc cref="ExecutionContexts.Redumper.DumpSettings.SectorOrder"/>
        public RedumperSectorOrder RedumperSectorOrder
        {
            get => Options.Dumping.Redumper.SectorOrder;
            set
            {
                Options.Dumping.Redumper.SectorOrder = value;
                TriggerPropertyChanged(nameof(RedumperSectorOrder));
            }
        }

        /// <inheritdoc cref="ExecutionContexts.Redumper.DumpSettings.RereadCount"/>
        public int RedumperRereadCount
        {
            get => Options.Dumping.Redumper.RereadCount;
            set
            {
                Options.Dumping.Redumper.RereadCount = value;
                TriggerPropertyChanged(nameof(RedumperRereadCount));
            }
        }

        /// <inheritdoc cref="SiteLoginSettings.RetrieveMatchInformation"/>
        public bool RetrieveMatchInformation
        {
            get => Options.Processing.Login.RetrieveMatchInformation;
            set
            {
                Options.Processing.Login.RetrieveMatchInformation = value;
                TriggerPropertyChanged(nameof(RetrieveMatchInformation));
            }
        }

        /// <inheritdoc cref="ProtectionScanningSettings.ScanArchivesForProtection"/>
        public bool ScanArchivesForProtection
        {
            get => Options.Processing.ProtectionScanning.ScanArchivesForProtection;
            set
            {
                Options.Processing.ProtectionScanning.ScanArchivesForProtection = value;
                TriggerPropertyChanged(nameof(ScanArchivesForProtection));
            }
        }

        /// <inheritdoc cref="ProtectionScanningSettings.ScanDumpImageFiles"/>
        public bool ScanDumpImageFiles
        {
            get => Options.Processing.ProtectionScanning.ScanDumpImageFiles;
            set
            {
                Options.Processing.ProtectionScanning.ScanDumpImageFiles = value;
                TriggerPropertyChanged(nameof(ScanDumpImageFiles));
            }
        }

        /// <inheritdoc cref="ProtectionScanningSettings.ScanForProtection"/>
        public bool ScanForProtection
        {
            get => Options.Processing.ProtectionScanning.ScanForProtection;
            set
            {
                Options.Processing.ProtectionScanning.ScanForProtection = value;
                TriggerPropertyChanged(nameof(ScanForProtection));
            }
        }

        /// <inheritdoc cref="ProcessingSettings.ShowDiscEjectReminder"/>
        public bool ShowDiscEjectReminder
        {
            get => Options.Processing.ShowDiscEjectReminder;
            set
            {
                Options.Processing.ShowDiscEjectReminder = value;
                TriggerPropertyChanged(nameof(ShowDiscEjectReminder));
            }
        }

        /// <inheritdoc cref="GuiSettings.SkipSystemDetection"/>
        public bool SkipSystemDetection
        {
            get => Options.GUI.SkipSystemDetection;
            set
            {
                Options.GUI.SkipSystemDetection = value;
                TriggerPropertyChanged(nameof(SkipSystemDetection));
            }
        }

        /// <summary>
        /// Title for the window
        /// </summary>
        public string? Title
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(Title));
            }
        }

        /// <inheritdoc cref="DumpSettings.UseRelativePaths"/>
        public bool UseRelativePaths
        {
            get => Options.Dumping.UseRelativePaths;
            set
            {
                Options.Dumping.UseRelativePaths = value;
                TriggerPropertyChanged(nameof(UseRelativePaths));
            }
        }

        /// <inheritdoc cref="Options.VerboseLogging"/>
        public bool VerboseLogging
        {
            get => Options.VerboseLogging;
            set
            {
                Options.VerboseLogging = value;
                TriggerPropertyChanged(nameof(VerboseLogging));
            }
        }

        #endregion

        #region Lists

        /// <summary>
        /// List of available internal programs
        /// </summary>
        public static List<Element<InternalProgram>> AvailableInternalPrograms => PopulateInternalPrograms();

        /// <summary>
        /// Current list of supported Dreamdump sector orders
        /// </summary>
        public static List<Element<DreamdumpSectorOrder>> DreamdumpSectorOrders => Element<DreamdumpSectorOrder>.GenerateElements();

        /// <summary>
        /// List of available interface languages
        /// </summary>
        public static List<Element<InterfaceLanguage>> InterfaceLanguages => Element<InterfaceLanguage>.GenerateElements();

        /// <summary>
        /// List of available log compression methods
        /// </summary>
        public static List<Element<LogCompression>> LogCompressions => Element<LogCompression>.GenerateElements();

        /// <summary>
        /// Current list of supported Redumper read methods
        /// </summary>
        public static List<Element<RedumperReadMethod>> RedumperReadMethods => Element<RedumperReadMethod>.GenerateElements();

        /// <summary>
        /// Current list of supported Redumper sector orders
        /// </summary>
        public static List<Element<RedumperSectorOrder>> RedumperSectorOrders => Element<RedumperSectorOrder>.GenerateElements();

        /// <summary>
        /// Current list of supported Redumper drive types
        /// </summary>
        public static List<Element<RedumperDriveType>> RedumperDriveTypes => Element<RedumperDriveType>.GenerateElements();

        /// <summary>
        /// Current list of supported system profiles
        /// </summary>
        public static List<PhysicalSystemComboBoxItem> Systems => PhysicalSystemComboBoxItem.GenerateElements();

        #endregion

        #region Constants

        /// <summary>
        /// Ordered set of dumping-supported programs
        /// </summary>
        private static readonly List<InternalProgram> DumpingPrograms =
        [
            InternalProgram.Redumper,
            InternalProgram.DiscImageCreator,
            InternalProgram.Aaru,
            InternalProgram.Dreamdump,
        ];

        #endregion

        /// <summary>
        /// Constructor for pure view model
        /// </summary>
        public OptionsViewModel()
        {
            Options = new Options();
        }

        /// <summary>
        /// Constructor for in-code
        /// </summary>
        public OptionsViewModel(Options baseOptions)
        {
            Options = new Options(baseOptions);
        }

        #region Population

        /// <summary>
        /// Get a complete list of supported internal programs
        /// </summary>
        private static List<Element<InternalProgram>> PopulateInternalPrograms()
            => DumpingPrograms.ConvertAll(ip => new Element<InternalProgram>(ip));

        #endregion

        #region UI Commands

        /// <summary>
        /// Reset Dreamdump non-redump options (Read Method, Sector Order, Drive Type)
        /// </summary>
        public void DreamdumpNonRedumpModeUnChecked()
        {
            Options.Dumping.Dreamdump.SectorOrder = DreamdumpSectorOrder.NONE;
            TriggerPropertyChanged(nameof(Options));
        }

        /// <summary>
        /// Reset and Redumper non-redump options (Read Method, Sector Order, Drive Type)
        /// </summary>
        public void RedumperNonRedumpModeUnChecked()
        {
            Options.Dumping.Redumper.ReadMethod = RedumperReadMethod.NONE;
            Options.Dumping.Redumper.SectorOrder = RedumperSectorOrder.NONE;
            Options.Dumping.Redumper.DriveType = RedumperDriveType.NONE;
            TriggerPropertyChanged(nameof(Options));
        }

        #endregion

        #region Property Updates

        /// <summary>
        /// Trigger a property changed event
        /// </summary>
        private void TriggerPropertyChanged(string propertyName)
        {
            // If the property change event is initialized
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
