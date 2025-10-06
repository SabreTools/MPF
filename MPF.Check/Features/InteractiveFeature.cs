
using System;
using MPF.Frontend;
using SabreTools.RedumpLib;
using SabreTools.RedumpLib.Data;

namespace MPF.Check.Features
{
    internal sealed class InteractiveFeature : SabreTools.CommandLine.Feature
    {
        #region Feature Definition

        public const string DisplayName = "interactive";

        private static readonly string[] _flags = ["i", "interactive"];

        private const string _description = "Enable interactive mode";

        #endregion

        #region Properties

        /// <summary>
        /// Progrma-specific options
        /// </summary>
        public Program.CommandOptions CommandOptions { get; private set; }

        /// <summary>
        /// User-defined options
        /// </summary>
        public Options Options { get; }

        /// <summary>
        /// Currently-selected system
        /// </summary>
        public RedumpSystem? System { get; private set; }

        #endregion

        public InteractiveFeature()
            : base(DisplayName, _flags, _description)
        {
            CommandOptions = new Program.CommandOptions();
            Options = new Options()
            {
                // Internal Program
                InternalProgram = InternalProgram.NONE,

                // Extra Dumping Options
                ScanForProtection = false,
                AddPlaceholders = true,
                PullAllInformation = false,
                AddFilenameSuffix = false,
                OutputSubmissionJSON = false,
                IncludeArtifacts = false,
                CompressLogFiles = false,
                DeleteUnnecessaryFiles = false,
                CreateIRDAfterDumping = false,

                // Protection Scanning Options
                ScanArchivesForProtection = true,
                IncludeDebugProtectionInformation = false,
                HideDriveLetters = false,

                // Redump Login Information
                RetrieveMatchInformation = true,
                RedumpUsername = null,
                RedumpPassword = null,
            };
        }

        /// <inheritdoc/>
        public override bool Execute()
        {
            // Create return values
            CommandOptions = new Program.CommandOptions();
            System = null;

            // These values require multiple parts to be active
            bool scan = false,
                enableArchives = true,
                enableDebug = false,
                hideDriveLetters = false;

            // Create state values
            string? result = string.Empty;

        root:
            Console.Clear();
            Console.WriteLine("MPF.Check Interactive Mode - Main Menu");
            Console.WriteLine("-------------------------");
            Console.WriteLine();
            Console.WriteLine($"1) Set system (Currently '{System}')");
            Console.WriteLine($"2) Set dumping program (Currently '{Options.InternalProgram}')");
            Console.WriteLine($"3) Set seed path (Currently '{CommandOptions.Seed}')");
            Console.WriteLine($"4) Add placeholders (Currently '{Options.AddPlaceholders}')");
            Console.WriteLine($"5) Create IRD (Currently '{Options.CreateIRDAfterDumping}')");
            Console.WriteLine($"6) Attempt Redump matches (Currently '{Options.RetrieveMatchInformation}')");
            Console.WriteLine($"7) Redump credentials (Currently '{Options.RedumpUsername}')");
            Console.WriteLine($"8) Pull all information (Currently '{Options.PullAllInformation}')");
            Console.WriteLine($"9) Set device path (Currently '{CommandOptions.DevicePath}')");
            Console.WriteLine($"A) Scan for protection (Currently '{scan}')");
            Console.WriteLine($"B) Scan archives for protection (Currently '{enableArchives}')");
            Console.WriteLine($"C) Debug protection scan output (Currently '{enableDebug}')");
            Console.WriteLine($"D) Hide drive letters in protection output (Currently '{hideDriveLetters}')");
            Console.WriteLine($"E) Hide filename suffix (Currently '{Options.AddFilenameSuffix}')");
            Console.WriteLine($"F) Output submission JSON (Currently '{Options.OutputSubmissionJSON}')");
            Console.WriteLine($"G) Include JSON artifacts (Currently '{Options.IncludeArtifacts}')");
            Console.WriteLine($"H) Compress logs (Currently '{Options.CompressLogFiles}')");
            Console.WriteLine($"I) Delete unnecessary files (Currently '{Options.DeleteUnnecessaryFiles}')");
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
                    Options.AddPlaceholders = !Options.AddPlaceholders;
                    goto root;
                case "5":
                    Options.CreateIRDAfterDumping = !Options.CreateIRDAfterDumping;
                    goto root;
                case "6":
                    Options.RetrieveMatchInformation = !Options.RetrieveMatchInformation;
                    goto root;
                case "7":
                    goto redumpCredentials;
                case "8":
                    Options.PullAllInformation = !Options.PullAllInformation;
                    goto root;
                case "9":
                    goto devicePath;
                case "a":
                case "A":
                    scan = !scan;
                    goto root;
                case "b":
                case "B":
                    enableArchives = !enableArchives;
                    goto root;
                case "c":
                case "C":
                    enableDebug = !enableDebug;
                    goto root;
                case "d":
                case "D":
                    hideDriveLetters = !hideDriveLetters;
                    goto root;
                case "e":
                case "E":
                    Options.AddFilenameSuffix = !Options.AddFilenameSuffix;
                    goto root;
                case "f":
                case "F":
                    Options.OutputSubmissionJSON = !Options.OutputSubmissionJSON;
                    goto root;
                case "g":
                case "G":
                    Options.IncludeArtifacts = !Options.IncludeArtifacts;
                    goto root;
                case "h":
                case "H":
                    Options.CompressLogFiles = !Options.CompressLogFiles;
                    goto root;
                case "i":
                case "I":
                    Options.DeleteUnnecessaryFiles = !Options.DeleteUnnecessaryFiles;
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
            Console.WriteLine("Input the system and press Enter:");
            Console.Write("> ");
            result = Console.ReadLine();
            System = Extensions.ToRedumpSystem(result);
            goto root;

        dumpingProgram:
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
            CommandOptions.Seed = Builder.CreateFromFile(result);
            goto root;

        redumpCredentials:
            Console.WriteLine();
            Console.WriteLine("Enter your Redumper username and press Enter:");
            Console.Write("> ");
            Options.RedumpUsername = Console.ReadLine();

            Console.WriteLine("Enter your Redumper password (hidden) and press Enter:");
            Console.Write("> ");
            Options.RedumpPassword = string.Empty;
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                    break;

                Options.RedumpPassword += key.KeyChar;
            }

            goto root;

        devicePath:
            Console.WriteLine();
            Console.WriteLine("Input the device path and press Enter:");
            Console.Write("> ");
            CommandOptions.DevicePath = Console.ReadLine();
            goto root;

        exit:
            // Now deal with the complex options
            Options.ScanForProtection = scan && !string.IsNullOrEmpty(CommandOptions.DevicePath);
            Options.ScanArchivesForProtection = enableArchives && scan && !string.IsNullOrEmpty(CommandOptions.DevicePath);
            Options.IncludeDebugProtectionInformation = enableDebug && scan && !string.IsNullOrEmpty(CommandOptions.DevicePath);
            Options.HideDriveLetters = hideDriveLetters && scan && !string.IsNullOrEmpty(CommandOptions.DevicePath);

            return true;
        }

        /// <inheritdoc/>
        public override bool VerifyInputs() => true;
    }
}
