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

            // Root-level
            Console.WriteLine($"Configuration Version = {options.Version}");
            Console.WriteLine($"Check for Updates on Startup = {options.CheckForUpdatesOnStartup}");
            Console.WriteLine($"Verbose Logging = {options.VerboseLogging}");
            Console.WriteLine();

            // GUI
            Console.WriteLine("GUI:");
            Console.WriteLine($"  Copy Update URL to Clipboard = {options.GUI.CopyUpdateUrlToClipboard}");
            Console.WriteLine($"  Default Interface Language = {options.GUI.DefaultInterfaceLanguage.LongName()}");
            Console.WriteLine($"  Show Debug Menu Item = {options.GUI.ShowDebugViewMenuItem}");
            Console.WriteLine($"  Open Log Window at Startup = {options.GUI.OpenLogWindowAtStartup}");
            Console.WriteLine("  Theming:");
            Console.WriteLine($"    Dark Mode = {options.GUI.Theming.EnableDarkMode}");
            Console.WriteLine($"    Purp Mode = {options.GUI.Theming.EnablePurpMode}");
            Console.WriteLine($"    Custom Background Color = {options.GUI.Theming.CustomBackgroundColor}");
            Console.WriteLine($"    Custom Text Color = {options.GUI.Theming.CustomTextColor}");
            Console.WriteLine($"  Fast Label Update = {options.GUI.FastUpdateLabel}");
            Console.WriteLine($"  Ignore Fixed Drives = {options.GUI.IgnoreFixedDrives}");
            Console.WriteLine($"  Skip System Detection = {options.GUI.SkipSystemDetection}");
            Console.WriteLine();

            // Dump Paths
            Console.WriteLine("Dump Paths:");
            Console.WriteLine($"  Aaru Path = {options.Dumping.AaruPath}");
            Console.WriteLine($"  DiscImageCreator Path = {options.Dumping.DiscImageCreatorPath}");
            Console.WriteLine($"  Dreamdump Path = {options.Dumping.DreamdumpPath}");
            Console.WriteLine($"  Redumper Path = {options.Dumping.RedumperPath}");
            Console.WriteLine();

            // Dump Defaults
            Console.WriteLine("Dump Defaults:");
            Console.WriteLine($"  Default Program = {options.InternalProgram.LongName()}");
            Console.WriteLine($"  Default Output Path = {options.Dumping.DefaultOutputPath}");
            Console.WriteLine($"  Default System = {options.Dumping.DefaultSystem.LongName()}");
            Console.WriteLine();

            // Dumping Speeds
            Console.WriteLine("Dumping Speeds:");
            Console.WriteLine($"  Default CD Speed = {options.Dumping.DumpSpeeds.PreferredCD}");
            Console.WriteLine($"  Default DVD Speed = {options.Dumping.DumpSpeeds.PreferredDVD}");
            Console.WriteLine($"  Default HD-DVD Speed = {options.Dumping.DumpSpeeds.PreferredHDDVD}");
            Console.WriteLine($"  Default Blu-ray Speed = {options.Dumping.DumpSpeeds.PreferredBD}");
            Console.WriteLine();

            // Aaru
            Console.WriteLine("Aaru-Specific Options:");
            Console.WriteLine($"  Enable Debug = {options.Dumping.Aaru.EnableDebug}");
            Console.WriteLine($"  Enable Verbose = {options.Dumping.Aaru.EnableVerbose}");
            Console.WriteLine($"  Force Dumping = {options.Dumping.Aaru.ForceDumping}");
            Console.WriteLine($"  Reread Count = {options.Dumping.Aaru.RereadCount}");
            Console.WriteLine($"  Strip Personal Data = {options.Dumping.Aaru.StripPersonalData}");
            Console.WriteLine();

            // DiscImageCreator
            Console.WriteLine("DiscImageCreator-Specific Options:");
            Console.WriteLine($"  Multi-Sector Read Flag = {options.Dumping.DIC.MultiSectorRead}");
            Console.WriteLine($"  Multi-Sector Read Value = {options.Dumping.DIC.MultiSectorReadValue}");
            Console.WriteLine($"  Overly-Secure Flags = {options.Dumping.DIC.ParanoidMode}");
            Console.WriteLine($"  Quiet Flag = {options.Dumping.DIC.QuietMode}");
            Console.WriteLine($"  CD Reread Count = {options.Dumping.DIC.RereadCount}");
            Console.WriteLine($"  DVD Reread Count = {options.Dumping.DIC.DVDRereadCount}");
            Console.WriteLine($"  Use CMI Flag = {options.Dumping.DIC.UseCMIFlag}");
            Console.WriteLine();

            // Dreamdump
            Console.WriteLine("Dreamdump-Specific Options:");
            Console.WriteLine($"  Non-Redump Mode = {options.Dumping.Dreamdump.NonRedumpMode}");
            Console.WriteLine($"  Sector Order = {options.Dumping.Dreamdump.SectorOrder.LongName()}");
            Console.WriteLine($"  Reread Count = {options.Dumping.Dreamdump.RereadCount}");
            Console.WriteLine();

            // Redumper
            Console.WriteLine("Redumper-Specific Options:");
            Console.WriteLine($"  Enable Skeleton = {options.Dumping.Redumper.EnableSkeleton}");
            Console.WriteLine($"  Enable Verbose = {options.Dumping.Redumper.EnableVerbose}");
            Console.WriteLine($"  Lead-in Retry Count = {options.Dumping.Redumper.LeadinRetryCount}");
            Console.WriteLine($"  Non-Redump Mode = {options.Dumping.Redumper.NonRedumpMode}");
            Console.WriteLine($"  Drive Type = {options.Dumping.Redumper.DriveType.LongName()}");
            Console.WriteLine($"  Read Method = {options.Dumping.Redumper.ReadMethod.LongName()}");
            Console.WriteLine($"  Sector Order = {options.Dumping.Redumper.SectorOrder.LongName()}");
            Console.WriteLine($"  Reread Count = {options.Dumping.Redumper.RereadCount}");
            Console.WriteLine($"  Refine Sector Mode = {options.Dumping.Redumper.RefineSectorMode}");
            Console.WriteLine();

            // Protection Scanning Options
            Console.WriteLine("Protection Scanning Options:");
            Console.WriteLine($"  Scan for Protection = {options.Processing.ProtectionScanning.ScanForProtection}");
            Console.WriteLine($"  Scan Archives for Protection = {options.Processing.ProtectionScanning.ScanArchivesForProtection}");
            Console.WriteLine($"  Include Debug Protection Information = {options.Processing.ProtectionScanning.IncludeDebugProtectionInformation}");
            Console.WriteLine($"  Hide Drive Letters = {options.Processing.ProtectionScanning.HideDriveLetters}");
            Console.WriteLine();

            // Redump Login Information
            Console.WriteLine("Redump Login Information:");
            Console.WriteLine($"  Retrieve Match Information = {options.Processing.Login.RetrieveMatchInformation}");
            Console.WriteLine($"  Redump Username = {options.Processing.Login.RedumpUsername}");
            Console.WriteLine($"  Redump Password = {(string.IsNullOrEmpty(options.Processing.Login.RedumpPassword) ? "[UNSET]" : "[SET]")}");
            Console.WriteLine();

            // Media Information
            Console.WriteLine("Media Information:");
            Console.WriteLine($"  Add Placeholders = {options.Processing.MediaInformation.AddPlaceholders}");
            Console.WriteLine($"  Prompt for Media Information = {options.Processing.MediaInformation.PromptForDiscInformation}");
            Console.WriteLine($"  Pull All Information = {options.Processing.MediaInformation.PullAllInformation}");
            Console.WriteLine($"  Enable Tabs in Input Fields = {options.Processing.MediaInformation.EnableTabsInInputFields}");
            Console.WriteLine($"  Enable Redump Compatibility = {options.Processing.MediaInformation.EnableRedumpCompatibility}");
            Console.WriteLine();

            // Output Options
            Console.WriteLine("Output Options:");
            Console.WriteLine($"  Show Disc Eject Reminder = {options.Processing.ShowDiscEjectReminder}");
            Console.WriteLine($"  Add Filename Suffix = {options.Processing.AddFilenameSuffix}");
            Console.WriteLine($"  Create IRD After Dumping = {options.Processing.CreateIRDAfterDumping}");
            Console.WriteLine($"  Output Submission JSON = {options.Processing.OutputSubmissionJSON}");
            Console.WriteLine($"  Include Artifacts = {options.Processing.IncludeArtifacts}");
            Console.WriteLine($"  Compress Log Files = {options.Processing.CompressLogFiles}");
            Console.WriteLine($"  Log Compression = {options.Processing.LogCompression.LongName()}");
            Console.WriteLine($"  Delete Unnecessary Files = {options.Processing.DeleteUnnecessaryFiles}");
            Console.WriteLine();

            return true;
        }

        /// <inheritdoc/>
        public override bool VerifyInputs() => true;
    }
}
