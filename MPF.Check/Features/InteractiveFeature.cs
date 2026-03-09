
using System;
using MPF.Frontend;
using MPF.Frontend.Tools;
using SabreTools.RedumpLib;
using SabreTools.RedumpLib.Data;
using LogCompression = MPF.Processors.LogCompression;

namespace MPF.Check.Features
{
    internal sealed class InteractiveFeature : BaseFeature
    {
        #region Feature Definition

        public const string DisplayName = "interactive";

        private static readonly string[] _flags = ["i", "interactive"];

        private const string _description = "Enable interactive mode";

        #endregion

        public InteractiveFeature()
            : base(DisplayName, _flags, _description)
        {
        }

        /// <inheritdoc/>
        public override bool ProcessArgs(string[] args, int index)
        {
            // Cache all args as inputs
            for (int i = 1; i < args.Length; i++)
            {
                Inputs.Add(args[i]);
            }

            // Read the options from config, if possible
            Options = OptionsLoader.LoadFromConfig();
            if (Options.FirstRun)
            {
                Options = new Options();

                // Internal Program
                Options.InternalProgram = InternalProgram.NONE;

                // Protection Scanning Options
                Options.Processing.ProtectionScanning.ScanForProtection = false;
                Options.Processing.ProtectionScanning.ScanArchivesForProtection = true;
                Options.Processing.ProtectionScanning.IncludeDebugProtectionInformation = false;
                Options.Processing.ProtectionScanning.HideDriveLetters = false;

                // Redump Login Information
                Options.Processing.Login.PullAllInformation = false;
                Options.Processing.Login.RedumpUsername = null;
                Options.Processing.Login.RedumpPassword = null;
                Options.Processing.Login.RetrieveMatchInformation = true;
                Options.Processing.Login.AttemptCount = 3;
                Options.Processing.Login.TimeoutSeconds = 30;

                // Media Information
                Options.Processing.MediaInformation.AddPlaceholders = true;

                // Post-Information Options
                Options.Processing.AddFilenameSuffix = false;
                Options.Processing.CreateIRDAfterDumping = false;
                Options.Processing.OutputSubmissionJSON = false;
                Options.Processing.IncludeArtifacts = false;
                Options.Processing.CompressLogFiles = false;
                Options.Processing.LogCompression = LogCompression.DeflateMaximum;
                Options.Processing.DeleteUnnecessaryFiles = false;
            }

            // Create return values
            System = null;

            // These values require multiple parts to be active
            bool scan = false,
                enableArchives = true,
                enableDebug = false,
                hideDriveLetters = false;

            // Create state values
            string? result;

        root:
            Console.Clear();
            Console.WriteLine("MPF.Check Interactive Mode - Main Menu");
            Console.WriteLine("-------------------------");
            Console.WriteLine();
            Console.WriteLine($"1) Set system (Currently '{System}')");
            Console.WriteLine($"2) Set dumping program (Currently '{Options.InternalProgram}')");
            Console.WriteLine($"3) Set seed path (Currently '{Seed}')");
            Console.WriteLine($"4) Add placeholders (Currently '{Options.Processing.MediaInformation.AddPlaceholders}')");
            Console.WriteLine($"5) Create IRD (Currently '{Options.Processing.CreateIRDAfterDumping}')");
            Console.WriteLine($"6) Attempt Redump matches (Currently '{Options.Processing.Login.RetrieveMatchInformation}')");
            Console.WriteLine($"7) Redump credentials (Currently '{Options.Processing.Login.RedumpUsername}')");
            Console.WriteLine($"8) Redump client attempt count (Currently '{Options.Processing.Login.AttemptCount}')");
            Console.WriteLine($"9) Redump client timeout in seconds (Currently '{Options.Processing.Login.TimeoutSeconds}')");
            Console.WriteLine($"A) Pull all information (Currently '{Options.Processing.Login.PullAllInformation}')");
            Console.WriteLine($"B) Set device path (Currently '{DevicePath}')");
            Console.WriteLine($"C) Scan for protection (Currently '{scan}')");
            Console.WriteLine($"D) Scan archives for protection (Currently '{enableArchives}')");
            Console.WriteLine($"E) Debug protection scan output (Currently '{enableDebug}')");
            Console.WriteLine($"F) Hide drive letters in protection output (Currently '{hideDriveLetters}')");
            Console.WriteLine($"G) Hide filename suffix (Currently '{Options.Processing.AddFilenameSuffix}')");
            Console.WriteLine($"H) Output submission JSON (Currently '{Options.Processing.OutputSubmissionJSON}')");
            Console.WriteLine($"I) Include JSON artifacts (Currently '{Options.Processing.IncludeArtifacts}')");
            Console.WriteLine($"J) Compress logs (Currently '{Options.Processing.CompressLogFiles}')");
            Console.WriteLine($"K) Log compression (Currently '{Options.Processing.LogCompression.LongName()}')");
            Console.WriteLine($"L) Delete unnecessary files (Currently '{Options.Processing.DeleteUnnecessaryFiles}')");
            Console.WriteLine();
            Console.WriteLine($"Q) Exit the program");
            Console.WriteLine($"X) Start checking");
            Console.Write("> ");

            result = Console.ReadLine();
            switch (result)
            {
                case "1":
                    goto system;
                case "2":
                    goto dumpingProgram;
                case "3":
                    goto seedPath;
                case "4":
                    Options.Processing.MediaInformation.AddPlaceholders = !Options.Processing.MediaInformation.AddPlaceholders;
                    goto root;
                case "5":
                    Options.Processing.CreateIRDAfterDumping = !Options.Processing.CreateIRDAfterDumping;
                    goto root;
                case "6":
                    Options.Processing.Login.RetrieveMatchInformation = !Options.Processing.Login.RetrieveMatchInformation;
                    goto root;
                case "7":
                    goto redumpCredentials;
                case "8":
                    goto attemptCount;
                case "9":
                    goto timeoutSeconds;
                case "a":
                case "A":
                    Options.Processing.Login.PullAllInformation = !Options.Processing.Login.PullAllInformation;
                    goto root;
                case "b":
                case "B":
                    goto devicePath;
                case "c":
                case "C":
                    scan = !scan;
                    goto root;
                case "d":
                case "D":
                    enableArchives = !enableArchives;
                    goto root;
                case "e":
                case "E":
                    enableDebug = !enableDebug;
                    goto root;
                case "f":
                case "F":
                    hideDriveLetters = !hideDriveLetters;
                    goto root;
                case "g":
                case "G":
                    Options.Processing.AddFilenameSuffix = !Options.Processing.AddFilenameSuffix;
                    goto root;
                case "h":
                case "H":
                    Options.Processing.OutputSubmissionJSON = !Options.Processing.OutputSubmissionJSON;
                    goto root;
                case "i":
                case "I":
                    Options.Processing.IncludeArtifacts = !Options.Processing.IncludeArtifacts;
                    goto root;
                case "j":
                case "J":
                    Options.Processing.CompressLogFiles = !Options.Processing.CompressLogFiles;
                    goto root;
                case "k":
                case "K":
                    goto logCompression;
                case "l":
                case "L":
                    Options.Processing.DeleteUnnecessaryFiles = !Options.Processing.DeleteUnnecessaryFiles;
                    goto root;

                case "q":
                case "Q":
                    Environment.Exit(0);
                    break;
                case "x":
                case "X":
                    Console.Clear();
                    goto exit;
                case "z":
                case "Z":
                    Console.WriteLine("It is pitch black. You are likely to be eaten by a grue.");
                    Console.Write("> ");
                    Console.ReadLine();
                    goto root;
                default:
                    Console.WriteLine($"Invalid selection: {result}");
                    Console.ReadLine();
                    goto root;
            }

        system:
            Console.WriteLine();
            Console.WriteLine("For possible inputs, use the List Systems commandline option");
            Console.WriteLine();
            Console.WriteLine("Input the system and press Enter:");
            Console.Write("> ");
            result = Console.ReadLine();
            System = result.ToRedumpSystem();
            goto root;

        dumpingProgram:
            Console.WriteLine();
            Console.WriteLine("Options:");
            foreach (var program in (InternalProgram[])Enum.GetValues(typeof(InternalProgram)))
            {
                // Skip the placeholder values
                if (program == InternalProgram.NONE)
                    continue;

                Console.WriteLine($"{program.ToString().ToLowerInvariant(),-15} => {program.LongName()}");
            }

            Console.WriteLine();
            Console.WriteLine("Input the dumping program and press Enter:");
            Console.Write("> ");
            result = Console.ReadLine();
            Options.InternalProgram = result.ToInternalProgram();
            goto root;

        seedPath:
            Console.WriteLine();
            Console.WriteLine("Input the seed path and press Enter:");
            Console.Write("> ");
            result = Console.ReadLine();
            Seed = Builder.CreateFromFile(result);
            goto root;

        redumpCredentials:
            Console.WriteLine();
            Console.WriteLine("Enter your Redump username and press Enter:");
            Console.Write("> ");
            Options.Processing.Login.RedumpUsername = Console.ReadLine();

            Console.WriteLine("Enter your Redump password (hidden) and press Enter:");
            Console.Write("> ");
            Options.Processing.Login.RedumpPassword = string.Empty;
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                    break;

                Options.Processing.Login.RedumpPassword += key.KeyChar;
            }

