using System;
using MPF.Frontend;
using MPF.Frontend.Tools;
using SabreTools.RedumpLib.Data;

namespace MPF.Frontend.Features
{
    public class ListConfigFeature : SabreTools.CommandLine.Feature
    {
        #region Feature Definition

        public const string DisplayName = "listconfig";

        private static readonly string[] _flags = ["lo", "listconfig"];

        private const string _description = "List current configuration values";

        #endregion

        public ListConfigFeature()
            : base(DisplayName, _flags, _description)
        {
        }

        /// <inheritdoc/>
        public override bool Execute()
        {
            // Try to load the current config
            var options = OptionsLoader.LoadFromConfig();
            if (options.FirstRun)
            {
                Console.WriteLine("No valid configuration found!");
                return true;
            }

            // Paths
            Console.WriteLine("Paths:");
            Console.WriteLine($"  Aaru Path = {options.AaruPath}");
            Console.WriteLine($"  DiscImageCreator Path = {options.DiscImageCreatorPath}");
            Console.WriteLine($"  Redumper Path = {options.RedumperPath}");
            Console.WriteLine($"  Default Program = {options.InternalProgram.LongName()}");
            Console.WriteLine();

            // UI Defaults
            Console.WriteLine("UI Defaults:");
            Console.WriteLine($"  Dark Mode = {options.EnableDarkMode}");
            Console.WriteLine($"  Purp Mode = {options.EnablePurpMode}");
            Console.WriteLine($"  Custom Background Color = {options.CustomBackgroundColor}");
            Console.WriteLine($"  Custom Text Color = {options.CustomTextColor}");
            Console.WriteLine($"  Check for Updates on Startup = {options.CheckForUpdatesOnStartup}");
            Console.WriteLine($"  Copy Update URL to Clipboard = {options.CopyUpdateUrlToClipboard}");
            Console.WriteLine($"  Fast Label Update = {options.FastUpdateLabel}");
            Console.WriteLine($"  Default Interface Language = {options.DefaultInterfaceLanguage.LongName()}");
            Console.WriteLine($"  Default Output Path = {options.DefaultOutputPath}");
            Console.WriteLine($"  Default System = {options.DefaultSystem.LongName()}");
            Console.WriteLine($"  Show Debug Menu Item = {options.ShowDebugViewMenuItem}");
            Console.WriteLine();

            // Dumping Speeds
            Console.WriteLine("Dumping Speeds:");
            Console.WriteLine($"  Default CD Speed = {options.PreferredDumpSpeedCD}");
            Console.WriteLine($"  Default DVD Speed = {options.PreferredDumpSpeedDVD}");
            Console.WriteLine($"  Default HD-DVD Speed = {options.PreferredDumpSpeedHDDVD}");
            Console.WriteLine($"  Default Blu-ray Speed = {options.PreferredDumpSpeedBD}");
            Console.WriteLine();

            // Aaru
            Console.WriteLine("Aaru-Specific Options:");
            Console.WriteLine($"  Enable Debug = {options.AaruEnableDebug}");
            Console.WriteLine($"  Enable Verbose = {options.AaruEnableVerbose}");
            Console.WriteLine($"  Force Dumping = {options.AaruForceDumping}");
            Console.WriteLine($"  Reread Count = {options.AaruRereadCount}");
            Console.WriteLine($"  Strip Personal Data = {options.AaruStripPersonalData}");
            Console.WriteLine();

            // DiscImageCreator
            Console.WriteLine("DiscImageCreator-Specific Options:");
            Console.WriteLine($"  Multi-Sector Read Flag = {options.DICMultiSectorRead}");
            Console.WriteLine($"  Multi-Sector Read Value = {options.DICMultiSectorReadValue}");
            Console.WriteLine($"  Overly-Secure Flags = {options.DICParanoidMode}");
            Console.WriteLine($"  Quiet Flag = {options.DICQuietMode}");
            Console.WriteLine($"  CD Reread Count = {options.DICRereadCount}");
            Console.WriteLine($"  DVD Reread Count = {options.DICDVDRereadCount}");
            Console.WriteLine($"  Use CMI Flag = {options.DICUseCMIFlag}");
            Console.WriteLine();

            // Redumper
            Console.WriteLine("Redumper-Specific Options:");
            Console.WriteLine($"  Enable Skeleton = {options.RedumperEnableSkeleton}");
            Console.WriteLine($"  Enable Verbose = {options.RedumperEnableVerbose}");
            Console.WriteLine($"  Lead-in Retry Count = {options.RedumperLeadinRetryCount}");
            Console.WriteLine($"  Non-Redump Mode = {options.RedumperNonRedumpMode}");
            Console.WriteLine($"  Drive Type = {options.RedumperDriveType.LongName()}");
            Console.WriteLine($"  Read Method = {options.RedumperReadMethod.LongName()}");
            Console.WriteLine($"  Sector Order = {options.RedumperSectorOrder.LongName()}");
            Console.WriteLine($"  Reread Count = {options.RedumperRereadCount}");
            Console.WriteLine($"  Refine Sector Mode = {options.RedumperRefineSectorMode}");
            Console.WriteLine();

            // Extra Dumping Options
            Console.WriteLine("Extra Dumping Options:");
            Console.WriteLine($"  Scan for Protection = {options.ScanForProtection}");
            Console.WriteLine($"  Add Placeholders = {options.AddPlaceholders}");
            Console.WriteLine($"  Prompt for Media Information = {options.PromptForDiscInformation}");
            Console.WriteLine($"  Pull All Information = {options.PullAllInformation}");
            Console.WriteLine($"  Enable Tabs in Input Fields = {options.EnableTabsInInputFields}");
            Console.WriteLine($"  Enable Redump Compatibility = {options.EnableRedumpCompatibility}");
            Console.WriteLine($"  Show Disc Eject Reminder = {options.ShowDiscEjectReminder}");
            Console.WriteLine($"  Ignore Fixed Drives = {options.IgnoreFixedDrives}");
            Console.WriteLine($"  Add Filename Suffix = {options.AddFilenameSuffix}");
            Console.WriteLine($"  Output Submission JSON = {options.OutputSubmissionJSON}");
            Console.WriteLine($"  Include Artifacts = {options.IncludeArtifacts}");
            Console.WriteLine($"  Compress Log Files = {options.CompressLogFiles}");
            Console.WriteLine($"  Log Compression = {options.LogCompression.LongName()}");
            Console.WriteLine($"  Delete Unnecessary Files = {options.DeleteUnnecessaryFiles}");
            Console.WriteLine($"  Create IRD After Dumping = {options.CreateIRDAfterDumping}");
            Console.WriteLine();

            // Skip Options
            Console.WriteLine("Skip Options:");
            Console.WriteLine($"  Skip System Detection = {options.SkipSystemDetection}");
            Console.WriteLine();

            // Protection Scanning Options
            Console.WriteLine("Protection Scanning Options:");
            Console.WriteLine($"  Scan Archives for Protection = {options.ScanArchivesForProtection}");
            Console.WriteLine($"  Include Debug Protection Information = {options.IncludeDebugProtectionInformation}");
            Console.WriteLine($"  Hide Drive Letters = {options.HideDriveLetters}");
            Console.WriteLine();

            // Logging Options
            Console.WriteLine("Logging Options:");
            Console.WriteLine($"  Verbose Logging = {options.VerboseLogging}");
            Console.WriteLine($"  Open Log Window at Startup = {options.OpenLogWindowAtStartup}");
            Console.WriteLine();

            // Redump Login Information
            Console.WriteLine("Redump Login Information:");
            Console.WriteLine($"  Retrieve Match Information = {options.RetrieveMatchInformation}");
            Console.WriteLine($"  Redump Username = {options.RedumpUsername}");
            Console.WriteLine($"  Redump Password = {(string.IsNullOrEmpty(options.RedumpPassword) ? "[UNSET]" : "[SET]")}");
            Console.WriteLine();

            return true;
        }

        /// <inheritdoc/>
        public override bool VerifyInputs() => true;
    }
}