            goto root;

        attemptCount:
            Console.WriteLine();
            Console.WriteLine("Enter your attempt count and press Enter:");
            Console.Write("> ");
            string possibleAttemptCount = Console.ReadLine();
            if (int.TryParse(possibleAttemptCount, out int attemptCount) && attemptCount > 0)
                Options.Processing.Login.AttemptCount = attemptCount;

            goto root;

        timeoutSeconds:
            Console.WriteLine();
            Console.WriteLine("Enter your timeout in seconds and press Enter:");
            Console.Write("> ");
            string possibleTimeout = Console.ReadLine();
            if (int.TryParse(possibleTimeout, out int timeoutSeconds) && timeoutSeconds > 0)
                Options.Processing.Login.TimeoutSeconds = timeoutSeconds;

            goto root;

        devicePath:
            Console.WriteLine();
            Console.WriteLine("Input the device path and press Enter:");
            Console.Write("> ");
            DevicePath = Console.ReadLine();
            goto root;

        logCompression:
            Console.WriteLine();
            Console.WriteLine("Options:");
            foreach (var compressionType in (LogCompression[])Enum.GetValues(typeof(LogCompression)))
            {
                Console.WriteLine($"{compressionType.ToString().ToLowerInvariant(),-15} => {compressionType.LongName()}");
            }

            Console.WriteLine();
            Console.WriteLine("Input the log compression type and press Enter:");
            Console.Write("> ");
            result = Console.ReadLine();
            Options.Processing.LogCompression = result.ToLogCompression();
            goto root;

        exit:
            // Now deal with the complex options
            Options.Processing.ProtectionScanning.ScanForProtection = scan && !string.IsNullOrEmpty(DevicePath);
            Options.Processing.ProtectionScanning.ScanArchivesForProtection = enableArchives && scan && !string.IsNullOrEmpty(DevicePath);
            Options.Processing.ProtectionScanning.IncludeDebugProtectionInformation = enableDebug && scan && !string.IsNullOrEmpty(DevicePath);
            Options.Processing.ProtectionScanning.HideDriveLetters = hideDriveLetters && scan && !string.IsNullOrEmpty(DevicePath);

            return true;
        }

        /// <inheritdoc/>
        public override bool VerifyInputs() => Inputs.Count > 0;
    }
}
